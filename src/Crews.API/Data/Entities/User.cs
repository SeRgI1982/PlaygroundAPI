using Microsoft.AspNetCore.Identity;

namespace Crews.API.Data.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; }

    public string LastName { get; set; }
}