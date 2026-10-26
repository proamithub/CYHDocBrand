using System;
using System.Text;

namespace CYHDocBrandRepo.Utilities;

/// <summary>
/// URL-safe AES-256 encryption helper for query strings and route parameters.
/// Encrypts integer IDs into tamper-proof, URL-safe alphanumeric tokens.
/// </summary>
public static class UrlEncryptionHelper
{
    /// <summary>
    /// Encrypts an integer ID into a URL-safe Base64 token using AES-256.
    /// </summary>
    public static string EncryptId(int id)
    {
        var rawEncrypted = AESEncryption.Encode(id.ToString());
        return ToUrlSafeBase64(rawEncrypted);
    }

    /// <summary>
    /// Decrypts a URL-safe token (or plain integer fallback) back to integer ID.
    /// </summary>
    public static int? DecryptId(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var trimmed = token.Trim();

        try
        {
            var base64 = FromUrlSafeBase64(trimmed);
            var decrypted = AESEncryption.Decode(base64);
            if (int.TryParse(decrypted, out int id) && id > 0)
            {
                return id;
            }
        }
        catch
        {
            // Ignore decryption failure and fallback to plain int
        }

        // Backward compatibility: If plain integer ID is supplied (e.g. ?id=1)
        if (int.TryParse(trimmed, out int plainId) && plainId > 0)
        {
            return plainId;
        }

        return null;
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
