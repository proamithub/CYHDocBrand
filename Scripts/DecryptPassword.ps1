param(
    [string]$EncryptedPassword = "n7uoG53mus5DzCXmNgv7NQ=="
)

try {
    $cipherBytes = [System.Convert]::FromBase64String($EncryptedPassword)

    $sha = [System.Security.Cryptography.SHA256]::Create()
    $key = $sha.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("CYH_DOCTOR_HUB_AES_256_SECRET_KEY_2026#"))

    $md5 = [System.Security.Cryptography.MD5]::Create()
    $iv = $md5.ComputeHash([System.Text.Encoding]::UTF8.GetBytes("CYH_DOCTOR_HUB_AES_IV_VECTOR_2026!"))

    $aes = [System.Security.Cryptography.Aes]::Create()
    $aes.Key = $key
    $aes.IV = $iv
    $aes.Mode = [System.Security.Cryptography.CipherMode]::CBC
    $aes.Padding = [System.Security.Cryptography.PaddingMode]::PKCS7

    $decryptor = $aes.CreateDecryptor()
    $plainBytes = $decryptor.TransformFinalBlock($cipherBytes, 0, $cipherBytes.Length)
    $plainText = [System.Text.Encoding]::UTF8.GetString($plainBytes)

    Write-Host "=========================================="
    Write-Host " Encrypted : $EncryptedPassword"
    Write-Host " Decrypted : $plainText" -ForegroundColor Green
    Write-Host "=========================================="
}
catch {
    Write-Host "Error decrypting: $_" -ForegroundColor Red
}
