using System.ComponentModel.DataAnnotations;

namespace Crews.API.Models;

public class UserViewModel
{
    public int Id { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    [Required]
    public string UserName { get; set; }

    public string Email { get; set; }

    [Required]
    public string FavoriteColor { get; set; }

    [Required]
    public string Password { get; set; }
}