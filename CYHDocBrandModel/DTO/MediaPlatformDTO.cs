using CYHDocBrandModel.Common;
namespace CYHDocBrandModel.DTO;

public class MediaPlatformDTO
{
    public int Id { get; set; }
    public string PlatformCode { get; set; } = string.Empty;
    public string PlatformName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public string ColorHex { get; set; } = "#ffffff";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool AllowGlobalFallback { get; set; } = true;
    public bool RequiresDoctorAccount { get; set; } = false;
    public string? GlobalApiEndpoint { get; set; }
    public string? GlobalApiKey { get; set; }
    public string? GlobalAppId { get; set; }
    public string? GlobalAppSecret { get; set; }
    public string? GlobalAccessToken { get; set; }
    public string? GlobalPageOrAccountId { get; set; }
    public string? GlobalPhoneNumberId { get; set; }
    public string? GlobalRefreshToken { get; set; }
    public DateTime? GlobalTokenExpiresAt { get; set; }
    public string? DefaultMessageTemplate { get; set; }
    public string? ApiNotes { get; set; }
    public DateTime UpdatedAt { get; set; } = IndianTimeHelper.Now;
    public string? UpdatedBy { get; set; }

    // Computed helpers for UI
    public bool HasGlobalApiConfigured => !string.IsNullOrWhiteSpace(GlobalAccessToken) || !string.IsNullOrWhiteSpace(GlobalApiKey);
    public string MaskedApiKey => string.IsNullOrWhiteSpace(GlobalApiKey) ? "Not Configured" : (GlobalApiKey.Length > 8 ? $"{GlobalApiKey[..4]}...{GlobalApiKey[^4..]}" : "********");
    public string MaskedAccessToken => string.IsNullOrWhiteSpace(GlobalAccessToken) ? "Not Configured" : (GlobalAccessToken.Length > 8 ? $"{GlobalAccessToken[..4]}...{GlobalAccessToken[^4..]}" : "********");
    public string StatusBadge => IsActive ? "Active" : "Inactive";
}

public class UpdateMediaPlatformApiRequest
{
    public int Id { get; set; }
    public string? GlobalApiEndpoint { get; set; }
    public string? GlobalApiKey { get; set; }
    public string? GlobalAppId { get; set; }
    public string? GlobalAppSecret { get; set; }
    public string? GlobalAccessToken { get; set; }
    public string? GlobalPageOrAccountId { get; set; }
    public string? GlobalPhoneNumberId { get; set; }
    public string? GlobalRefreshToken { get; set; }
    public DateTime? GlobalTokenExpiresAt { get; set; }
    public string? DefaultMessageTemplate { get; set; }
    public string? ApiNotes { get; set; }
    public string? UpdatedBy { get; set; }
}
