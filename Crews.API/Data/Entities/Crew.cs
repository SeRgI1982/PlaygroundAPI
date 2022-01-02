using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Crews.API.Data.Entities;

public class Crew
{
    [Key]
    public int Id { get; set; }

    public Guid UniqueId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(50)]
    public string ShortName { get; set; }

    // żółta, niebieska, czerwona, podstawowa, średnio-zaawansowana, zaawansowana etc.
    [Required]
    [MaxLength(50)]
    public string Group { get; set; }

    [Required]
    [Range(1990, 2999)]
    public int Year { get; set; }

    // Na serwerze w DB trzymany jest Id do loga. Na podstawie Id możemy ściągnąć Resource (byte[]) z serwera bądź

    // też z cache, który jest lokalnie

    public Guid LogoId { get; set; }

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

    [Required]
    public DateTime DateTimeAdd { get; set; }

    public ICollection<Training> Trainings { get; set; }

    public User User { get; set; }
}