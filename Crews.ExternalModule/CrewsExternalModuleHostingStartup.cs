using System.Collections.Generic;
using Crews.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly:HostingStartup(typeof(Crews.ExternalModule.CrewsExternalModuleHostingStartup))]
namespace Crews.ExternalModule
{
    public class CrewsExternalModuleHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(config =>
            {
                var rolesDescription = new Dictionary<string, string>
                {
                    { "Roles:1|Follower", "Follows the crew" },
                    { "Roles:2|Fan", "Follows the crew. " +
                             "Can see internal crew data like trainings, upcoming events, historical matches" },
                    { "Roles:3|Assistant", "Follows the crew. " +
                                   "Can see internal crew data like trainings, upcoming events, historical matches. " +
                                   "Can register status of ongoing match and event." },
                    { "Roles:4|Moderator", "Follows the crew. " +
                                   "Can see internal crew data like trainings, upcoming events, historical matches. " +
                                   "Can register status of ongoing match and event. " +
                                   "Can moderate existing or upcoming matches, events." },
                    { "Roles:5|Coach", "Follows the crew. " +
                                  "Can see internal crew data like trainings, upcoming events, historical matches. " +
                                  "Can register status of ongoing match and event. " +
                                  "Can moderate existing or upcoming matches, events. " +
                                  "Can manage players." },
                    { "Roles:6|Owner", "He is a creator of the crew. Can do whatever he wants with the crew." },
                    { "Roles:7|ClubOwner", "He is a creator of the club who owns crew(s)." },
                    { "Roles:8|Organizer", "Can create and manage of football events." },
                    { "Roles:9|Player", "He is a member of one or many crews. " +
                                "As a player of specific crew he can see all internal crew data." +
                                "Can see his own personalized statistics." },
                };

                config.AddInMemoryCollection(rolesDescription);
            });

            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IModule, CrewsModule>();
            });
        }
    }
}
