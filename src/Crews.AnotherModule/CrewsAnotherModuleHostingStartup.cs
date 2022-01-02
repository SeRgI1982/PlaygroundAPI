using Crews.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly:HostingStartup(typeof(Crews.AnotherModule.CrewsAnotherModuleHostingStartup))]
namespace Crews.AnotherModule
{
    public class CrewsAnotherModuleHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IModule, CrewsAnotherModule>();
            });
        }
    }
}

