using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CYHDocBrandData;

public static class MediaPlatformInitializer
{
    public static async Task InitializeAsync(string connectionString, ILogger? logger = null)
    {
        try
        {
            await using var con = new SqlConnection(connectionString);
            await con.OpenAsync();

            var createTableSql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MediaPlatforms_CYH]') AND type IN (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[MediaPlatforms_CYH] (
                        [Id]                    INT             IDENTITY(1,1)   NOT NULL,
                        [PlatformCode]          NVARCHAR(50)                    NOT NULL,
                        [PlatformName]          NVARCHAR(100)                   NOT NULL,
                        [Category]              NVARCHAR(50)                    NOT NULL,
                        [IconClass]             NVARCHAR(50)                    NOT NULL,
                        [ColorHex]              NVARCHAR(10)                    NOT NULL,
                        [SortOrder]             INT                             NOT NULL CONSTRAINT [DF_MediaPlatforms_CYH_SortOrder] DEFAULT (99),
                        [IsActive]              BIT                             NOT NULL CONSTRAINT [DF_MediaPlatforms_CYH_IsActive] DEFAULT (1),
                        [AllowGlobalFallback]   BIT                             NOT NULL CONSTRAINT [DF_MediaPlatforms_CYH_AllowGlobalFallback] DEFAULT (1),
                        [RequiresDoctorAccount] BIT                             NOT NULL CONSTRAINT [DF_MediaPlatforms_CYH_RequiresDoctorAccount] DEFAULT (0),
                        [GlobalApiEndpoint]     NVARCHAR(500)                   NULL,
                        [GlobalApiKey]          NVARCHAR(MAX)                   NULL,
                        [GlobalAppId]           NVARCHAR(500)                   NULL,
                        [GlobalAppSecret]       NVARCHAR(MAX)                   NULL,
                        [GlobalAccessToken]     NVARCHAR(MAX)                   NULL,
                        [GlobalPageOrAccountId] NVARCHAR(500)                   NULL,
                        [GlobalPhoneNumberId]   NVARCHAR(200)                   NULL,
                        [GlobalRefreshToken]    NVARCHAR(MAX)                   NULL,
                        [GlobalTokenExpiresAt]  DATETIME2(7)                    NULL,
                        [DefaultMessageTemplate] NVARCHAR(MAX)                  NULL,
                        [ApiNotes]              NVARCHAR(MAX)                   NULL,
                        [UpdatedAt]             DATETIME2(7)                    NOT NULL CONSTRAINT [DF_MediaPlatforms_CYH_UpdatedAt] DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
                        [UpdatedBy]             NVARCHAR(200)                   NULL,
                        CONSTRAINT [PK_MediaPlatforms_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    CREATE UNIQUE NONCLUSTERED INDEX [UQ_MediaPlatforms_CYH_PlatformCode] ON [dbo].[MediaPlatforms_CYH] ([PlatformCode] ASC);
                END

                -- Seed platforms if missing
                IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'WHATSAPP')
                BEGIN
                    INSERT INTO [dbo].[MediaPlatforms_CYH] ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex], [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount], [ApiNotes], [UpdatedAt], [UpdatedBy])
                    VALUES (N'WHATSAPP', N'WhatsApp Business', N'Messaging', N'bi-whatsapp', N'#25D366', 1, 1, 1, 0, N'WhatsApp Cloud API — Meta Business Account. Store Phone Number ID and Access Token here.', DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
                END

                IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'FACEBOOK')
                BEGIN
                    INSERT INTO [dbo].[MediaPlatforms_CYH] ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex], [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount], [ApiNotes], [UpdatedAt], [UpdatedBy])
                    VALUES (N'FACEBOOK', N'Facebook Page', N'Social Media', N'bi-facebook', N'#1877F2', 2, 1, 1, 0, N'Facebook Graph API — Meta App credentials. Store Page Access Token and Page ID here.', DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
                END

                IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'INSTAGRAM')
                BEGIN
                    INSERT INTO [dbo].[MediaPlatforms_CYH] ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex], [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount], [ApiNotes], [UpdatedAt], [UpdatedBy])
                    VALUES (N'INSTAGRAM', N'Instagram Business', N'Social Media', N'bi-instagram', N'#E1306C', 3, 1, 1, 0, N'Instagram Graph API — linked Facebook Business Account. Store Instagram Account ID and Access Token here.', DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
                END

                IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'LINKEDIN')
                BEGIN
                    INSERT INTO [dbo].[MediaPlatforms_CYH] ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex], [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount], [ApiNotes], [UpdatedAt], [UpdatedBy])
                    VALUES (N'LINKEDIN', N'LinkedIn Page', N'Professional', N'bi-linkedin', N'#0A66C2', 4, 1, 1, 0, N'LinkedIn API — Community Management API. Store Organization ID and Access Token here.', DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
                END

                IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'TWITTER')
                BEGIN
                    INSERT INTO [dbo].[MediaPlatforms_CYH] ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex], [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount], [ApiNotes], [UpdatedAt], [UpdatedBy])
                    VALUES (N'TWITTER', N'Twitter / X', N'Microblogging', N'bi-twitter-x', N'#ffffff', 5, 1, 1, 0, N'Twitter / X API v2. Store API Key, Secret and Bearer Token here.', DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
                END

                -- Google Business Profile (AllowGlobalFallback=0, RequiresDoctorAccount=1)
                IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'GOOGLE')
                BEGIN
                    INSERT INTO [dbo].[MediaPlatforms_CYH] ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex], [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount], [ApiNotes], [UpdatedAt], [UpdatedBy])
                    VALUES (N'GOOGLE', N'Google Business Profile', N'Local Search', N'bi-google', N'#4285F4', 6, 1, 0, 1, N'Google Business Profile API — each doctor must link their own Google Business account. No global fallback allowed.', DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
                END
            ";

            await using (var cmd = new SqlCommand(createTableSql, con))
            {
                await cmd.ExecuteNonQueryAsync();
            }

            // Stored procedure ManageMediaPlatforms_CYH
            var spSql = @"
                CREATE OR ALTER PROCEDURE [dbo].[ManageMediaPlatforms_CYH]
                    @Flag                   NVARCHAR(50)    = NULL,
                    @Action                 NVARCHAR(50)    = NULL,
                    @Id                     INT             = NULL,
                    @UpdatedBy              NVARCHAR(200)   = NULL,
                    @GlobalApiEndpoint      NVARCHAR(500)   = NULL,
                    @GlobalApiKey           NVARCHAR(MAX)   = NULL,
                    @GlobalAppId            NVARCHAR(500)   = NULL,
                    @GlobalAppSecret        NVARCHAR(MAX)   = NULL,
                    @GlobalAccessToken      NVARCHAR(MAX)   = NULL,
                    @GlobalPageOrAccountId  NVARCHAR(500)   = NULL,
                    @GlobalPhoneNumberId    NVARCHAR(200)   = NULL,
                    @GlobalRefreshToken     NVARCHAR(MAX)   = NULL,
                    @GlobalTokenExpiresAt   DATETIME2(7)    = NULL,
                    @DefaultMessageTemplate NVARCHAR(MAX)   = NULL,
                    @ApiNotes               NVARCHAR(MAX)   = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET @Action = COALESCE(@Flag, @Action);

                    IF @Action = 'GET_ALL'
                    BEGIN
                        SELECT [Id], [PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
                               [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
                               [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
                               [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
                               [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate],
                               [ApiNotes], [UpdatedAt], [UpdatedBy]
                        FROM [dbo].[MediaPlatforms_CYH]
                        ORDER BY [SortOrder] ASC, [Id] ASC;
                    END
                    ELSE IF @Action = 'GET_ACTIVE'
                    BEGIN
                        SELECT [Id], [PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
                               [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
                               [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
                               [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
                               [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate],
                               [ApiNotes], [UpdatedAt], [UpdatedBy]
                        FROM [dbo].[MediaPlatforms_CYH]
                        WHERE [IsActive] = 1
                        ORDER BY [SortOrder] ASC, [Id] ASC;
                    END
                    ELSE IF @Action = 'GET_BY_ID'
                    BEGIN
                        SELECT [Id], [PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
                               [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
                               [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
                               [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
                               [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate],
                               [ApiNotes], [UpdatedAt], [UpdatedBy]
                        FROM [dbo].[MediaPlatforms_CYH]
                        WHERE [Id] = @Id;
                    END
                    ELSE IF @Action = 'TOGGLE_ACTIVE'
                    BEGIN
                        UPDATE [dbo].[MediaPlatforms_CYH]
                        SET [IsActive] = CASE WHEN [IsActive] = 1 THEN 0 ELSE 1 END,
                            [UpdatedAt] = DATEADD(minute, 330, SYSUTCDATETIME()),
                            [UpdatedBy] = @UpdatedBy
                        WHERE [Id] = @Id;
                        SELECT @@ROWCOUNT;
                    END
                    ELSE IF @Action = 'UPDATE_GLOBAL_API'
                    BEGIN
                        UPDATE [dbo].[MediaPlatforms_CYH]
                        SET [GlobalApiEndpoint] = @GlobalApiEndpoint,
                            [GlobalApiKey] = @GlobalApiKey,
                            [GlobalAppId] = @GlobalAppId,
                            [GlobalAppSecret] = @GlobalAppSecret,
                            [GlobalAccessToken] = @GlobalAccessToken,
                            [GlobalPageOrAccountId] = @GlobalPageOrAccountId,
                            [GlobalPhoneNumberId] = @GlobalPhoneNumberId,
                            [GlobalRefreshToken] = @GlobalRefreshToken,
                            [GlobalTokenExpiresAt] = @GlobalTokenExpiresAt,
                            [DefaultMessageTemplate] = @DefaultMessageTemplate,
                            [ApiNotes] = @ApiNotes,
                            [UpdatedAt] = DATEADD(minute, 330, SYSUTCDATETIME()),
                            [UpdatedBy] = @UpdatedBy
                        WHERE [Id] = @Id;
                        SELECT @@ROWCOUNT;
                    END
                END
            ";

            await using (var cmdSp = new SqlCommand(spSql, con))
            {
                await cmdSp.ExecuteNonQueryAsync();
            }

            logger?.LogInformation("Verified MediaPlatforms_CYH table and ManageMediaPlatforms_CYH stored procedure in SQL.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error initializing MediaPlatforms_CYH in SQL.");
        }
    }
}
