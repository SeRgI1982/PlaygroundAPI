using System.ComponentModel.DataAnnotations;

namespace Crews.API.Models;

public class LoginViewModel
{
    public bool RememberMe { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Password { get; set; }
}