using Microsoft.AspNetCore.Authorization;

namespace Crews.API;

public class SpecialUserRequirement : IAuthorizationRequirement
{
    public SpecialUserRequirement(string username)
    {
        UserName = username;
    }

    public string UserName { get; }
}