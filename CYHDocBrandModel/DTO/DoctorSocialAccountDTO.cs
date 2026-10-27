using CYHDocBrandModel.Common;
namespace CYHDocBrandModel.DTO;

public class DoctorSocialAccountDTO
{
    public int Id { get; set; }
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string? DoctorName { get; set; }
    public string Platform { get; set; } = "Facebook"; // Facebook, Instagram, LinkedIn, Twitter
    public string AccountName { get; set; } = string.Empty; // e.g. "Dr. Sharma Dental Care"
    public string PageOrAccountId { get; set; } = string.Empty; // FB Page ID or IG Account ID
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime ConnectedAt { get; set; } = IndianTimeHelper.Now;
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestStatus { get; set; } // "Success", "Failed", "Pending"
    public string? LastTestMessage { get; set; }
}

public class FacebookPageOptionDTO
{
    public string PageId { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string PageAccessToken { get; set; } = string.Empty;
    public string? InstagramBusinessAccountId { get; set; }
    public string? InstagramUsername { get; set; }
}

public class SocialAccountSaveRequest
{
    public int Id { get; set; }
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string? DoctorName { get; set; }
    public string Platform { get; set; } = "Facebook";
    public string AccountName { get; set; } = string.Empty;
    public string PageOrAccountId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? TokenExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SocialAccountTestResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ProfileName { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
