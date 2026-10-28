-- ============================================================================
-- CYH DoctorHub — Media Platforms Migration Script
-- Target: SQL Server (VPS Server: https://vps2.clickyourhospital.com)
-- Database: CYHDocBrandBD
-- File: Add_MediaPlatforms_CYH.sql
-- Description: Creates MediaPlatforms_CYH table and seeds 6 platforms,
--              plus stored procedure ManageMediaPlatforms_CYH
-- ============================================================================

-- ============================================================================
-- 1. Create MediaPlatforms_CYH Table
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[MediaPlatforms_CYH]') AND type IN (N'U'))
BEGIN
    CREATE TABLE [dbo].[MediaPlatforms_CYH] (
        [Id]                    INT             IDENTITY(1,1)   NOT NULL,
        [PlatformCode]          NVARCHAR(50)                    NOT NULL,
        [PlatformName]          NVARCHAR(100)                   NOT NULL,
        [Category]              NVARCHAR(50)                    NOT NULL,
        [IconClass]             NVARCHAR(50)                    NOT NULL,
        [ColorHex]              NVARCHAR(10)                    NOT NULL,
        [SortOrder]             INT                             NOT NULL    CONSTRAINT [DF_MediaPlatforms_CYH_SortOrder]             DEFAULT (99),
        [IsActive]              BIT                             NOT NULL    CONSTRAINT [DF_MediaPlatforms_CYH_IsActive]               DEFAULT (1),
        [AllowGlobalFallback]   BIT                             NOT NULL    CONSTRAINT [DF_MediaPlatforms_CYH_AllowGlobalFallback]    DEFAULT (1),
        [RequiresDoctorAccount] BIT                             NOT NULL    CONSTRAINT [DF_MediaPlatforms_CYH_RequiresDoctorAccount]  DEFAULT (0),
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
        [UpdatedAt]             DATETIME2(7)                    NOT NULL    CONSTRAINT [DF_MediaPlatforms_CYH_UpdatedAt]             DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [UpdatedBy]             NVARCHAR(200)                   NULL,
        CONSTRAINT [PK_MediaPlatforms_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    PRINT 'Table [MediaPlatforms_CYH] created successfully.';
END
ELSE
BEGIN
    PRINT 'Table [MediaPlatforms_CYH] already exists. Skipping creation.';
END
GO

-- ============================================================================
-- 2. Unique Index on PlatformCode
-- ============================================================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'UQ_MediaPlatforms_CYH_PlatformCode' AND object_id = OBJECT_ID(N'[dbo].[MediaPlatforms_CYH]'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_MediaPlatforms_CYH_PlatformCode]
        ON [dbo].[MediaPlatforms_CYH] ([PlatformCode] ASC);
    PRINT 'Unique index [UQ_MediaPlatforms_CYH_PlatformCode] created successfully.';
END
ELSE
BEGIN
    PRINT 'Unique index [UQ_MediaPlatforms_CYH_PlatformCode] already exists. Skipping.';
END
GO

-- ============================================================================
-- 3. Seed Data — 6 Media Platforms
-- ============================================================================

-- WhatsApp Business
IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'WHATSAPP')
BEGIN
    INSERT INTO [dbo].[MediaPlatforms_CYH]
        ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
         [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
         [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
         [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
         [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate], [ApiNotes],
         [UpdatedAt], [UpdatedBy])
    VALUES
        (N'WHATSAPP', N'WhatsApp Business', N'Messaging', N'bi-whatsapp', N'#25D366',
         1, 1, 1, 0,
         NULL, NULL, NULL, NULL,
         NULL, NULL, NULL,
         NULL, NULL, NULL,
         N'WhatsApp Cloud API — requires Meta Business Account. Store Phone Number ID and Access Token here.',
         DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
    PRINT 'Seeded platform: WHATSAPP';
END
ELSE
BEGIN
    PRINT 'Platform WHATSAPP already exists. Skipping seed.';
END
GO

-- Facebook Page
IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'FACEBOOK')
BEGIN
    INSERT INTO [dbo].[MediaPlatforms_CYH]
        ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
         [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
         [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
         [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
         [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate], [ApiNotes],
         [UpdatedAt], [UpdatedBy])
    VALUES
        (N'FACEBOOK', N'Facebook Page', N'Social Media', N'bi-facebook', N'#1877F2',
         2, 1, 1, 0,
         NULL, NULL, NULL, NULL,
         NULL, NULL, NULL,
         NULL, NULL, NULL,
         N'Facebook Graph API — requires Meta App credentials. Store Page Access Token and Page ID here.',
         DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
    PRINT 'Seeded platform: FACEBOOK';
END
ELSE
BEGIN
    PRINT 'Platform FACEBOOK already exists. Skipping seed.';
END
GO

-- Instagram Business
IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'INSTAGRAM')
BEGIN
    INSERT INTO [dbo].[MediaPlatforms_CYH]
        ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
         [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
         [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
         [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
         [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate], [ApiNotes],
         [UpdatedAt], [UpdatedBy])
    VALUES
        (N'INSTAGRAM', N'Instagram Business', N'Social Media', N'bi-instagram', N'#E1306C',
         3, 1, 1, 0,
         NULL, NULL, NULL, NULL,
         NULL, NULL, NULL,
         NULL, NULL, NULL,
         N'Instagram Graph API — requires linked Facebook Business Account. Store Instagram Business Account ID and Access Token here.',
         DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
    PRINT 'Seeded platform: INSTAGRAM';
END
ELSE
BEGIN
    PRINT 'Platform INSTAGRAM already exists. Skipping seed.';
END
GO

-- LinkedIn Page
IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'LINKEDIN')
BEGIN
    INSERT INTO [dbo].[MediaPlatforms_CYH]
        ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
         [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
         [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
         [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
         [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate], [ApiNotes],
         [UpdatedAt], [UpdatedBy])
    VALUES
        (N'LINKEDIN', N'LinkedIn Page', N'Professional', N'bi-linkedin', N'#0A66C2',
         4, 1, 1, 0,
         NULL, NULL, NULL, NULL,
         NULL, NULL, NULL,
         NULL, NULL, NULL,
         N'LinkedIn API — requires LinkedIn Developer Application. Store Client ID, Client Secret, and Access Token here.',
         DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
    PRINT 'Seeded platform: LINKEDIN';
END
ELSE
BEGIN
    PRINT 'Platform LINKEDIN already exists. Skipping seed.';
END
GO

-- Twitter / X
IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'TWITTER')
BEGIN
    INSERT INTO [dbo].[MediaPlatforms_CYH]
        ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
         [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
         [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
         [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
         [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate], [ApiNotes],
         [UpdatedAt], [UpdatedBy])
    VALUES
        (N'TWITTER', N'Twitter / X', N'Microblogging', N'bi-twitter-x', N'#000000',
         5, 1, 1, 0,
         NULL, NULL, NULL, NULL,
         NULL, NULL, NULL,
         NULL, NULL, NULL,
         N'Twitter/X API v2 — requires Developer Account and Elevated Access. Store API Key, API Secret, Access Token, and Access Token Secret here.',
         DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
    PRINT 'Seeded platform: TWITTER';
END
ELSE
BEGIN
    PRINT 'Platform TWITTER already exists. Skipping seed.';
END
GO

-- Google Business Profile  (AllowGlobalFallback=0, RequiresDoctorAccount=1)
IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [PlatformCode] = N'GOOGLE')
BEGIN
    INSERT INTO [dbo].[MediaPlatforms_CYH]
        ([PlatformCode], [PlatformName], [Category], [IconClass], [ColorHex],
         [SortOrder], [IsActive], [AllowGlobalFallback], [RequiresDoctorAccount],
         [GlobalApiEndpoint], [GlobalApiKey], [GlobalAppId], [GlobalAppSecret],
         [GlobalAccessToken], [GlobalPageOrAccountId], [GlobalPhoneNumberId],
         [GlobalRefreshToken], [GlobalTokenExpiresAt], [DefaultMessageTemplate], [ApiNotes],
         [UpdatedAt], [UpdatedBy])
    VALUES
        (N'GOOGLE', N'Google Business Profile', N'Local Search', N'bi-google', N'#4285F4',
         6, 1, 0, 1,
         NULL, NULL, NULL, NULL,
         NULL, NULL, NULL,
         NULL, NULL, NULL,
         N'Google Business Profile API — each doctor must link their own Google Business account. No global fallback allowed. AllowGlobalFallback=0, RequiresDoctorAccount=1.',
         DATEADD(minute, 330, SYSUTCDATETIME()), N'System');
    PRINT 'Seeded platform: GOOGLE';
END
ELSE
BEGIN
    PRINT 'Platform GOOGLE already exists. Skipping seed.';
END
GO

-- ============================================================================
-- 4. Stored Procedure: ManageMediaPlatforms_CYH
-- ============================================================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ManageMediaPlatforms_CYH]') AND type IN (N'P'))
BEGIN
    DROP PROCEDURE [dbo].[ManageMediaPlatforms_CYH];
    PRINT 'Existing procedure [ManageMediaPlatforms_CYH] dropped for recreation.';
END
GO

CREATE PROCEDURE [dbo].[ManageMediaPlatforms_CYH]
    -- Action flag (accepts either @Flag or @Action)
    @Flag                   NVARCHAR(50)    = NULL,
    @Action                 NVARCHAR(50)    = NULL,

    -- Lookup / toggle params
    @Id                     INT             = NULL,
    @UpdatedBy              NVARCHAR(200)   = NULL,

    -- Global API update params
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

    -- ----------------------------------------------------------------
    -- GET_ALL — returns all platforms ordered by SortOrder
    -- ----------------------------------------------------------------
    IF @Action = 'GET_ALL'
    BEGIN
        SELECT
            [Id],
            [PlatformCode],
            [PlatformName],
            [Category],
            [IconClass],
            [ColorHex],
            [SortOrder],
            [IsActive],
            [AllowGlobalFallback],
            [RequiresDoctorAccount],
            [GlobalApiEndpoint],
            [GlobalApiKey],
            [GlobalAppId],
            [GlobalAppSecret],
            [GlobalAccessToken],
            [GlobalPageOrAccountId],
            [GlobalPhoneNumberId],
            [GlobalRefreshToken],
            [GlobalTokenExpiresAt],
            [DefaultMessageTemplate],
            [ApiNotes],
            [UpdatedAt],
            [UpdatedBy]
        FROM [dbo].[MediaPlatforms_CYH]
        ORDER BY [SortOrder] ASC;
        RETURN;
    END

    -- ----------------------------------------------------------------
    -- GET_ACTIVE — returns only IsActive=1 platforms, ordered by SortOrder
    -- ----------------------------------------------------------------
    IF @Action = 'GET_ACTIVE'
    BEGIN
        SELECT
            [Id],
            [PlatformCode],
            [PlatformName],
            [Category],
            [IconClass],
            [ColorHex],
            [SortOrder],
            [IsActive],
            [AllowGlobalFallback],
            [RequiresDoctorAccount],
            [GlobalApiEndpoint],
            [GlobalApiKey],
            [GlobalAppId],
            [GlobalAppSecret],
            [GlobalAccessToken],
            [GlobalPageOrAccountId],
            [GlobalPhoneNumberId],
            [GlobalRefreshToken],
            [GlobalTokenExpiresAt],
            [DefaultMessageTemplate],
            [ApiNotes],
            [UpdatedAt],
            [UpdatedBy]
        FROM [dbo].[MediaPlatforms_CYH]
        WHERE [IsActive] = 1
        ORDER BY [SortOrder] ASC;
        RETURN;
    END

    -- ----------------------------------------------------------------
    -- GET_BY_ID — returns a single platform by Id
    -- ----------------------------------------------------------------
    IF @Action = 'GET_BY_ID'
    BEGIN
        IF @Id IS NULL
        BEGIN
            RAISERROR('GET_BY_ID requires @Id parameter.', 16, 1);
            RETURN;
        END

        SELECT
            [Id],
            [PlatformCode],
            [PlatformName],
            [Category],
            [IconClass],
            [ColorHex],
            [SortOrder],
            [IsActive],
            [AllowGlobalFallback],
            [RequiresDoctorAccount],
            [GlobalApiEndpoint],
            [GlobalApiKey],
            [GlobalAppId],
            [GlobalAppSecret],
            [GlobalAccessToken],
            [GlobalPageOrAccountId],
            [GlobalPhoneNumberId],
            [GlobalRefreshToken],
            [GlobalTokenExpiresAt],
            [DefaultMessageTemplate],
            [ApiNotes],
            [UpdatedAt],
            [UpdatedBy]
        FROM [dbo].[MediaPlatforms_CYH]
        WHERE [Id] = @Id;
        RETURN;
    END

    -- ----------------------------------------------------------------
    -- TOGGLE_ACTIVE — flips IsActive bit, records UpdatedAt (IST) and UpdatedBy
    -- ----------------------------------------------------------------
    IF @Action = 'TOGGLE_ACTIVE'
    BEGIN
        IF @Id IS NULL
        BEGIN
            RAISERROR('TOGGLE_ACTIVE requires @Id parameter.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('No platform found with the specified @Id.', 16, 1);
            RETURN;
        END

        UPDATE [dbo].[MediaPlatforms_CYH]
        SET
            [IsActive]  = CASE WHEN [IsActive] = 1 THEN 0 ELSE 1 END,
            [UpdatedAt] = DATEADD(minute, 330, SYSUTCDATETIME()),
            [UpdatedBy] = @UpdatedBy
        WHERE [Id] = @Id;

        -- Return updated row
        SELECT
            [Id],
            [PlatformCode],
            [PlatformName],
            [IsActive],
            [UpdatedAt],
            [UpdatedBy]
        FROM [dbo].[MediaPlatforms_CYH]
        WHERE [Id] = @Id;
        RETURN;
    END

    -- ----------------------------------------------------------------
    -- UPDATE_GLOBAL_API — updates all Global* fields + UpdatedAt + UpdatedBy
    -- ----------------------------------------------------------------
    IF @Action = 'UPDATE_GLOBAL_API'
    BEGIN
        IF @Id IS NULL
        BEGIN
            RAISERROR('UPDATE_GLOBAL_API requires @Id parameter.', 16, 1);
            RETURN;
        END

        IF NOT EXISTS (SELECT 1 FROM [dbo].[MediaPlatforms_CYH] WHERE [Id] = @Id)
        BEGIN
            RAISERROR('No platform found with the specified @Id.', 16, 1);
            RETURN;
        END

        UPDATE [dbo].[MediaPlatforms_CYH]
        SET
            [GlobalApiEndpoint]      = @GlobalApiEndpoint,
            [GlobalApiKey]           = @GlobalApiKey,
            [GlobalAppId]            = @GlobalAppId,
            [GlobalAppSecret]        = @GlobalAppSecret,
            [GlobalAccessToken]      = @GlobalAccessToken,
            [GlobalPageOrAccountId]  = @GlobalPageOrAccountId,
            [GlobalPhoneNumberId]    = @GlobalPhoneNumberId,
            [GlobalRefreshToken]     = @GlobalRefreshToken,
            [GlobalTokenExpiresAt]   = @GlobalTokenExpiresAt,
            [DefaultMessageTemplate] = @DefaultMessageTemplate,
            [ApiNotes]               = @ApiNotes,
            [UpdatedAt]              = DATEADD(minute, 330, SYSUTCDATETIME()),
            [UpdatedBy]              = @UpdatedBy
        WHERE [Id] = @Id;

        -- Return updated row
        SELECT
            [Id],
            [PlatformCode],
            [PlatformName],
            [GlobalApiEndpoint],
            [GlobalApiKey],
            [GlobalAppId],
            [GlobalAppSecret],
            [GlobalAccessToken],
            [GlobalPageOrAccountId],
            [GlobalPhoneNumberId],
            [GlobalRefreshToken],
            [GlobalTokenExpiresAt],
            [DefaultMessageTemplate],
            [ApiNotes],
            [UpdatedAt],
            [UpdatedBy]
        FROM [dbo].[MediaPlatforms_CYH]
        WHERE [Id] = @Id;
        RETURN;
    END

    -- ----------------------------------------------------------------
    -- Unknown action — raise error
    -- ----------------------------------------------------------------
    RAISERROR('Unknown @Action value: ''%s''. Valid actions are: GET_ALL, GET_ACTIVE, GET_BY_ID, TOGGLE_ACTIVE, UPDATE_GLOBAL_API.', 16, 1, @Action);
END
GO

PRINT 'Stored procedure [ManageMediaPlatforms_CYH] created successfully.';
GO

-- ============================================================================
-- END OF SCRIPT
-- ============================================================================
