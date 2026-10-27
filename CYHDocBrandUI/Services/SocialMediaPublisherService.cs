using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;

namespace CYHDocBrandUI.Services;

public class SocialMediaPublisherService : ISocialMediaPublisherService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<SocialMediaPublisherService>? _logger;

    private string _fbAppId;
    private string _fbAppSecret;
    private readonly string _apiVersion;

    public SocialMediaPublisherService(HttpClient http, IConfiguration config, ILogger<SocialMediaPublisherService>? logger = null)
    {
        _http = http;
        _config = config;
        _logger = logger;

        _fbAppId = _config["SocialMedia:Facebook:AppId"] ?? "";
        _fbAppSecret = _config["SocialMedia:Facebook:AppSecret"] ?? "";
        _apiVersion = _config["SocialMedia:Facebook:ApiVersion"] ?? "v19.0";
    }

    public string GetMetaAppId() => _fbAppId;
    public string GetMetaAppSecret() => _fbAppSecret;

    public void UpdateMetaApiSettings(string appId, string appSecret)
    {
        if (!string.IsNullOrWhiteSpace(appId)) _fbAppId = appId.Trim();
        if (!string.IsNullOrWhiteSpace(appSecret)) _fbAppSecret = appSecret.Trim();
    }

    public string GetFacebookOAuthUrl(string redirectUri, string state)
    {
        var scopes = "pages_show_list,pages_read_engagement,pages_manage_posts,instagram_basic,instagram_content_publish";
        return $"https://www.facebook.com/{_apiVersion}/dialog/oauth" +
               $"?client_id={Uri.EscapeDataString(_fbAppId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&scope={Uri.EscapeDataString(scopes)}" +
               $"&response_type=code" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    public async Task<List<FacebookPageOptionDTO>> ExchangeFacebookCodeForPagesAsync(string code, string redirectUri)
    {
        var results = new List<FacebookPageOptionDTO>();
        try
        {
            // 1. Real Exchange: Exchange OAuth code for User Access Token
            var tokenUrl = $"https://graph.facebook.com/{_apiVersion}/oauth/access_token" +
                           $"?client_id={Uri.EscapeDataString(_fbAppId)}" +
                           $"&client_secret={Uri.EscapeDataString(_fbAppSecret)}" +
                           $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                           $"&code={Uri.EscapeDataString(code)}";

            var tokenRes = await _http.GetAsync(tokenUrl);
            var tokenJson = await tokenRes.Content.ReadAsStringAsync();

            if (!tokenRes.IsSuccessStatusCode)
            {
                _logger?.LogError("Real Meta OAuth Token Exchange failed: {Response}", tokenJson);
                throw new InvalidOperationException($"Meta OAuth Error: {ExtractErrorMessage(tokenJson)}");
            }

            using var doc = JsonDocument.Parse(tokenJson);
            var userAccessToken = doc.RootElement.GetProperty("access_token").GetString();
            if (string.IsNullOrEmpty(userAccessToken))
            {
                throw new InvalidOperationException("Did not receive a valid user access token from Meta.");
            }

            // 2. Fetch all real Facebook Pages managed by this user + linked Instagram business accounts
            var pagesUrl = $"https://graph.facebook.com/{_apiVersion}/me/accounts" +
                           $"?fields=id,name,category,access_token,instagram_business_account{{id,username}}" +
                           $"&access_token={Uri.EscapeDataString(userAccessToken)}";

            var pagesRes = await _http.GetAsync(pagesUrl);
            var pagesJson = await pagesRes.Content.ReadAsStringAsync();

            if (!pagesRes.IsSuccessStatusCode)
            {
                _logger?.LogError("Real Meta Fetch Pages failed: {Response}", pagesJson);
                throw new InvalidOperationException($"Meta Page Accounts Error: {ExtractErrorMessage(pagesJson)}");
            }

            using var pagesDoc = JsonDocument.Parse(pagesJson);
            if (pagesDoc.RootElement.TryGetProperty("data", out var dataArr))
            {
                foreach (var item in dataArr.EnumerateArray())
                {
                    var opt = new FacebookPageOptionDTO
                    {
                        PageId = item.GetProperty("id").GetString() ?? "",
                        PageName = item.GetProperty("name").GetString() ?? "",
                        Category = item.TryGetProperty("category", out var c) ? c.GetString() : "Medical & Health",
                        PageAccessToken = item.GetProperty("access_token").GetString() ?? ""
                    };

                    if (item.TryGetProperty("instagram_business_account", out var igObj))
                    {
                        opt.InstagramBusinessAccountId = igObj.GetProperty("id").GetString();
                        opt.InstagramUsername = igObj.TryGetProperty("username", out var u) ? u.GetString() : null;
                    }

                    results.Add(opt);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error executing real Facebook OAuth code exchange");
            throw;
        }

        return results;
    }

    public async Task<SocialAccountTestResult> TestConnectionAsync(DoctorSocialAccountDTO account)
    {
        if (string.IsNullOrWhiteSpace(account.AccessToken))
        {
            return new SocialAccountTestResult { IsSuccess = false, Message = "Access token is empty." };
        }

        if (account.Platform.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
        {
            return await TestWhatsAppConnectionAsync(account);
        }

        try
        {
            if (account.Platform.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
            {
                var testUrl = $"https://graph.facebook.com/{_apiVersion}/{account.PageOrAccountId}" +
                              $"?fields=id,name,category,link&access_token={Uri.EscapeDataString(account.AccessToken)}";

                var res = await _http.GetAsync(testUrl);
                var content = await res.Content.ReadAsStringAsync();

                if (res.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(content);
                    var name = doc.RootElement.TryGetProperty("name", out var n) ? n.GetString() : account.AccountName;
                    var cat = doc.RootElement.TryGetProperty("category", out var c) ? c.GetString() : "";
                    return new SocialAccountTestResult
                    {
                        IsSuccess = true,
                        Message = $"Verified Facebook Page: {name} ({cat})",
                        ProfileName = name
                    };
                }

                return new SocialAccountTestResult
                {
                    IsSuccess = false,
                    Message = $"Facebook API Error: {ExtractErrorMessage(content)}"
                };
            }
            else if (account.Platform.Equals("Instagram", StringComparison.OrdinalIgnoreCase))
            {
                var testUrl = $"https://graph.facebook.com/{_apiVersion}/{account.PageOrAccountId}" +
                              $"?fields=id,username,name&access_token={Uri.EscapeDataString(account.AccessToken)}";

                var res = await _http.GetAsync(testUrl);
                var content = await res.Content.ReadAsStringAsync();

                if (res.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(content);
                    var username = doc.RootElement.TryGetProperty("username", out var u) ? u.GetString() : account.AccountName;
                    return new SocialAccountTestResult
                    {
                        IsSuccess = true,
                        Message = $"Verified Instagram Business Account: @{username}",
                        ProfileName = $"@{username}"
                    };
                }

                return new SocialAccountTestResult
                {
                    IsSuccess = false,
                    Message = $"Instagram API Error: {ExtractErrorMessage(content)}"
                };
            }

            return new SocialAccountTestResult { IsSuccess = true, Message = "Connection verified." };
        }
        catch (Exception ex)
        {
            return new SocialAccountTestResult { IsSuccess = false, Message = $"Connection test failed: {ex.Message}" };
        }
    }

    public async Task<SocialAccountTestResult> TestWhatsAppConnectionAsync(DoctorSocialAccountDTO account)
    {
        if (string.IsNullOrWhiteSpace(account.AccessToken) || string.IsNullOrWhiteSpace(account.PageOrAccountId))
        {
            return new SocialAccountTestResult { IsSuccess = false, Message = "WhatsApp Phone Number ID and Access Token are required." };
        }

        try
        {
            // Verify Phone Number ID via Meta Graph API
            var testUrl = $"https://graph.facebook.com/{_apiVersion}/{account.PageOrAccountId}?fields=id,display_phone_number,verified_name,quality_rating";
            using var req = new HttpRequestMessage(HttpMethod.Get, testUrl);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken.Trim());

            var res = await _http.SendAsync(req);
            var content = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(content);
                var phone = doc.RootElement.TryGetProperty("display_phone_number", out var p) ? p.GetString() : account.AccountName;
                var name = doc.RootElement.TryGetProperty("verified_name", out var n) ? n.GetString() : "";
                var rating = doc.RootElement.TryGetProperty("quality_rating", out var q) ? q.GetString() : "Active";
                
                return new SocialAccountTestResult
                {
                    IsSuccess = true,
                    Message = $"Verified WhatsApp Business Number: {phone} (Rating: {rating})",
                    ProfileName = string.IsNullOrEmpty(name) ? phone : $"{name} ({phone})"
                };
            }

            return new SocialAccountTestResult
            {
                IsSuccess = false,
                Message = $"WhatsApp API Error: {ExtractErrorMessage(content)}"
            };
        }
        catch (Exception ex)
        {
            return new SocialAccountTestResult { IsSuccess = false, Message = $"WhatsApp connection test failed: {ex.Message}" };
        }
    }

    public async Task<PublishResult> PublishBannerAsync(DoctorSocialAccountDTO account, string? imageUrl, string? caption, string? recipientPhone = null)
    {
        if (string.IsNullOrWhiteSpace(account.AccessToken))
        {
            return new PublishResult { IsSuccess = false, ErrorMessage = "No access token configured for this account." };
        }

        try
        {
            if (account.Platform.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
            {
                return await PublishToFacebookAsync(account, imageUrl, caption);
            }
            else if (account.Platform.Equals("Instagram", StringComparison.OrdinalIgnoreCase))
            {
                return await PublishToInstagramAsync(account, imageUrl, caption);
            }
            else if (account.Platform.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
            {
                return await PublishToWhatsAppAsync(account, imageUrl, caption, recipientPhone);
            }

            return new PublishResult { IsSuccess = false, ErrorMessage = $"Unsupported platform: {account.Platform}" };
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception publishing banner to {Platform} for account {AccountId}", account.Platform, account.PageOrAccountId);
            return new PublishResult { IsSuccess = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<PublishResult> PublishToFacebookAsync(DoctorSocialAccountDTO account, string? imageUrl, string? caption)
    {
        var postUrl = $"https://graph.facebook.com/{_apiVersion}/{account.PageOrAccountId}/photos";

        if (!string.IsNullOrWhiteSpace(imageUrl) && imageUrl.StartsWith("data:image", StringComparison.OrdinalIgnoreCase))
        {
            var commaIdx = imageUrl.IndexOf(',');
            var base64Data = commaIdx >= 0 ? imageUrl[(commaIdx + 1)..] : imageUrl;
            var imageBytes = Convert.FromBase64String(base64Data);

            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(account.AccessToken.Trim()), "access_token");
            if (!string.IsNullOrWhiteSpace(caption)) form.Add(new StringContent(caption), "message");

            var byteContent = new ByteArrayContent(imageBytes);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            form.Add(byteContent, "source", "banner.png");

            var res = await _http.PostAsync(postUrl, form);
            var content = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(content);
                var photoId = doc.RootElement.GetProperty("id").GetString();
                return new PublishResult
                {
                    IsSuccess = true,
                    PlatformPostId = photoId,
                    PostUrl = $"https://www.facebook.com/{account.PageOrAccountId}/photos/{photoId}"
                };
            }

            return new PublishResult { IsSuccess = false, ErrorMessage = ExtractErrorMessage(content) };
        }
        else
        {
            var parameters = new Dictionary<string, string>
            {
                { "access_token", account.AccessToken.Trim() }
            };
            if (!string.IsNullOrWhiteSpace(imageUrl)) parameters.Add("url", imageUrl);
            if (!string.IsNullOrWhiteSpace(caption)) parameters.Add("message", caption);

            var res = await _http.PostAsync(postUrl, new FormUrlEncodedContent(parameters));
            var content = await res.Content.ReadAsStringAsync();

            if (res.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(content);
                var photoId = doc.RootElement.GetProperty("id").GetString();
                return new PublishResult
                {
                    IsSuccess = true,
                    PlatformPostId = photoId,
                    PostUrl = $"https://www.facebook.com/{account.PageOrAccountId}/photos/{photoId}"
                };
            }

            return new PublishResult { IsSuccess = false, ErrorMessage = ExtractErrorMessage(content) };
        }
    }

    private async Task<PublishResult> PublishToInstagramAsync(DoctorSocialAccountDTO account, string? imageUrl, string? caption)
    {
        if (string.IsNullOrWhiteSpace(imageUrl) || imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return new PublishResult
            {
                IsSuccess = false,
                ErrorMessage = "Instagram Graph API requires a publicly accessible image URL (https://...)."
            };
        }

        // Step 1: Create Container
        var containerUrl = $"https://graph.facebook.com/{_apiVersion}/{account.PageOrAccountId}/media";
        var cParams = new Dictionary<string, string>
        {
            { "access_token", account.AccessToken.Trim() },
            { "image_url", imageUrl }
        };
        if (!string.IsNullOrWhiteSpace(caption)) cParams.Add("caption", caption);

        var cRes = await _http.PostAsync(containerUrl, new FormUrlEncodedContent(cParams));
        var cContent = await cRes.Content.ReadAsStringAsync();

        if (!cRes.IsSuccessStatusCode)
        {
            return new PublishResult { IsSuccess = false, ErrorMessage = ExtractErrorMessage(cContent) };
        }

        using var cDoc = JsonDocument.Parse(cContent);
        var creationId = cDoc.RootElement.GetProperty("id").GetString();

        // Step 2: Publish Container
        var pubUrl = $"https://graph.facebook.com/{_apiVersion}/{account.PageOrAccountId}/media_publish";
        var pParams = new Dictionary<string, string>
        {
            { "access_token", account.AccessToken.Trim() },
            { "creation_id", creationId! }
        };

        var pRes = await _http.PostAsync(pubUrl, new FormUrlEncodedContent(pParams));
        var pContent = await pRes.Content.ReadAsStringAsync();

        if (pRes.IsSuccessStatusCode)
        {
            using var pDoc = JsonDocument.Parse(pContent);
            var mediaId = pDoc.RootElement.GetProperty("id").GetString();
            return new PublishResult
            {
                IsSuccess = true,
                PlatformPostId = mediaId,
                PostUrl = $"https://www.instagram.com/p/{mediaId}/"
            };
        }

        return new PublishResult { IsSuccess = false, ErrorMessage = ExtractErrorMessage(pContent) };
    }

    public async Task<PublishResult> PublishToWhatsAppAsync(DoctorSocialAccountDTO account, string? imageUrl, string? caption, string? recipientPhone)
    {
        // Target phone: explicitly passed > account.RefreshToken (stored default recipient number) > account.AccountName
        var targetPhone = !string.IsNullOrWhiteSpace(recipientPhone) ? recipientPhone.Trim() :
                          !string.IsNullOrWhiteSpace(account.RefreshToken) ? account.RefreshToken.Trim() :
                          account.AccountName.Trim();

        // Clean digits
        targetPhone = new string(targetPhone.Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(targetPhone))
        {
            return new PublishResult
            {
                IsSuccess = false,
                ErrorMessage = "WhatsApp requires a recipient phone number with country code (e.g. 919876543210)."
            };
        }

        var waUrl = $"https://graph.facebook.com/{_apiVersion}/{account.PageOrAccountId}/messages";

        object payload;
        if (!string.IsNullOrWhiteSpace(imageUrl) && !imageUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            // Send Image Message
            payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = targetPhone,
                type = "image",
                image = new
                {
                    link = imageUrl,
                    caption = caption ?? ""
                }
            };
        }
        else
        {
            // Send Text Message with caption
            payload = new
            {
                messaging_product = "whatsapp",
                recipient_type = "individual",
                to = targetPhone,
                type = "text",
                text = new
                {
                    preview_url = true,
                    body = caption ?? "New Healthcare Banner from CYH Hospital"
                }
            };
        }

        using var req = new HttpRequestMessage(HttpMethod.Post, waUrl);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", account.AccessToken.Trim());
        req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req);
        var content = await res.Content.ReadAsStringAsync();

        if (res.IsSuccessStatusCode)
        {
            using var doc = JsonDocument.Parse(content);
            string? msgId = null;
            if (doc.RootElement.TryGetProperty("messages", out var msgArr) && msgArr.GetArrayLength() > 0)
            {
                msgId = msgArr[0].GetProperty("id").GetString();
            }

            return new PublishResult
            {
                IsSuccess = true,
                PlatformPostId = msgId ?? "wa_" + DateTime.UtcNow.Ticks,
                PostUrl = $"https://api.whatsapp.com/send?phone={targetPhone}"
            };
        }

        return new PublishResult
        {
            IsSuccess = false,
            ErrorMessage = $"WhatsApp API Error: {ExtractErrorMessage(content)}"
        };
    }

    private static string ExtractErrorMessage(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("error", out var errObj))
            {
                if (errObj.TryGetProperty("message", out var msg))
                {
                    return msg.GetString() ?? "Unknown API Error";
                }
            }
        }
        catch { }
        return json.Length > 250 ? json[..250] + "..." : json;
    }
}
