using CYHDocBrandModel.Common;

namespace CYHDocBrandData.Entities;

public class ApplicationUser
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Doctor";
    public string? AvatarUrl { get; set; }
    public string? Department { get; set; }
    public string? Password { get; set; }
    public DateTime CreatedAt { get; set; } = IndianTimeHelper.Now;
    public bool IsActive { get; set; } = true;
}
