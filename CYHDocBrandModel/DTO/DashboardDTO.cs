namespace CYHDocBrandModel.DTO;

public class AdminDashboardDTO
{
    public int TotalDoctors { get; set; }
    public int ActiveDoctorsOnDuty { get; set; }
    public int TotalDepartments { get; set; }
    public int TotalUsers { get; set; }
}

public class DoctorDashboardDTO
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public string AvailableHours { get; set; } = string.Empty;
    public string AvailableDays { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public bool IsAvailableToday { get; set; }
}

public class LoginDTO
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = true;
}

public class CurrentUserDTO
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DoctorProfileId { get; set; }
    public string? Department { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
