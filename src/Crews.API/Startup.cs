using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Crews.API.Controllers;
using Crews.API.Data;
using Crews.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Crews.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfig(Configuration);
            services.AddIdentity<User, Role>(
                options =>
                {
                    //options.SignIn.RequireConfirmedAccount = true;
                    options.User.RequireUniqueEmail = true;
                    options.Password.RequiredLength = 5;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequiredUniqueChars = 1;
                }).AddEntityFrameworkStores<OneCrewContext>();

            services.AddAuthentication()
                .AddCookie() // <= DEFAULT
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = Configuration["Token:Issuer"],
                        ValidAudience = Configuration["Token:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:Key"]))
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("SpecialUser", policy => policy.AddRequirements(new SpecialUserRequirement("lukas.szumylo@gmail.com")));
                options.AddPolicy("IsAdmin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("CanAddTraining", policy => policy.RequireClaim("Permission", "AddTraining"));
            });

            services.AddDbContext<OneCrewContext>();
            services.AddSingleton<IAuthorizationHandler, SpecialUserAuthorizationHandler>();
            services.AddScoped<IOneCrewRepository, OneCrewRepository>();
            services.AddScoped<IUserClaimsPrincipalFactory<User>, UserClaimsPrincipalFactory<User, Role>>();
            services.AddAutoMapper(Assembly.GetEntryAssembly());

            services.AddApiVersioning(options =>
            {
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;

                // FROM QueryString
                //option.ApiVersionReader = new QueryStringApiVersionReader(); // <- DEFAULT with api-version={version}
                //option.ApiVersionReader = new QueryStringApiVersionReader("ver"); // <- DEFAULT overriden with ver={version}

                // FROM Header Or/And QueryString
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new HeaderApiVersionReader("X-Version"),
                    new QueryStringApiVersionReader("ver", "version"));

                // FROM URL
                //option.ApiVersionReader = new UrlSegmentApiVersionReader();

                // Configure controller here instead of using attributes inside controller class
                options.Conventions.Controller<TrainingsController>()
                    .HasApiVersion(1, 0)
                    .HasApiVersion(1, 1)
                    .Action(typeof(TrainingsController).GetMethod(nameof(TrainingsController.Delete))!)
                    .MapToApiVersion(1, 1);
            });

            services
                .AddControllers(options =>
                {
                    // You can apply [Authorize] to all controllers globally - then you need to use 
                    // [AllowAnonymous] attribute on those Actions which can be accessed but anonymous user
                    //options.Filters.Add(new AuthorizeFilter());
                })
                .AddJsonOptions(options =>
                {
                    var enumConverter = new JsonStringEnumConverter();
                    options.JsonSerializerOptions.Converters.Add(enumConverter);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(OnStarted);
            appLifetime.ApplicationStopping.Register(OnStopping);
            appLifetime.ApplicationStopped.Register(OnStopped);

            if (app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope() is var serviceScope)
            {
                var context = serviceScope!.ServiceProvider.GetRequiredService<OneCrewContext>();
                context.Database.EnsureCreated();
            }

            // Custom middleware that runs at the start of the pipeline
            app.UseWhen(context => context.Request.Query.ContainsKey("branch"), HandleBranchAndRejoin);

            // Middleware registration - order is important
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/middleware-pipeline.svg?view=aspnetcore-6.0
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpLogging();
            app.UseHttpsRedirection(); // Redirects HTTP requests to HTTPS
                                       //app.UseStaticFiles();    // Returns static files and finish request processing - no authorization checks.
                                       //In this order, static files are not compressed

            //app.UseCookiePolicy();   // Conforms the app to GDPR (RODO) regulations
            //app.UseHttpLogging();    // Logs info about HTTP request & response

            app.UseRouting();          // Routes requests

            // Must appear before any middleware that might check the request culture
            //app.UseRequestLocalization(); 

            // Those 3 needs to be in exact order
            //app.UseCors(); // Configure Cross Origin Resource Sharing - Must appear before UseResponseCaching
            app.UseAuthentication();   // Authenticates the user right before to access secure resource
            app.UseAuthorization();    // Authorize a user to access secure resource

            //app.UseSession();        // Establishes and maintains session state

            // Those 2 can be with different ordering according to specific scenario
            //app.UseResponseCompression(); 
            //app.UseResponseCaching();

            // Adds endpoints (controllers) to the request pipeline
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/mvc-endpoint.svg?view=aspnetcore-6.0
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void HandleBranchAndRejoin(IApplicationBuilder app)
        {
            var logger = app.ApplicationServices.GetRequiredService<ILogger<Program>>();

            app.Use(async (context, next) =>
            {
                // STEPS
                // 1. User calls http://localhost:6600/api/crews?branch=main
                // 2. Configured app.UseWhen detects branch query string and calls HandleBranchAndRejoin method
                // 3. Everything before await next(context) is called right before calling Controller action (CrewsController)
                // 4. Everything after await next(context) is called right after calling Controller action (CrewsController)

                // The logic below can be achieved also by app.
                Stream originalBody = context.Response.Body;

                try
                {
                    var branchVer = context.Request.Query["branch"];
                    string requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    logger.LogInformation($"[BEFORE call main pipeline] Branch used = {branchVer} \n {requestBody}", branchVer);

                    await using var memStream = new MemoryStream();
                    context.Response.Body = memStream;

                    // Do work that doesn't write to the Response.
                    // Re-join to the main pipeline
                    await next(context);

                    // Do other work that doesn't write to the Response.
                    memStream.Position = 0;
                    string responseBody = await new StreamReader(memStream).ReadToEndAsync();

                    memStream.Position = 0;
                    await memStream.CopyToAsync(originalBody);

                    logger.LogInformation($"[AFTER after call main pipeline] Branch used = {branchVer} \n {responseBody}", branchVer);
                }
                finally
                {
                    context.Response.Body = originalBody;
                }
            });
        }

        private void OnStarted()
        {
            // Perform post-startup activities here
        }

        private void OnStopping()
        {
            // Perform on-stopping activities here
        }

        private void OnStopped()
        {
            // Perform post-stopped activities here
        }
    }
}
