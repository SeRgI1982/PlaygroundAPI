using System;
using System.ComponentModel.DataAnnotations;
using Crews.API.Data;

namespace Crews.API.Models;

public class TrainingViewModel
{
    public int Id { get; set; }

    public Guid UniqueId { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    [System.Text.Json.Serialization.JsonConverterAttribute(typeof(TimeSpanConverter))]
    public TimeSpan StartTime { get; set; }

    [Required]
    [System.Text.Json.Serialization.JsonConverterAttribute(typeof(TimeSpanConverter))]
    public TimeSpan EndTime { get; set; }

    [Required]
    public TrainingPlace Place { get; set; }

    public string Description { get; set; }

    public string Country { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string Street { get; set; }
}