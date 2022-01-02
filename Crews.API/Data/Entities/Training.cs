using System;
using System.ComponentModel.DataAnnotations;

namespace Crews.API.Data.Entities;

public class Training
{
    [Key]
    public int Id { get; set; }

    public Guid UniqueId { get; set; }

    [Required]
    public Crew Crew { get; set; }

    public DateTime ValidFrom { get; set; }

    public DateTime ValidTo { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    [Required]
    public TrainingPlace Place { get; set; }

    public string Description { get; set; }

    public string Country { get; set; }

    [Required]
    public string City { get; set; }

    [Required]
    public string Street { get; set; }

    [Required]
    public DateTime DateTimeAdd { get; set; }

    public User User { get; set; }
}