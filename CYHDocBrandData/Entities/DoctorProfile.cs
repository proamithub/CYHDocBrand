using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CYHDocBrandData.Entities;

public class DoctorProfile
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public ApplicationUser? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; } = "General Medicine";

    [MaxLength(50)]
    public string LicenseNumber { get; set; } = string.Empty;

    public int ExperienceYears { get; set; } = 5;

    [Column(TypeName = "decimal(18,2)")]
    public decimal ConsultationFee { get; set; } = 50.00m;

    [MaxLength(100)]
    public string AvailableDays { get; set; } = "Mon - Fri";

    [MaxLength(50)]
    public string AvailableHours { get; set; } = "09:00 AM - 05:00 PM";

    [MaxLength(500)]
    public string? Bio { get; set; }

    public bool IsAvailableToday { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
