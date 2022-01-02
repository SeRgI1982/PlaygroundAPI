using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Crews.API;

public class SpecialUserAuthorizationHandler : AuthorizationHandler<SpecialUserRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SpecialUserRequirement requirement)
    {
        if (context.User?.Identity?.Name == requirement.UserName)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}