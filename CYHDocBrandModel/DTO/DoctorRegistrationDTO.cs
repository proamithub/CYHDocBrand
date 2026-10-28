using System.ComponentModel.DataAnnotations;

namespace CYHDocBrandModel.DTO;

public class DoctorRegistrationDTO
{
    [Required(ErrorMessage = "Full Name is required.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile number is required.")]
    [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Mobile number must be exactly 10 digits (starting with 6, 7, 8, or 9).")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Specialization is required.")]
    public string Specialization { get; set; } = "General Medicine";

    [Required(ErrorMessage = "Medical License / Registration number is required.")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(0, 70, ErrorMessage = "Experience must be between 0 and 70 years.")]
    public int ExperienceYears { get; set; } = 3;

    [Range(0, 100000, ErrorMessage = "Please enter a valid consultation fee.")]
    public decimal ConsultationFee { get; set; } = 500m;

    public string AvailableDays { get; set; } = "Mon - Sat";

    public string AvailableHours { get; set; } = "09:00 AM - 05:00 PM";

    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
    public string? Bio { get; set; }
}
