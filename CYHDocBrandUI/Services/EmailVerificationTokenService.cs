using System.Security.Cryptography;
using System.Text;
using CYHDocBrandRepo.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CYHDocBrandUI.Services;

public class EmailVerificationTokenService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailVerificationTokenService> _logger;

    public EmailVerificationTokenService(IConfiguration config, ILogger<EmailVerificationTokenService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <summary>
    /// Generates a cryptographically encrypted, tamper-proof, URL-safe verification token (AES-256).
    /// </summary>
    public string GenerateToken(string userId, string email, TimeSpan? validity = null)
    {
        var duration = validity ?? TimeSpan.FromHours(24);
        var expiryTicks = DateTime.UtcNow.Add(duration).Ticks;
        var payload = $"{userId.Trim()}|{email.Trim().ToLowerInvariant()}|{expiryTicks}";
        var encrypted = AESEncryption.Encode(payload);
        return ToUrlSafeBase64(encrypted);
    }

    /// <summary>
    /// Validates and decrypts the verification token, checking expiry and structural integrity.
    /// </summary>
    public (bool IsValid, string? UserId, string? Email, string? ErrorMessage) ValidateToken(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, null, null, "Verification token is missing.");

        try
        {
            var base64 = FromUrlSafeBase64(token.Trim());
            var decrypted = AESEncryption.Decode(base64);

            if (string.IsNullOrWhiteSpace(decrypted))
                return (false, null, null, "Invalid or corrupted verification token.");

            var parts = decrypted.Split('|');
            if (parts.Length < 3)
                return (false, null, null, "Malformed verification token structure.");

            var userId = parts[0];
            var email = parts[1];
            if (!long.TryParse(parts[2], out var expiryTicks))
                return (false, null, null, "Invalid expiration timestamp in token.");

            var expiryTime = new DateTime(expiryTicks, DateTimeKind.Utc);
            if (DateTime.UtcNow > expiryTime)
                return (false, null, null, "This verification link has expired (valid for 24 hours). Please request a new link.");

            return (true, userId, email, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt email verification token");
            return (false, null, null, "Token decryption failed or token is invalid.");
        }
    }

    private static string ToUrlSafeBase64(string base64)
    {
        return base64.Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    private static string FromUrlSafeBase64(string urlSafe)
    {
        var base64 = urlSafe.Replace("-", "+").Replace("_", "/");
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return base64;
    }
}
