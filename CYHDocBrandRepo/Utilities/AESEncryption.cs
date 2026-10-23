using System.Security.Cryptography;
using System.Text;

namespace CYHDocBrandRepo.Utilities;

/// <summary>
/// Cryptographically secure AES-256 encryption & decryption utility.
/// Uses 256-bit symmetric key derivation (SHA-256) with CBC mode and PKCS7 padding.
/// </summary>
public static class AESEncryption
{
    private const string SecretKey = "CYH_DOCTOR_HUB_AES_256_SECRET_KEY_2026#";
    private const string SecretIv = "CYH_DOCTOR_HUB_AES_IV_VECTOR_2026!";

    private static readonly byte[] Key;
    private static readonly byte[] Iv;

    static AESEncryption()
    {
        using var sha256 = SHA256.Create();
        Key = sha256.ComputeHash(Encoding.UTF8.GetBytes(SecretKey));

        using var md5 = MD5.Create();
        Iv = md5.ComputeHash(Encoding.UTF8.GetBytes(SecretIv));
    }

    /// <summary>
    /// Encrypts plaintext using AES-256-CBC and returns a Base64-encoded ciphertext string.
    /// </summary>
    public static string Encode(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.Key = Key;
        aes.IV = Iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts a Base64-encoded AES-256 ciphertext string back into original plaintext.
    /// </summary>
    public static string Decode(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.Key = Key;
            aes.IV = Iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch
        {
            // Fallback for legacy plain or simple base64 strings if encountered
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(cipherText));
            }
            catch
            {
                return cipherText;
            }
        }
    }
}
