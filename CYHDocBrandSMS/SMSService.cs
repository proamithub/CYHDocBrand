using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CYHDocBrandSMS;

public interface ISMSService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<bool> SendOtpSmsAsync(string phoneNumber, string otp);
}

public class SMSService : ISMSService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<SMSService> _logger;

    public SMSService(HttpClient httpClient, IConfiguration config, ILogger<SMSService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;
    }

    public async Task<bool> SendOtpSmsAsync(string phoneNumber, string otp)
    {
        var message = $"Your CYH Doctor Hub verification OTP is {otp}. Valid for 5 minutes. Do not share with anyone. (Demo OTP: 123456)";
        return await SendSmsAsync(phoneNumber, message);
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (cleanPhone.Length > 10) cleanPhone = cleanPhone.Substring(cleanPhone.Length - 10);

            var apiUrl = _config["SmsGateway:ApiUrl"] ?? "https://www.fast2sms.com/dev/bulkV2";
            var apiKey = _config["SmsGateway:ApiKey"] ?? "";
            var senderId = _config["SmsGateway:SenderId"] ?? "CYHDOC";
            var route = _config["SmsGateway:Route"] ?? "otp";
            var isLive = bool.TryParse(_config["SmsGateway:IsLive"], out var live) && live;
            var customUrlTemplate = _config["SmsGateway:CustomUrlTemplate"];

            _logger.LogInformation("📲 [SMS GATEWAY REQUEST] Sending to: {Phone} | IsLive: {IsLive} | Gateway: {Url}", cleanPhone, isLive, apiUrl);

            if (!isLive || string.IsNullOrWhiteSpace(apiKey))
            {
                _logger.LogInformation("📢 [SMS GATEWAY SIMULATION] (Live disabled or no API key set). To: +91-{Phone} | Content: \"{Msg}\"", cleanPhone, message);
                return true;
            }

            // Real HTTP Gateway Execution
            if (!string.IsNullOrWhiteSpace(customUrlTemplate))
            {
                // Custom Gateway URL Template (e.g. GET http://api.smsprovider.com/send?user=...&phone={PhoneNumber}&msg={Message})
                var url = customUrlTemplate
                    .Replace("{ApiKey}", Uri.EscapeDataString(apiKey))
                    .Replace("{PhoneNumber}", Uri.EscapeDataString(cleanPhone))
                    .Replace("{Message}", Uri.EscapeDataString(message))
                    .Replace("{SenderId}", Uri.EscapeDataString(senderId));

                var response = await _httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("📲 [CUSTOM SMS GATEWAY RESPONSE] StatusCode: {Code} | Response: {Resp}", response.StatusCode, content);
                return response.IsSuccessStatusCode;
            }
            else
            {
                // Standard REST POST Gateway (e.g. Fast2SMS / Standard SMS API)
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                request.Headers.Add("authorization", apiKey);
                request.Headers.Add("Accept", "application/json");

                var payload = new
                {
                    route = route,
                    message = message,
                    language = "english",
                    flash = 0,
                    numbers = cleanPhone
                };

                request.Content = JsonContent.Create(payload);

                var response = await _httpClient.SendAsync(request);
                var respBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("📲 [SMS GATEWAY RESPONSE] StatusCode: {Code} | Body: {Body}", response.StatusCode, respBody);

                return response.IsSuccessStatusCode;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to send SMS to {Phone}", phoneNumber);
            return false;
        }
    }
}
