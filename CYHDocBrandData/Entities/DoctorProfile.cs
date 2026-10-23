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

    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; } = "General Medicine";

    public string Qualifications { get; set; } = "MBBS, MD";

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

    public string? ProfilePic { get; set; }
    public string? MobileNo { get; set; } = "+91 98765 43210";
    public string? ClinicName { get; set; } = "CYH Heart Care Institute & Hospital";
    public string? ClinicAddress { get; set; } = "Plot 42, Health City, Medical Enclave";
    public string? City { get; set; } = "New Delhi";
    public string? State { get; set; } = "Delhi";
    public string? PinCode { get; set; } = "110029";
    public string? WebsiteUrl { get; set; } = "https://www.clickyourhospital.com";
    public string? ClinicLogo { get; set; }
    public string PrimaryColorHex { get; set; } = "#00f2fe";
    public string SecondaryColorHex { get; set; } = "#4facfe";
    public string PreferredFont { get; set; } = "Inter";
    public int AiCredits { get; set; } = 25;
    public bool IsVerified { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
