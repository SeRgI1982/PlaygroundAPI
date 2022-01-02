using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Crews.API.Models;

public class CrewViewModel
{
    public int Id { get; set; }

    public Guid UniqueId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 5)]
    [Display(Name = "Name", Description = "Your Crew Full Name.")]
    public string Name { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 5)]
    [Display(Name = "ShortName", Description = "Your Crew Short Name.")]
    public string ShortName { get; set; }

    // żółta, niebieska, czerwona, podstawowa, średnio-zaawansowana, zaawansowana etc.
    [Required]
    [StringLength(100, MinimumLength = 1)]
    [Display(Name = "Name", Description = "Your Crew Group Name.")]
    public string Group { get; set; }

    // Na serwerze w DB trzymany jest Id do loga. Na podstawie Id możemy ściągnąć Resource (byte[]) z serwera bądź
    // też z cache, który jest lokalnie
    //public string LogoUrl { get; set; }

    [Required]
    [Range(1990, 2990, ErrorMessage = "Please enter valid age category.")]
    public int Year { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public string Country { get; set; }

    public string City { get; set; }

    public string Street { get; set; }

    // Województwo - gdyby ktoś chciał wylistować wszystkie zespoły z danego województwa
    public string Region { get; set; }

    // Powiat - gdyby ktoś chciał wylistować wszystkie zespoły z powiatu np. Trzebnicki
    public string Subregion { get; set; }

    public bool Archived { get; set; }

    public ICollection<TrainingViewModel> Trainings { get; set; }
}