using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using Crews.API.Controllers;
using Crews.API.Data;
using Crews.API.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
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

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
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
