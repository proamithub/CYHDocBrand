USE [CYHDocBrandBD];
GO

-- ============================================================================
-- Stored Procedure: dbo.DecryptDocPassword
-- Purpose: Decrypts doctor / user password in SQL Server
--
-- Examples:
--   1. Pass encrypted string directly:
--      EXEC dbo.DecryptDocPassword @EncryptedPassword = 'n7uoG53mus5DzCXmNgv7NQ=='
--
--   2. Pass doctor's email to decrypt their password:
--      EXEC dbo.DecryptDocPassword @Email = 'doctor@cyhdoctorhub.com'
--
--   3. View all doctors and their decrypted passwords:
--      EXEC dbo.DecryptDocPassword
-- ============================================================================
CREATE OR ALTER PROCEDURE dbo.DecryptDocPassword
    @EncryptedPassword NVARCHAR(MAX) = NULL,
    @Email NVARCHAR(256) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Cipher NVARCHAR(MAX) = LTRIM(RTRIM(@EncryptedPassword));

    -- If Email is provided, lookup password from AspNetUsers
    IF @Cipher IS NULL AND @Email IS NOT NULL
    BEGIN
        SELECT TOP 1 @Cipher = Password 
        FROM AspNetUsers 
        WHERE Email = @Email OR UserName = @Email;
    END

    -- If neither is provided, list all doctors with passwords
    IF @Cipher IS NULL
    BEGIN
        SELECT 
            u.Id AS UserId,
            u.FullName,
            u.Email,
            u.Role,
            u.Password AS EncryptedPassword,
            CASE 
                WHEN u.Password = 'n7uoG53mus5DzCXmNgv7NQ==' THEN 'Doctor@12345'
                WHEN u.Password = 'L4Yq5jzhlap0wTagsBRCUg==' THEN 'Admin@12345'
                ELSE 'Run: powershell -File "D:\MyAppAi\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + ISNULL(u.Password, '') + '"'
            END AS DecryptedPassword,
            u.IsActive
        FROM AspNetUsers u
        WHERE u.Role = 'Doctor'
        ORDER BY u.FullName;
        RETURN;
    END

    -- Direct Check for standard system passwords
    IF @Cipher = 'n7uoG53mus5DzCXmNgv7NQ=='
    BEGIN
        SELECT @Cipher AS EncryptedPassword, 'Doctor@12345' AS DecryptedPassword;
        RETURN;
    END

    IF @Cipher = 'L4Yq5jzhlap0wTagsBRCUg=='
    BEGIN
        SELECT @Cipher AS EncryptedPassword, 'Admin@12345' AS DecryptedPassword;
        RETURN;
    END

    -- Dynamic Decryption using PowerShell helper script (if xp_cmdshell enabled)
    BEGIN TRY
        DECLARE @cmd NVARCHAR(4000) = 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\MyAppAi\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + @Cipher + '"';
        CREATE TABLE #CmdOutput (Line NVARCHAR(MAX));
        INSERT INTO #CmdOutput
        EXEC xp_cmdshell @cmd;

        DECLARE @Decrypted NVARCHAR(MAX);
        SELECT TOP 1 @Decrypted = LTRIM(RTRIM(REPLACE(Line, 'Decrypted :', '')))
        FROM #CmdOutput 
        WHERE Line LIKE '%Decrypted :%';

        SELECT 
            @Cipher AS EncryptedPassword, 
            ISNULL(@Decrypted, 'Could not decode') AS DecryptedPassword;

        DROP TABLE #CmdOutput;
    END TRY
    BEGIN CATCH
        SELECT 
            @Cipher AS EncryptedPassword, 
            'Run: powershell -File "D:\MyAppAi\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + @Cipher + '"' AS DecryptedPassword;
    END CATCH
END
GO
