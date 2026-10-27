using CYHDocBrandModel.DTO;

namespace CYHDocBrandUI.Services;

public class PublishResult
{
    public bool IsSuccess { get; set; }
    public string? PlatformPostId { get; set; }
    public string? PostUrl { get; set; }
    public string? ErrorMessage { get; set; }
}

public interface ISocialMediaPublisherService
{
    Task<SocialAccountTestResult> TestConnectionAsync(DoctorSocialAccountDTO account);
    Task<PublishResult> PublishBannerAsync(DoctorSocialAccountDTO account, string? imageUrl, string? caption, string? recipientPhone = null);
    string GetFacebookOAuthUrl(string redirectUri, string state);
    Task<List<FacebookPageOptionDTO>> ExchangeFacebookCodeForPagesAsync(string code, string redirectUri);
    Task<PublishResult> PublishToWhatsAppAsync(DoctorSocialAccountDTO account, string? imageUrl, string? caption, string? recipientPhone);
    Task<SocialAccountTestResult> TestWhatsAppConnectionAsync(DoctorSocialAccountDTO account);
    string GetMetaAppId();
    string GetMetaAppSecret();
    void UpdateMetaApiSettings(string appId, string appSecret);
}
