using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace CYHDocBrandData;

public static class DbInitializer
{
    public static async Task InitializeAsync(string connectionString, ILogger? logger = null)
    {
        try
        {
            await using var con = new SqlConnection(connectionString);
            await con.OpenAsync();

            // 1. Ensure BannerDesigns_CYH and DoctorAiSettings_CYH tables exist
            var createTableSql = @"
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BannerDesigns_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[BannerDesigns_CYH] (
                        [Id]                 INT IDENTITY(1,1) NOT NULL,
                        [UserId]             NVARCHAR(450) NULL,
                        [DoctorName]         NVARCHAR(200) NOT NULL,
                        [HospitalName]       NVARCHAR(200) NULL,
                        [TemplateType]       NVARCHAR(100) NOT NULL,
                        [Heading]            NVARCHAR(500) NOT NULL,
                        [Subtitle]           NVARCHAR(MAX) NULL,
                        [Status]             NVARCHAR(50) NOT NULL DEFAULT ('Draft'),
                        [AspectRatio]        NVARCHAR(50) NOT NULL DEFAULT ('1:1'),
                        [PrimaryColor]       NVARCHAR(50) NOT NULL DEFAULT ('#00f2fe'),
                        [BackgroundImageUrl] NVARCHAR(MAX) NULL,
                        [ConfigJson]         NVARCHAR(MAX) NOT NULL,
                        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        [UpdatedAt]          DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        [PublishedAt]        DATETIME2(7) NULL,
                        CONSTRAINT [PK_BannerDesigns_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                END

                -- Ensure DoctorAiSettings_CYH table exists for Doctor-mapped AI API keys (Gemini, OpenAI, etc.)
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorAiSettings_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[DoctorAiSettings_CYH] (
                        [Id]                 INT IDENTITY(1,1) NOT NULL,
                        [DoctorId]           INT NULL,
                        [UserId]             NVARCHAR(450) NULL,
                        [DoctorName]         NVARCHAR(200) NULL,
                        [Provider]           NVARCHAR(50) NOT NULL,
                        [ApiKey]             NVARCHAR(500) NOT NULL,
                        [ModelName]          NVARCHAR(100) NULL,
                        [BaseUrl]            NVARCHAR(500) NULL,
                        [IsActive]           BIT NOT NULL DEFAULT (1),
                        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        [UpdatedAt]          DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        CONSTRAINT [PK_DoctorAiSettings_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    CREATE NONCLUSTERED INDEX [IX_DoctorAiSettings_DoctorId] ON [dbo].[DoctorAiSettings_CYH]([DoctorId]);
                    CREATE NONCLUSTERED INDEX [IX_DoctorAiSettings_UserId] ON [dbo].[DoctorAiSettings_CYH]([UserId]);
                END

                -- Ensure extended profile columns exist on DoctorProfiles table
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BannerCategories_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[BannerCategories_CYH] (
                        [Id]           INT IDENTITY(1,1) NOT NULL,
                        [CategoryName] NVARCHAR(100) NOT NULL,
                        [Slug]         NVARCHAR(100) NOT NULL,
                        [IconClass]    NVARCHAR(100) NOT NULL DEFAULT ('bi-folder-fill'),
                        [BadgeColor]   NVARCHAR(50)  NULL DEFAULT ('#00f2fe'),
                        [DisplayOrder] INT NOT NULL DEFAULT (0),
                        [IsActive]     BIT NOT NULL DEFAULT (1),
                        [CreatedAt]    DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        CONSTRAINT [PK_BannerCategories_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BannerTemplates_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[BannerTemplates_CYH] (
                        [Id]                 INT IDENTITY(1,1) NOT NULL,
                        [CategoryId]         INT NOT NULL,
                        [Title]              NVARCHAR(200) NOT NULL,
                        [StyleTheme]         NVARCHAR(100) NOT NULL,
                        [AspectRatio]        NVARCHAR(50) NOT NULL DEFAULT ('1:1'),
                        [BackgroundImageUrl] NVARCHAR(500) NOT NULL,
                        [ThumbnailUrl]       NVARCHAR(500) NULL,
                        [PrimaryColor]       NVARCHAR(50) NOT NULL DEFAULT ('#00f2fe'),
                        [SecondaryColor]     NVARCHAR(50) NOT NULL DEFAULT ('#0a192f'),
                        [TextColor]          NVARCHAR(50) NOT NULL DEFAULT ('#ffffff'),
                        [DefaultHeading]     NVARCHAR(500) NOT NULL,
                        [DefaultSubtitle]    NVARCHAR(MAX) NULL,
                        [BadgeText]          NVARCHAR(100) NULL,
                        [ConfigJson]         NVARCHAR(MAX) NOT NULL,
                        [DisplayOrder]       INT NOT NULL DEFAULT (0),
                        [IsActive]           BIT NOT NULL DEFAULT (1),
                        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        CONSTRAINT [PK_BannerTemplates_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                END

                -- Ensure extended profile columns exist on DoctorProfiles table
                IF COL_LENGTH('DoctorProfiles', 'DisplayName') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [DisplayName] NVARCHAR(100) NULL;
                IF COL_LENGTH('DoctorProfiles', 'Qualifications') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [Qualifications] NVARCHAR(200) NULL;
                IF COL_LENGTH('DoctorProfiles', 'MobileNo') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [MobileNo] NVARCHAR(50) NULL;
                IF COL_LENGTH('DoctorProfiles', 'ClinicName') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [ClinicName] NVARCHAR(200) NULL;
                IF COL_LENGTH('DoctorProfiles', 'ClinicAddress') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [ClinicAddress] NVARCHAR(500) NULL;
                IF COL_LENGTH('DoctorProfiles', 'City') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [City] NVARCHAR(100) NULL;
                IF COL_LENGTH('DoctorProfiles', 'State') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [State] NVARCHAR(100) NULL;
                IF COL_LENGTH('DoctorProfiles', 'PinCode') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [PinCode] NVARCHAR(20) NULL;
                IF COL_LENGTH('DoctorProfiles', 'WebsiteUrl') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [WebsiteUrl] NVARCHAR(500) NULL;
                IF COL_LENGTH('DoctorProfiles', 'ClinicLogo') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [ClinicLogo] NVARCHAR(MAX) NULL;
                IF COL_LENGTH('DoctorProfiles', 'PrimaryColorHex') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [PrimaryColorHex] NVARCHAR(50) NULL DEFAULT ('#00f2fe');
                IF COL_LENGTH('DoctorProfiles', 'SecondaryColorHex') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [SecondaryColorHex] NVARCHAR(50) NULL DEFAULT ('#4facfe');
                IF COL_LENGTH('DoctorProfiles', 'PreferredFont') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [PreferredFont] NVARCHAR(50) NULL DEFAULT ('Inter');
                IF COL_LENGTH('DoctorProfiles', 'AiCredits') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [AiCredits] INT NOT NULL DEFAULT (25);
                IF COL_LENGTH('DoctorProfiles', 'IsVerified') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [IsVerified] BIT NOT NULL DEFAULT (1);
                IF COL_LENGTH('DoctorProfiles', 'IsVerified') IS NULL
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [IsVerified] BIT NOT NULL DEFAULT (1);

                -- Credit System Tables
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditTypes_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[CreditTypes_CYH] (
                        [Id]           INT IDENTITY(1,1) NOT NULL,
                        [Name]         NVARCHAR(100) NOT NULL,
                        [Code]         NVARCHAR(100) NOT NULL,
                        [ServiceType]  NVARCHAR(50)  NOT NULL DEFAULT ('Free'),
                        [IsPaid]       BIT           NOT NULL DEFAULT (0),
                        [IsActive]     BIT           NOT NULL DEFAULT (1),
                        [Description]  NVARCHAR(500) NULL,
                        [CreditCost]   INT           NOT NULL DEFAULT (1),
                        [CreatedAt]    DATETIME2(7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
                        CONSTRAINT [PK_CreditTypes_CYH] PRIMARY KEY CLUSTERED ([Id] ASC),
                        CONSTRAINT [UQ_CreditTypes_Code] UNIQUE ([Code])
                    );
                END

                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorCredits_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[DoctorCredits_CYH] (
                        [Id]             INT IDENTITY(1,1) NOT NULL,
                        [DoctorId]       INT NOT NULL,
                        [CreditTypeId]   INT NOT NULL,
                        [TotalCredits]   INT NOT NULL DEFAULT (0),
                        [UsedCredits]    INT NOT NULL DEFAULT (0),
                        [IsActive]       BIT NOT NULL DEFAULT (1),
                        [LastUpdatedAt]  DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        CONSTRAINT [PK_DoctorCredits_CYH] PRIMARY KEY CLUSTERED ([Id] ASC),
                        CONSTRAINT [UQ_Doctor_CreditType] UNIQUE ([DoctorId], [CreditTypeId])
                    );
                    CREATE NONCLUSTERED INDEX [IX_DoctorCredits_DoctorId] ON [dbo].[DoctorCredits_CYH]([DoctorId]);
                END

                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditTransactions_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[CreditTransactions_CYH] (
                        [Id]              INT IDENTITY(1,1) NOT NULL,
                        [DoctorId]        INT NOT NULL,
                        [CreditTypeId]    INT NOT NULL,
                        [TransactionType] NVARCHAR(50)  NOT NULL,
                        [CreditsChanged]  INT           NOT NULL,
                        [BalanceBefore]   INT           NOT NULL,
                        [BalanceAfter]    INT           NOT NULL,
                        [ActionBy]        NVARCHAR(200) NULL,
                        [ReferenceId]     NVARCHAR(100) NULL,
                        [Note]            NVARCHAR(500) NULL,
                        [CreatedAt]       DATETIME2(7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
                        CONSTRAINT [PK_CreditTransactions_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    CREATE NONCLUSTERED INDEX [IX_CreditTransactions_DoctorId] ON [dbo].[CreditTransactions_CYH]([DoctorId]);
                    CREATE NONCLUSTERED INDEX [IX_CreditTransactions_CreatedAt] ON [dbo].[CreditTransactions_CYH]([CreatedAt] DESC);
                END

                -- Ensure PasswordChangeHistory_CYH table exists
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PasswordChangeHistory_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[PasswordChangeHistory_CYH] (
                        [Id]               INT IDENTITY(1,1) NOT NULL,
                        [UserId]           NVARCHAR(450)     NOT NULL,
                        [UserEmail]        NVARCHAR(256)     NULL,
                        [UserRole]         NVARCHAR(50)      NULL,
                        [IpAddress]        NVARCHAR(100)     NULL,
                        [City]             NVARCHAR(100)     NULL,
                        [Region]           NVARCHAR(100)     NULL,
                        [Country]          NVARCHAR(100)     NULL,
                        [Latitude]         DECIMAL(9,6)      NULL,
                        [Longitude]        DECIMAL(9,6)      NULL,
                        [LocationSummary]  NVARCHAR(300)     NULL,
                        [UserAgent]        NVARCHAR(500)     NULL,
                        [Remarks]          NVARCHAR(250)     NULL,
                        [ChangedAt]        DATETIME2(7)      NOT NULL DEFAULT (SYSUTCDATETIME()),
                        CONSTRAINT [PK_PasswordChangeHistory_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    CREATE NONCLUSTERED INDEX [IX_PasswordChangeHistory_UserId] ON [dbo].[PasswordChangeHistory_CYH]([UserId]);
                    CREATE NONCLUSTERED INDEX [IX_PasswordChangeHistory_ChangedAt] ON [dbo].[PasswordChangeHistory_CYH]([ChangedAt] DESC);
                END

                -- Social Accounts Table (Doctor Connections for OAuth & Tokens)
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorSocialAccounts_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[DoctorSocialAccounts_CYH] (
                        [Id]               INT IDENTITY(1,1) NOT NULL,
                        [DoctorId]         INT NULL,
                        [UserId]           NVARCHAR(450) NULL,
                        [DoctorName]       NVARCHAR(200) NULL,
                        [Platform]         NVARCHAR(50)  NOT NULL,
                        [AccountName]      NVARCHAR(200) NOT NULL,
                        [PageOrAccountId]  NVARCHAR(200) NOT NULL,
                        [AccessToken]      NVARCHAR(MAX) NOT NULL,
                        [RefreshToken]     NVARCHAR(MAX) NULL,
                        [TokenExpiresAt]   DATETIME2(7)  NULL,
                        [IsActive]         BIT           NOT NULL DEFAULT (1),
                        [ConnectedAt]      DATETIME2(7)  NOT NULL DEFAULT (SYSUTCDATETIME()),
                        [LastTestedAt]     DATETIME2(7)  NULL,
                        [LastTestStatus]   NVARCHAR(50)  NULL,
                        [LastTestMessage]  NVARCHAR(MAX) NULL,
                        CONSTRAINT [PK_DoctorSocialAccounts_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    CREATE NONCLUSTERED INDEX [IX_DoctorSocialAccounts_DoctorId] ON [dbo].[DoctorSocialAccounts_CYH]([DoctorId]);
                    CREATE NONCLUSTERED INDEX [IX_DoctorSocialAccounts_UserId] ON [dbo].[DoctorSocialAccounts_CYH]([UserId]);
                END

                -- Banner Schedules Master Table
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BannerSchedules_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[BannerSchedules_CYH] (
                        [Id]                 INT IDENTITY(1,1) NOT NULL,
                        [BannerDesignId]     INT NOT NULL,
                        [DoctorId]           INT NULL,
                        [UserId]             NVARCHAR(450) NULL,
                        [DoctorName]         NVARCHAR(200) NOT NULL,
                        [HospitalName]       NVARCHAR(200) NULL,
                        [BannerHeading]      NVARCHAR(500) NULL,
                        [ImageUrl]           NVARCHAR(MAX) NULL,
                        [Caption]            NVARCHAR(MAX) NULL,
                        [Platforms]          NVARCHAR(200) NOT NULL,
                        [ScheduledAt]        DATETIME2(7) NOT NULL,
                        [Status]             NVARCHAR(50) NOT NULL DEFAULT ('Pending'),
                        [ExecutionAttempts]  INT NOT NULL DEFAULT (0),
                        [LastExecutedAt]     DATETIME2(7) NULL,
                        [ErrorMessage]       NVARCHAR(MAX) NULL,
                        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (SYSUTCDATETIME()),
                        [CreatedBy]          NVARCHAR(200) NULL,
                        CONSTRAINT [PK_BannerSchedules_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    CREATE NONCLUSTERED INDEX [IX_BannerSchedules_ScheduledAt_Status] ON [dbo].[BannerSchedules_CYH]([ScheduledAt], [Status]);
                    CREATE NONCLUSTERED INDEX [IX_BannerSchedules_DoctorId] ON [dbo].[BannerSchedules_CYH]([DoctorId]);
                END

                -- Banner Schedule Details (Per-Platform Breakdown)
                IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BannerScheduleDetails_CYH]') AND type in (N'U'))
                BEGIN
                    CREATE TABLE [dbo].[BannerScheduleDetails_CYH] (
                        [Id]                INT IDENTITY(1,1) NOT NULL,
                        [BannerScheduleId]  INT NOT NULL,
                        [Platform]          NVARCHAR(50) NOT NULL,
                        [TargetAccountName] NVARCHAR(200) NULL,
                        [TargetPageId]      NVARCHAR(200) NULL,
                        [Status]            NVARCHAR(50) NOT NULL DEFAULT ('Pending'),
                        [PlatformPostId]    NVARCHAR(200) NULL,
                        [PostUrl]           NVARCHAR(MAX) NULL,
                        [ErrorMessage]      NVARCHAR(MAX) NULL,
                        [PublishedAt]       DATETIME2(7) NULL,
                        CONSTRAINT [PK_BannerScheduleDetails_CYH] PRIMARY KEY CLUSTERED ([Id] ASC),
                        CONSTRAINT [FK_BannerScheduleDetails_Master] FOREIGN KEY ([BannerScheduleId]) REFERENCES [dbo].[BannerSchedules_CYH]([Id]) ON DELETE CASCADE
                    );
                    CREATE NONCLUSTERED INDEX [IX_BannerScheduleDetails_ScheduleId] ON [dbo].[BannerScheduleDetails_CYH]([BannerScheduleId]);
                END";

            await using (var cmdTable = new SqlCommand(createTableSql, con))
            {
                await cmdTable.ExecuteNonQueryAsync();
            }

            // 2. Ensure AuthenticateDoctor_CYH Stored Procedure exists & updated
            var spAuthSql = @"
                CREATE OR ALTER PROCEDURE dbo.AuthenticateDoctor_CYH
                    @Flag NVARCHAR(50),
                    @Email NVARCHAR(256),
                    @Password NVARCHAR(MAX)
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF @Flag = 'LOGIN'
                    BEGIN
                        SELECT TOP 1
                            u.Id AS UserId,
                            u.Email AS Email,
                            u.FullName AS FullName,
                            u.Role AS Role,
                            u.Department AS Department,
                            u.IsActive AS IsActive,
                            u.AvatarUrl AS AvatarUrl,
                            d.Id AS DoctorProfileId
                        FROM AspNetUsers u
                        LEFT JOIN DoctorProfiles d ON u.Id = d.UserId
                        WHERE (u.Email = @Email OR u.UserName = @Email)
                          AND u.Password = @Password;
                    END
                END";

            await using (var cmdSpAuth = new SqlCommand(spAuthSql, con))
            {
                await cmdSpAuth.ExecuteNonQueryAsync();
            }

            // 3. Ensure ManageDoctor_CYH Stored Procedure exists & updated
            var spDoctorSql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageDoctor_CYH
                    @Flag NVARCHAR(50),
                    @DoctorId INT = NULL,
                    @UserId NVARCHAR(450) = NULL,
                    @FullName NVARCHAR(100) = NULL,
                    @Specialization NVARCHAR(100) = NULL,
                    @LicenseNumber NVARCHAR(50) = NULL,
                    @ExperienceYears INT = NULL,
                    @ConsultationFee DECIMAL(18,2) = NULL,
                    @AvailableDays NVARCHAR(100) = NULL,
                    @AvailableHours NVARCHAR(50) = NULL,
                    @Bio NVARCHAR(MAX) = NULL,
                    @IsAvailableToday BIT = NULL,
                    @IsActive BIT = NULL,
                    @Email NVARCHAR(256) = NULL,
                    @PhoneNumber NVARCHAR(50) = NULL,
                    @Password NVARCHAR(MAX) = NULL,
                    @ProfilePic NVARCHAR(MAX) = NULL,
                    @DisplayName NVARCHAR(100) = NULL,
                    @Qualifications NVARCHAR(200) = NULL,
                    @MobileNo NVARCHAR(50) = NULL,
                    @ClinicName NVARCHAR(200) = NULL,
                    @ClinicAddress NVARCHAR(500) = NULL,
                    @City NVARCHAR(100) = NULL,
                    @State NVARCHAR(100) = NULL,
                    @PinCode NVARCHAR(20) = NULL,
                    @WebsiteUrl NVARCHAR(500) = NULL,
                    @ClinicLogo NVARCHAR(MAX) = NULL,
                    @PrimaryColorHex NVARCHAR(50) = NULL,
                    @SecondaryColorHex NVARCHAR(50) = NULL,
                    @PreferredFont NVARCHAR(50) = NULL,
                    @AiCredits INT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- 1. Get all doctors with user accounts
                    IF @Flag = 'GET_ALL'
                    BEGIN
                        SELECT 
                            d.Id, d.UserId, d.FullName, d.Specialization, d.LicenseNumber, d.ExperienceYears,
                            d.ConsultationFee, d.AvailableDays, d.AvailableHours, d.Bio, d.IsAvailableToday,
                            d.CreatedAt, d.ProfilePic,
                            d.DisplayName, d.Qualifications, d.MobileNo, d.ClinicName, d.ClinicAddress,
                            d.City, d.State, d.PinCode, d.WebsiteUrl, d.ClinicLogo,
                            d.PrimaryColorHex, d.SecondaryColorHex, d.PreferredFont, d.AiCredits, d.IsVerified,
                            u.Id AS AppUserId, u.UserName, u.Email, u.PhoneNumber, u.FullName AS UserFullName,
                            u.Role, u.Department, u.IsActive, u.AvatarUrl, u.Password
                        FROM DoctorProfiles d
                        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
                        ORDER BY d.FullName ASC;
                    END

                    -- 2. Get doctor by DoctorProfile Id
                    ELSE IF @Flag = 'GET_BY_ID'
                    BEGIN
                        SELECT 
                            d.Id, d.UserId, d.FullName, d.Specialization, d.LicenseNumber, d.ExperienceYears,
                            d.ConsultationFee, d.AvailableDays, d.AvailableHours, d.Bio, d.IsAvailableToday,
                            d.CreatedAt, d.ProfilePic,
                            d.DisplayName, d.Qualifications, d.MobileNo, d.ClinicName, d.ClinicAddress,
                            d.City, d.State, d.PinCode, d.WebsiteUrl, d.ClinicLogo,
                            d.PrimaryColorHex, d.SecondaryColorHex, d.PreferredFont, d.AiCredits, d.IsVerified,
                            u.Id AS AppUserId, u.UserName, u.Email, u.PhoneNumber, u.FullName AS UserFullName,
                            u.Role, u.Department, u.IsActive, u.AvatarUrl, u.Password
                        FROM DoctorProfiles d
                        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
                        WHERE d.Id = @DoctorId;
                    END

                    -- 3. Get doctor by AspNetUsers UserId
                    ELSE IF @Flag = 'GET_BY_USERID'
                    BEGIN
                        SELECT 
                            d.Id, d.UserId, d.FullName, d.Specialization, d.LicenseNumber, d.ExperienceYears,
                            d.ConsultationFee, d.AvailableDays, d.AvailableHours, d.Bio, d.IsAvailableToday,
                            d.CreatedAt, d.ProfilePic,
                            d.DisplayName, d.Qualifications, d.MobileNo, d.ClinicName, d.ClinicAddress,
                            d.City, d.State, d.PinCode, d.WebsiteUrl, d.ClinicLogo,
                            d.PrimaryColorHex, d.SecondaryColorHex, d.PreferredFont, d.AiCredits, d.IsVerified,
                            u.Id AS AppUserId, u.UserName, u.Email, u.PhoneNumber, u.FullName AS UserFullName,
                            u.Role, u.Department, u.IsActive, u.AvatarUrl, u.Password
                        FROM DoctorProfiles d
                        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
                        WHERE d.UserId = @UserId;
                    END

                    -- 4. Toggle on-duty availability
                    ELSE IF @Flag = 'TOGGLE_DUTY'
                    BEGIN
                        UPDATE DoctorProfiles
                        SET IsAvailableToday = CASE WHEN IsAvailableToday = 1 THEN 0 ELSE 1 END
                        WHERE Id = @DoctorId;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 5. Toggle active status for doctor
                    ELSE IF @Flag = 'TOGGLE_ACTIVE'
                    BEGIN
                        UPDATE AspNetUsers
                        SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                        WHERE Id = @UserId;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 6. Set active status explicitly
                    ELSE IF @Flag = 'SET_ACTIVE_STATUS'
                    BEGIN
                        UPDATE AspNetUsers
                        SET IsActive = @IsActive
                        WHERE Id = @UserId;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 7. Insert new DoctorProfile
                    ELSE IF @Flag = 'INSERT'
                    BEGIN
                        INSERT INTO DoctorProfiles 
                        (
                            UserId, FullName, Specialization, LicenseNumber, ExperienceYears, 
                            ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt, ProfilePic,
                            DisplayName, Qualifications, MobileNo, ClinicName, ClinicAddress, City, State, PinCode, WebsiteUrl, ClinicLogo,
                            PrimaryColorHex, SecondaryColorHex, PreferredFont, AiCredits, IsVerified
                        )
                        VALUES 
                        (
                            @UserId, @FullName, @Specialization, @LicenseNumber, @ExperienceYears,
                            @ConsultationFee, @AvailableDays, @AvailableHours, @Bio, ISNULL(@IsAvailableToday, 1), SYSUTCDATETIME(), @ProfilePic,
                            ISNULL(@DisplayName, @FullName), @Qualifications, @MobileNo, @ClinicName, @ClinicAddress, @City, @State, @PinCode, @WebsiteUrl, @ClinicLogo,
                            ISNULL(@PrimaryColorHex, '#00f2fe'), ISNULL(@SecondaryColorHex, '#4facfe'), ISNULL(@PreferredFont, 'Inter'), ISNULL(@AiCredits, 25), 1
                        );

                        SELECT SCOPE_IDENTITY() AS NewId;
                    END

                    -- 8. Register new Doctor (Atomic transaction with inactive state by default)
                    ELSE IF @Flag = 'REGISTER_DOCTOR'
                    BEGIN
                        IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @Email OR UserName = @Email)
                        BEGIN
                            SELECT 0 AS StatusCode, 'An account with this email address already exists. Please sign in or use another email.' AS Message, NULL AS UserId;
                            RETURN;
                        END

                        DECLARE @NewUserId NVARCHAR(450) = ISNULL(@UserId, CAST(NEWID() AS NVARCHAR(450)));
                        DECLARE @Now DATETIME2(7) = SYSUTCDATETIME();

                        BEGIN TRANSACTION;
                        BEGIN TRY
                            INSERT INTO AspNetUsers 
                            (
                                Id, UserName, NormalizedUserName, Email, NormalizedEmail, 
                                EmailConfirmed, FullName, Role, Department, PhoneNumber, 
                                Password, CreatedAt, IsActive, AccessFailedCount, LockoutEnabled, 
                                PhoneNumberConfirmed, TwoFactorEnabled
                            )
                            VALUES 
                            (
                                @NewUserId, @Email, UPPER(@Email), @Email, UPPER(@Email), 
                                1, @FullName, 'Doctor', @Specialization, @PhoneNumber, 
                                @Password, @Now, 0, 0, 0, 
                                0, 0
                            );

                            INSERT INTO DoctorProfiles 
                            (
                                UserId, FullName, Specialization, LicenseNumber, ExperienceYears, 
                                ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt,
                                DisplayName, Qualifications, PrimaryColorHex, SecondaryColorHex, PreferredFont, AiCredits, IsVerified
                            )
                            VALUES 
                            (
                                @NewUserId, @FullName, @Specialization, ISNULL(@LicenseNumber, ''), ISNULL(@ExperienceYears, 0), 
                                ISNULL(@ConsultationFee, 0.00), 
                                ISNULL(NULLIF(@AvailableDays, ''), 'Mon - Sat'), 
                                ISNULL(NULLIF(@AvailableHours, ''), '09:00 AM - 05:00 PM'), 
                                @Bio, 0, @Now,
                                @FullName, 'MBBS', '#00f2fe', '#4facfe', 'Inter', 25, 0
                            );

                            DECLARE @NewDocId INT = SCOPE_IDENTITY();
                            EXEC dbo.ManageCredits_CYH @Flag = 'INIT_DOCTOR_CREDITS', @DoctorId = @NewDocId;
                            COMMIT TRANSACTION;
                            SELECT 1 AS StatusCode, 'Doctor registered successfully (Pending Approval).' AS Message, @NewUserId AS UserId;
                        END TRY
                        BEGIN CATCH
                            ROLLBACK TRANSACTION;
                            SELECT 0 AS StatusCode, ERROR_MESSAGE() AS Message, NULL AS UserId;
                        END CATCH
                    END

                    -- 9. Update Doctor Profile & Avatar
                    ELSE IF @Flag = 'UPDATE_PROFILE'
                    BEGIN
                        DECLARE @TargetDocId INT = @DoctorId;
                        IF (@TargetDocId IS NULL OR @TargetDocId <= 0) AND @UserId IS NOT NULL
                        BEGIN
                            SELECT TOP 1 @TargetDocId = Id FROM DoctorProfiles WHERE UserId = @UserId;
                        END

                        IF @TargetDocId IS NOT NULL AND @TargetDocId > 0
                        BEGIN
                            UPDATE DoctorProfiles 
                            SET FullName = ISNULL(@FullName, FullName), 
                                DisplayName = COALESCE(@DisplayName, DisplayName, @FullName, FullName),
                                Specialization = ISNULL(@Specialization, Specialization), 
                                Qualifications = COALESCE(@Qualifications, Qualifications),
                                LicenseNumber = ISNULL(@LicenseNumber, LicenseNumber), 
                                ExperienceYears = ISNULL(@ExperienceYears, ExperienceYears), 
                                ConsultationFee = ISNULL(@ConsultationFee, ConsultationFee), 
                                AvailableDays = ISNULL(@AvailableDays, AvailableDays), 
                                AvailableHours = ISNULL(@AvailableHours, AvailableHours), 
                                Bio = @Bio,
                                ProfilePic = COALESCE(@ProfilePic, ProfilePic),
                                MobileNo = COALESCE(@MobileNo, MobileNo),
                                ClinicName = COALESCE(@ClinicName, ClinicName),
                                ClinicAddress = COALESCE(@ClinicAddress, ClinicAddress),
                                City = COALESCE(@City, City),
                                State = COALESCE(@State, State),
                                PinCode = COALESCE(@PinCode, PinCode),
                                WebsiteUrl = COALESCE(@WebsiteUrl, WebsiteUrl),
                                ClinicLogo = COALESCE(@ClinicLogo, ClinicLogo),
                                PrimaryColorHex = COALESCE(@PrimaryColorHex, PrimaryColorHex, '#00f2fe'),
                                SecondaryColorHex = COALESCE(@SecondaryColorHex, SecondaryColorHex, '#4facfe'),
                                PreferredFont = COALESCE(@PreferredFont, PreferredFont, 'Inter')
                            WHERE Id = @TargetDocId;

                            DECLARE @UpdatedRows INT = @@ROWCOUNT;

                            IF @ProfilePic IS NOT NULL AND @ProfilePic <> ''
                            BEGIN
                                UPDATE AspNetUsers
                                SET AvatarUrl = @ProfilePic
                                WHERE Id = (SELECT TOP 1 UserId FROM DoctorProfiles WHERE Id = @TargetDocId);
                            END

                            IF @FullName IS NOT NULL AND @FullName <> ''
                            BEGIN
                                UPDATE AspNetUsers
                                SET FullName = @FullName
                                WHERE Id = (SELECT TOP 1 UserId FROM DoctorProfiles WHERE Id = @TargetDocId);
                            END

                            SELECT ISNULL(@UpdatedRows, 1) AS RowsAffected;
                        END
                        ELSE
                        BEGIN
                            SELECT 0 AS RowsAffected;
                        END
                    END

                    -- 10. Initial Seed Data
                    ELSE IF @Flag = 'SEED_INITIAL_DATA'
                    BEGIN
                        IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'admin@cyhdoctorhub.com')
                        BEGIN
                            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, FullName, Role, Department, Password, CreatedAt, IsActive, AccessFailedCount, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled)
                            VALUES (
                                CAST(NEWID() AS NVARCHAR(450)), 'admin@cyhdoctorhub.com', 'ADMIN@CYHDOCTORHUB.COM', 'admin@cyhdoctorhub.com', 'ADMIN@CYHDOCTORHUB.COM', 1,
                                'Hospital Chief Administrator', 'Admin', 'Hospital Operations & Executive',
                                'L4Yq5jzhlap0wTagsBRCUg==', SYSUTCDATETIME(), 1, 0, 0, 0, 0
                            );
                        END
                        ELSE
                        BEGIN
                            UPDATE AspNetUsers SET Password = 'L4Yq5jzhlap0wTagsBRCUg==' WHERE Email = 'admin@cyhdoctorhub.com';
                        END

                        IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'doctor@cyhdoctorhub.com')
                        BEGIN
                            DECLARE @Doc1UserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
                            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, FullName, Role, Department, Password, CreatedAt, IsActive, AccessFailedCount, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled)
                            VALUES (
                                @Doc1UserId, 'doctor@cyhdoctorhub.com', 'DOCTOR@CYHDOCTORHUB.COM', 'doctor@cyhdoctorhub.com', 'DOCTOR@CYHDOCTORHUB.COM', 1,
                                'Dr. A. Sharma', 'Doctor', 'Cardiology',
                                'n7uoG53mus5DzCXmNgv7NQ==', SYSUTCDATETIME(), 1, 0, 0, 0, 0
                            );

                            INSERT INTO DoctorProfiles (UserId, FullName, Specialization, LicenseNumber, ExperienceYears, ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt, DisplayName, Qualifications, PrimaryColorHex, SecondaryColorHex, PreferredFont, AiCredits, IsVerified)
                            VALUES (
                                @Doc1UserId, 'Dr. A. Sharma', 'Cardiology', 'MCI-2015-88214', 12, 800.00, 'Mon - Sat', '09:00 AM - 02:00 PM',
                                'Senior Interventional Cardiologist specializing in preventive cardiology and coronary angioplasty.', 1, SYSUTCDATETIME(),
                                'Dr. A. Sharma, MD', 'MBBS, MD (Cardiology)', '#00f2fe', '#4facfe', 'Inter', 25, 1
                            );
                        END
                        ELSE
                        BEGIN
                            UPDATE AspNetUsers SET Password = 'n7uoG53mus5DzCXmNgv7NQ==' WHERE Email = 'doctor@cyhdoctorhub.com';
                        END
                    END
                END";

            await using (var cmdSpDoc = new SqlCommand(spDoctorSql, con))
            {
                await cmdSpDoc.ExecuteNonQueryAsync();
            }

            // 4. Ensure ManageBannerDesigns_CYH Stored Procedure exists
            var spBannerSql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageBannerDesigns_CYH
                    @Flag NVARCHAR(50),
                    @Id INT = NULL,
                    @UserId NVARCHAR(450) = NULL,
                    @DoctorName NVARCHAR(200) = NULL,
                    @HospitalName NVARCHAR(200) = NULL,
                    @TemplateType NVARCHAR(100) = NULL,
                    @Heading NVARCHAR(500) = NULL,
                    @Subtitle NVARCHAR(MAX) = NULL,
                    @Status NVARCHAR(50) = NULL,
                    @AspectRatio NVARCHAR(50) = NULL,
                    @PrimaryColor NVARCHAR(50) = NULL,
                    @BackgroundImageUrl NVARCHAR(MAX) = NULL,
                    @ConfigJson NVARCHAR(MAX) = NULL,
                    @Search NVARCHAR(200) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF @Flag = 'GET_ALL'
                    BEGIN
                        SELECT 
                            Id, UserId, DoctorName, HospitalName, TemplateType, Heading, Subtitle, 
                            Status, AspectRatio, PrimaryColor, BackgroundImageUrl, 
                            CreatedAt, UpdatedAt, PublishedAt
                        FROM [dbo].[BannerDesigns_CYH]
                        WHERE (1=1)
                          AND (@UserId IS NULL OR UserId = @UserId OR UserId IS NULL)
                          AND (@Status IS NULL OR @Status = 'All' OR Status = @Status)
                          AND (
                                @Search IS NULL 
                                OR Heading LIKE '%' + @Search + '%' 
                                OR DoctorName LIKE '%' + @Search + '%' 
                                OR HospitalName LIKE '%' + @Search + '%' 
                                OR TemplateType LIKE '%' + @Search + '%'
                              )
                        ORDER BY UpdatedAt DESC;
                    END

                    ELSE IF @Flag = 'GET_BY_ID'
                    BEGIN
                        SELECT * 
                        FROM [dbo].[BannerDesigns_CYH]
                        WHERE Id = @Id;
                    END

                    ELSE IF @Flag IN ('UPSERT', 'SAVE_DRAFT')
                    BEGIN
                        IF @Id IS NOT NULL AND @Id > 0 AND EXISTS (SELECT 1 FROM [dbo].[BannerDesigns_CYH] WHERE Id = @Id)
                        BEGIN
                            UPDATE [dbo].[BannerDesigns_CYH]
                            SET 
                                DoctorName = ISNULL(@DoctorName, DoctorName),
                                HospitalName = ISNULL(@HospitalName, HospitalName),
                                TemplateType = ISNULL(@TemplateType, TemplateType),
                                Heading = ISNULL(@Heading, Heading),
                                Subtitle = ISNULL(@Subtitle, Subtitle),
                                Status = ISNULL(@Status, Status),
                                AspectRatio = ISNULL(@AspectRatio, AspectRatio),
                                PrimaryColor = ISNULL(@PrimaryColor, PrimaryColor),
                                BackgroundImageUrl = ISNULL(@BackgroundImageUrl, BackgroundImageUrl),
                                ConfigJson = ISNULL(@ConfigJson, ConfigJson),
                                UpdatedAt = SYSUTCDATETIME(),
                                PublishedAt = CASE WHEN @Status = 'Published' AND PublishedAt IS NULL THEN SYSUTCDATETIME() ELSE PublishedAt END
                            WHERE Id = @Id;

                            SELECT @Id AS ResultId;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO [dbo].[BannerDesigns_CYH] 
                            (
                                UserId, DoctorName, HospitalName, TemplateType, Heading, Subtitle,
                                Status, AspectRatio, PrimaryColor, BackgroundImageUrl, ConfigJson,
                                CreatedAt, UpdatedAt, PublishedAt
                            )
                            VALUES 
                            (
                                @UserId, @DoctorName, @HospitalName, @TemplateType, @Heading, @Subtitle,
                                ISNULL(@Status, 'Draft'), ISNULL(@AspectRatio, '1:1'), ISNULL(@PrimaryColor, '#00f2fe'),
                                @BackgroundImageUrl, @ConfigJson,
                                SYSUTCDATETIME(), SYSUTCDATETIME(),
                                CASE WHEN @Status = 'Published' THEN SYSUTCDATETIME() ELSE NULL END
                            );

                            SELECT SCOPE_IDENTITY() AS ResultId;
                        END
                    END

                    ELSE IF @Flag IN ('UPDATE_STATUS', 'PUBLISH')
                    BEGIN
                        DECLARE @NewStatus NVARCHAR(50) = ISNULL(@Status, CASE WHEN @Flag = 'PUBLISH' THEN 'Published' ELSE 'Draft' END);
                        UPDATE [dbo].[BannerDesigns_CYH]
                        SET 
                            Status = @NewStatus,
                            UpdatedAt = SYSUTCDATETIME(),
                            PublishedAt = CASE WHEN @NewStatus = 'Published' AND PublishedAt IS NULL THEN SYSUTCDATETIME() ELSE PublishedAt END
                        WHERE Id = @Id;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    ELSE IF @Flag = 'DELETE'
                    BEGIN
                        DELETE FROM [dbo].[BannerDesigns_CYH]
                        WHERE Id = @Id;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                END";

            await using (var cmdSp = new SqlCommand(spBannerSql, con))
            {
                await cmdSp.ExecuteNonQueryAsync();
            }

            // 5. Ensure ManageDoctorAiSettings_CYH Stored Procedure exists
            var spDoctorAiSql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageDoctorAiSettings_CYH
                    @Flag NVARCHAR(50),
                    @Id INT = NULL,
                    @DoctorId INT = NULL,
                    @UserId NVARCHAR(450) = NULL,
                    @DoctorName NVARCHAR(200) = NULL,
                    @Provider NVARCHAR(50) = NULL,
                    @ApiKey NVARCHAR(500) = NULL,
                    @ModelName NVARCHAR(100) = NULL,
                    @BaseUrl NVARCHAR(500) = NULL,
                    @IsActive BIT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- 1. Get Active Setting for doctor or hospital fallback
                    IF @Flag = 'GET_ACTIVE'
                    BEGIN
                        SELECT TOP 1 
                            s.Id, s.DoctorId, s.UserId, s.DoctorName, s.Provider, s.ApiKey, s.ModelName, s.BaseUrl, s.IsActive, s.CreatedAt, s.UpdatedAt
                        FROM DoctorAiSettings_CYH s
                        WHERE s.IsActive = 1
                          AND (
                                (@Provider IS NULL OR UPPER(LTRIM(RTRIM(s.Provider))) = UPPER(LTRIM(RTRIM(@Provider))))
                              )
                          AND (
                                (@DoctorId IS NOT NULL AND s.DoctorId = @DoctorId)
                             OR (@UserId IS NOT NULL AND s.UserId = @UserId)
                             OR (s.DoctorId IS NULL)
                          )
                        ORDER BY 
                            CASE WHEN s.DoctorId = @DoctorId THEN 0 
                                 WHEN s.UserId = @UserId THEN 1 
                                 ELSE 2 END ASC,
                            s.UpdatedAt DESC;
                    END

                    -- 2. Get all settings for a specific doctor (includes doctor specific + system global)
                    ELSE IF @Flag = 'GET_BY_DOCTOR'
                    BEGIN
                        SELECT Id, DoctorId, UserId, DoctorName, Provider, ApiKey, ModelName, BaseUrl, IsActive, CreatedAt, UpdatedAt
                        FROM DoctorAiSettings_CYH
                        WHERE ((@DoctorId IS NOT NULL AND DoctorId = @DoctorId) 
                               OR (@UserId IS NOT NULL AND UserId = @UserId)
                               OR (DoctorId IS NULL))
                        ORDER BY CASE WHEN DoctorId IS NOT NULL THEN 0 ELSE 1 END, Provider ASC;
                    END

                    -- 3. Get all settings (for Admin view)
                    ELSE IF @Flag = 'GET_ALL'
                    BEGIN
                        SELECT s.Id, s.DoctorId, s.UserId, 
                               COALESCE(s.DoctorName, d.FullName, 'Hospital Global') AS DoctorName, 
                               s.Provider, s.ApiKey, s.ModelName, s.BaseUrl, s.IsActive, s.CreatedAt, s.UpdatedAt
                        FROM DoctorAiSettings_CYH s
                        LEFT JOIN DoctorProfiles d ON s.DoctorId = d.Id
                        ORDER BY s.UpdatedAt DESC;
                    END

                    -- 4. Upsert (Save or Update) Doctor AI Key
                    ELSE IF @Flag = 'UPSERT'
                    BEGIN
                        DECLARE @ExistingId INT = NULL;

                        IF @Id IS NOT NULL AND @Id > 0
                        BEGIN
                            SET @ExistingId = @Id;
                        END
                        ELSE
                        BEGIN
                            SELECT TOP 1 @ExistingId = Id 
                            FROM DoctorAiSettings_CYH
                            WHERE UPPER(LTRIM(RTRIM(Provider))) = UPPER(LTRIM(RTRIM(@Provider)))
                              AND (
                                    (@DoctorId IS NOT NULL AND DoctorId = @DoctorId) 
                                 OR (@UserId IS NOT NULL AND UserId = @UserId)
                                 OR (@DoctorId IS NULL AND @UserId IS NULL AND DoctorId IS NULL)
                              );
                        END

                        IF @ExistingId IS NOT NULL
                        BEGIN
                            UPDATE DoctorAiSettings_CYH
                            SET ApiKey = @ApiKey,
                                DoctorName = COALESCE(@DoctorName, DoctorName),
                                ModelName = COALESCE(@ModelName, ModelName),
                                BaseUrl = COALESCE(@BaseUrl, BaseUrl),
                                IsActive = ISNULL(@IsActive, 1),
                                UpdatedAt = SYSUTCDATETIME()
                            WHERE Id = @ExistingId;

                            SELECT @ExistingId AS ResultId;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO DoctorAiSettings_CYH (DoctorId, UserId, DoctorName, Provider, ApiKey, ModelName, BaseUrl, IsActive, CreatedAt, UpdatedAt)
                            VALUES (@DoctorId, @UserId, @DoctorName, @Provider, @ApiKey, @ModelName, @BaseUrl, ISNULL(@IsActive, 1), SYSUTCDATETIME(), SYSUTCDATETIME());

                            SELECT SCOPE_IDENTITY() AS ResultId;
                        END
                    END

                    -- 5. Delete Setting
                    ELSE IF @Flag = 'DELETE'
                    BEGIN
                        DELETE FROM DoctorAiSettings_CYH WHERE Id = @Id;
                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                END";

            await using (var cmdSp = new SqlCommand(spDoctorAiSql, con))
            {
                await cmdSp.ExecuteNonQueryAsync();
            }

            // 6. Ensure DecryptDocPassword Stored Procedure exists
            var spDecryptSql = @"
                CREATE OR ALTER PROCEDURE dbo.DecryptDocPassword
                    @EncryptedPassword NVARCHAR(MAX) = NULL,
                    @Email NVARCHAR(256) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    DECLARE @Cipher NVARCHAR(MAX) = LTRIM(RTRIM(@EncryptedPassword));

                    IF @Cipher IS NULL AND @Email IS NOT NULL
                    BEGIN
                        SELECT TOP 1 @Cipher = Password 
                        FROM AspNetUsers 
                        WHERE Email = @Email OR UserName = @Email;
                    END

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
                                ELSE 'Encrypted'
                            END AS DecryptedPassword,
                            u.IsActive
                        FROM AspNetUsers u
                        WHERE u.Role = 'Doctor'
                        ORDER BY u.FullName;
                        RETURN;
                    END

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

                    SELECT @Cipher AS EncryptedPassword, 'Decryption tool available in Admin portal' AS DecryptedPassword;
                END";

            await using (var cmdDecrypt = new SqlCommand(spDecryptSql, con))
            {
                await cmdDecrypt.ExecuteNonQueryAsync();
            }

            // 7. Ensure ManageBannerCategories_CYH and ManageBannerTemplates_CYH Stored Procedures exist
            var spCategorySql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageBannerCategories_CYH
                    @Flag NVARCHAR(50),
                    @Id INT = NULL,
                    @CategoryName NVARCHAR(100) = NULL,
                    @Slug NVARCHAR(100) = NULL,
                    @IconClass NVARCHAR(100) = NULL,
                    @BadgeColor NVARCHAR(50) = NULL,
                    @DisplayOrder INT = NULL,
                    @IsActive BIT = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF @Flag = 'GET_ACTIVE_WITH_COUNTS'
                    BEGIN
                        SELECT 
                            c.Id, 
                            c.CategoryName, 
                            c.Slug, 
                            c.IconClass, 
                            c.BadgeColor, 
                            c.DisplayOrder, 
                            c.IsActive, 
                            c.CreatedAt,
                            COUNT(t.Id) AS TemplateCount
                        FROM [dbo].[BannerCategories_CYH] c
                        LEFT JOIN [dbo].[BannerTemplates_CYH] t ON c.Id = t.CategoryId AND t.IsActive = 1
                        WHERE c.IsActive = 1
                        GROUP BY c.Id, c.CategoryName, c.Slug, c.IconClass, c.BadgeColor, c.DisplayOrder, c.IsActive, c.CreatedAt
                        ORDER BY c.DisplayOrder ASC, c.CategoryName ASC;
                    END

                    ELSE IF @Flag = 'GET_ALL'
                    BEGIN
                        SELECT 
                            c.Id, 
                            c.CategoryName, 
                            c.Slug, 
                            c.IconClass, 
                            c.BadgeColor, 
                            c.DisplayOrder, 
                            c.IsActive, 
                            c.CreatedAt,
                            COUNT(t.Id) AS TemplateCount
                        FROM [dbo].[BannerCategories_CYH] c
                        LEFT JOIN [dbo].[BannerTemplates_CYH] t ON c.Id = t.CategoryId
                        GROUP BY c.Id, c.CategoryName, c.Slug, c.IconClass, c.BadgeColor, c.DisplayOrder, c.IsActive, c.CreatedAt
                        ORDER BY c.DisplayOrder ASC;
                    END

                    ELSE IF @Flag = 'GET_BY_ID'
                    BEGIN
                        SELECT c.*, 0 AS TemplateCount
                        FROM [dbo].[BannerCategories_CYH] c
                        WHERE c.Id = @Id;
                    END

                    ELSE IF @Flag = 'INSERT'
                    BEGIN
                        INSERT INTO [dbo].[BannerCategories_CYH] 
                        (CategoryName, Slug, IconClass, BadgeColor, DisplayOrder, IsActive, CreatedAt)
                        VALUES 
                        (@CategoryName, @Slug, ISNULL(@IconClass, 'bi-folder-fill'), ISNULL(@BadgeColor, '#00f2fe'), ISNULL(@DisplayOrder, 0), ISNULL(@IsActive, 1), SYSUTCDATETIME());

                        SELECT SCOPE_IDENTITY() AS ResultId;
                    END

                    ELSE IF @Flag = 'UPDATE'
                    BEGIN
                        UPDATE [dbo].[BannerCategories_CYH]
                        SET CategoryName = ISNULL(@CategoryName, CategoryName),
                            Slug = ISNULL(@Slug, Slug),
                            IconClass = ISNULL(@IconClass, IconClass),
                            BadgeColor = ISNULL(@BadgeColor, BadgeColor),
                            DisplayOrder = ISNULL(@DisplayOrder, DisplayOrder),
                            IsActive = ISNULL(@IsActive, IsActive)
                        WHERE Id = @Id;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    ELSE IF @Flag = 'DELETE'
                    BEGIN
                        DELETE FROM [dbo].[BannerCategories_CYH] WHERE Id = @Id;
                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                END";

            await using (var cmdCat = new SqlCommand(spCategorySql, con))
            {
                await cmdCat.ExecuteNonQueryAsync();
            }

            var spTemplatesSql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageBannerTemplates_CYH
                    @Flag NVARCHAR(50),
                    @Id INT = NULL,
                    @CategoryId INT = NULL,
                    @Title NVARCHAR(200) = NULL,
                    @StyleTheme NVARCHAR(100) = NULL,
                    @AspectRatio NVARCHAR(50) = NULL,
                    @BackgroundImageUrl NVARCHAR(500) = NULL,
                    @ThumbnailUrl NVARCHAR(500) = NULL,
                    @PrimaryColor NVARCHAR(50) = NULL,
                    @SecondaryColor NVARCHAR(50) = NULL,
                    @TextColor NVARCHAR(50) = NULL,
                    @DefaultHeading NVARCHAR(500) = NULL,
                    @DefaultSubtitle NVARCHAR(MAX) = NULL,
                    @BadgeText NVARCHAR(100) = NULL,
                    @ConfigJson NVARCHAR(MAX) = NULL,
                    @DisplayOrder INT = NULL,
                    @IsActive BIT = NULL,
                    @Search NVARCHAR(200) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF @Flag = 'GET_ALL'
                    BEGIN
                        SELECT 
                            t.Id,
                            t.CategoryId,
                            c.CategoryName,
                            c.IconClass AS CategoryIcon,
                            t.Title,
                            t.StyleTheme,
                            t.AspectRatio,
                            t.BackgroundImageUrl,
                            t.ThumbnailUrl,
                            t.PrimaryColor,
                            t.SecondaryColor,
                            t.TextColor,
                            t.DefaultHeading,
                            t.DefaultSubtitle,
                            t.BadgeText,
                            t.ConfigJson,
                            t.DisplayOrder,
                            t.IsActive,
                            t.CreatedAt
                        FROM [dbo].[BannerTemplates_CYH] t
                        INNER JOIN [dbo].[BannerCategories_CYH] c ON t.CategoryId = c.Id
                        WHERE (t.IsActive = 1)
                          AND (@CategoryId IS NULL OR @CategoryId = 0 OR t.CategoryId = @CategoryId)
                          AND (@StyleTheme IS NULL OR @StyleTheme = 'All' OR t.StyleTheme = @StyleTheme)
                          AND (
                                @Search IS NULL 
                                OR t.Title LIKE '%' + @Search + '%' 
                                OR t.DefaultHeading LIKE '%' + @Search + '%'
                                OR t.DefaultSubtitle LIKE '%' + @Search + '%'
                                OR c.CategoryName LIKE '%' + @Search + '%'
                              )
                        ORDER BY t.DisplayOrder ASC, t.Id ASC;
                    END

                    ELSE IF @Flag = 'GET_BY_ID'
                    BEGIN
                        SELECT 
                            t.Id,
                            t.CategoryId,
                            c.CategoryName,
                            c.IconClass AS CategoryIcon,
                            t.Title,
                            t.StyleTheme,
                            t.AspectRatio,
                            t.BackgroundImageUrl,
                            t.ThumbnailUrl,
                            t.PrimaryColor,
                            t.SecondaryColor,
                            t.TextColor,
                            t.DefaultHeading,
                            t.DefaultSubtitle,
                            t.BadgeText,
                            t.ConfigJson,
                            t.DisplayOrder,
                            t.IsActive,
                            t.CreatedAt
                        FROM [dbo].[BannerTemplates_CYH] t
                        INNER JOIN [dbo].[BannerCategories_CYH] c ON t.CategoryId = c.Id
                        WHERE t.Id = @Id;
                    END

                    ELSE IF @Flag = 'SAVE_TEMPLATE'
                    BEGIN
                        IF @Id IS NOT NULL AND @Id > 0 AND EXISTS (SELECT 1 FROM [dbo].[BannerTemplates_CYH] WHERE Id = @Id)
                        BEGIN
                            UPDATE [dbo].[BannerTemplates_CYH]
                            SET CategoryId = ISNULL(@CategoryId, CategoryId),
                                Title = ISNULL(@Title, Title),
                                StyleTheme = ISNULL(@StyleTheme, StyleTheme),
                                AspectRatio = ISNULL(@AspectRatio, AspectRatio),
                                BackgroundImageUrl = ISNULL(@BackgroundImageUrl, BackgroundImageUrl),
                                ThumbnailUrl = ISNULL(@ThumbnailUrl, ThumbnailUrl),
                                PrimaryColor = ISNULL(@PrimaryColor, PrimaryColor),
                                SecondaryColor = ISNULL(@SecondaryColor, SecondaryColor),
                                TextColor = ISNULL(@TextColor, TextColor),
                                DefaultHeading = ISNULL(@DefaultHeading, DefaultHeading),
                                DefaultSubtitle = ISNULL(@DefaultSubtitle, DefaultSubtitle),
                                BadgeText = ISNULL(@BadgeText, BadgeText),
                                ConfigJson = ISNULL(@ConfigJson, ConfigJson),
                                DisplayOrder = ISNULL(@DisplayOrder, DisplayOrder),
                                IsActive = ISNULL(@IsActive, IsActive)
                            WHERE Id = @Id;

                            SELECT @Id AS ResultId;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO [dbo].[BannerTemplates_CYH]
                            (
                                CategoryId, Title, StyleTheme, AspectRatio, BackgroundImageUrl,
                                ThumbnailUrl, PrimaryColor, SecondaryColor, TextColor, DefaultHeading,
                                DefaultSubtitle, BadgeText, ConfigJson, DisplayOrder, IsActive, CreatedAt
                            )
                            VALUES
                            (
                                @CategoryId, @Title, ISNULL(@StyleTheme, 'Modern Clean'), ISNULL(@AspectRatio, '1:1'),
                                @BackgroundImageUrl, @ThumbnailUrl, ISNULL(@PrimaryColor, '#00f2fe'),
                                ISNULL(@SecondaryColor, '#0a192f'), ISNULL(@TextColor, '#ffffff'),
                                @DefaultHeading, @DefaultSubtitle, @BadgeText, @ConfigJson,
                                ISNULL(@DisplayOrder, 0), ISNULL(@IsActive, 1), SYSUTCDATETIME()
                            );

                            SELECT SCOPE_IDENTITY() AS ResultId;
                        END
                    END

                    ELSE IF @Flag = 'DELETE'
                    BEGIN
                        DELETE FROM [dbo].[BannerTemplates_CYH] WHERE Id = @Id;
                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                END";

            await using (var cmdTpl = new SqlCommand(spTemplatesSql, con))
            {
                await cmdTpl.ExecuteNonQueryAsync();
            }

            // 8. Seed Banner Categories and 50 Templates if not already present
            
            // 7.1. Ensure ManageCredits_CYH Stored Procedure exists & updated
            var spCreditsSql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageCredits_CYH
                    @Flag NVARCHAR(50),
                    @DoctorId INT = NULL,
                    @CreditTypeId INT = NULL,
                    @Code NVARCHAR(100) = NULL,
                    @Name NVARCHAR(100) = NULL,
                    @ServiceType NVARCHAR(50) = NULL,
                    @IsPaid BIT = NULL,
                    @IsActive BIT = NULL,
                    @Description NVARCHAR(500) = NULL,
                    @CreditCost INT = NULL,
                    @Amount INT = NULL,
                    @ActionBy NVARCHAR(200) = NULL,
                    @ReferenceId NVARCHAR(100) = NULL,
                    @Note NVARCHAR(500) = NULL,
                    @TransactionType NVARCHAR(50) = NULL,
                    @FromDate DATETIME2(7) = NULL,
                    @ToDate DATETIME2(7) = NULL,
                    @OnlyActive BIT = 0,
                    @OutSuccess BIT = NULL OUTPUT,
                    @OutMessage NVARCHAR(500) = NULL OUTPUT,
                    @OutBalanceAfter INT = NULL OUTPUT
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- 1. Initialize Doctor Credits (create 00 entries for any active CreditType not yet in DoctorCredits_CYH)
                    IF @Flag = 'INIT_DOCTOR_CREDITS'
                    BEGIN
                        INSERT INTO [dbo].[DoctorCredits_CYH] (DoctorId, CreditTypeId, TotalCredits, UsedCredits, IsActive, LastUpdatedAt)
                        SELECT @DoctorId, ct.Id, 0, 0, 1, SYSUTCDATETIME()
                        FROM [dbo].[CreditTypes_CYH] ct
                        WHERE ct.IsActive = 1
                          AND NOT EXISTS (
                              SELECT 1 FROM [dbo].[DoctorCredits_CYH] dc 
                              WHERE dc.DoctorId = @DoctorId AND dc.CreditTypeId = ct.Id
                          );

                        -- Insert SYSTEM_INIT log if no transaction exists for this doctor
                        IF NOT EXISTS (SELECT 1 FROM [dbo].[CreditTransactions_CYH] WHERE DoctorId = @DoctorId)
                        BEGIN
                            INSERT INTO [dbo].[CreditTransactions_CYH]
                            (DoctorId, CreditTypeId, TransactionType, CreditsChanged, BalanceBefore, BalanceAfter, ActionBy, Note, CreatedAt)
                            SELECT @DoctorId, ct.Id, 'SYSTEM_INIT', 0, 0, 0, 'System', 'Doctor account credit initialization at 0', SYSUTCDATETIME()
                            FROM [dbo].[CreditTypes_CYH] ct
                            WHERE ct.IsActive = 1;
                        END

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 2. Get All Credit Types
                    ELSE IF @Flag = 'GET_TYPES'
                    BEGIN
                        SELECT Id, Name, Code, ServiceType, IsPaid, IsActive, Description, CreditCost, CreatedAt
                        FROM [dbo].[CreditTypes_CYH]
                        WHERE (@OnlyActive = 0 OR IsActive = 1)
                        ORDER BY Id ASC;
                    END

                    -- 3. Get Credit Type by ID
                    ELSE IF @Flag = 'GET_TYPE_BY_ID'
                    BEGIN
                        SELECT Id, Name, Code, ServiceType, IsPaid, IsActive, Description, CreditCost, CreatedAt
                        FROM [dbo].[CreditTypes_CYH]
                        WHERE Id = @CreditTypeId;
                    END

                    -- 4. Get Credit Type by Code
                    ELSE IF @Flag = 'GET_TYPE_BY_CODE'
                    BEGIN
                        SELECT Id, Name, Code, ServiceType, IsPaid, IsActive, Description, CreditCost, CreatedAt
                        FROM [dbo].[CreditTypes_CYH]
                        WHERE Code = @Code;
                    END

                    -- 5. Save/Update Credit Type
                    ELSE IF @Flag = 'SAVE_TYPE'
                    BEGIN
                        IF @CreditTypeId IS NOT NULL AND @CreditTypeId > 0 AND EXISTS (SELECT 1 FROM [dbo].[CreditTypes_CYH] WHERE Id = @CreditTypeId)
                        BEGIN
                            UPDATE [dbo].[CreditTypes_CYH]
                            SET Name = ISNULL(@Name, Name),
                                Code = ISNULL(@Code, Code),
                                ServiceType = ISNULL(@ServiceType, ServiceType),
                                IsPaid = ISNULL(@IsPaid, IsPaid),
                                IsActive = ISNULL(@IsActive, IsActive),
                                Description = ISNULL(@Description, Description),
                                CreditCost = ISNULL(@CreditCost, CreditCost)
                            WHERE Id = @CreditTypeId;

                            SELECT @CreditTypeId AS ResultId;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO [dbo].[CreditTypes_CYH]
                            (Name, Code, ServiceType, IsPaid, IsActive, Description, CreditCost, CreatedAt)
                            VALUES
                            (@Name, @Code, ISNULL(@ServiceType, 'Free'), ISNULL(@IsPaid, 0), ISNULL(@IsActive, 1), @Description, ISNULL(@CreditCost, 1), SYSUTCDATETIME());

                            DECLARE @NewTypeId INT = SCOPE_IDENTITY();

                            -- Auto add this new credit type to all existing doctors with 0
                            INSERT INTO [dbo].[DoctorCredits_CYH] (DoctorId, CreditTypeId, TotalCredits, UsedCredits, IsActive, LastUpdatedAt)
                            SELECT d.Id, @NewTypeId, 0, 0, 1, SYSUTCDATETIME()
                            FROM [dbo].[DoctorProfiles] d
                            WHERE NOT EXISTS (
                                SELECT 1 FROM [dbo].[DoctorCredits_CYH] dc WHERE dc.DoctorId = d.Id AND dc.CreditTypeId = @NewTypeId
                            );

                            SELECT @NewTypeId AS ResultId;
                        END
                    END

                    -- 6. Toggle Type Active/Inactive
                    ELSE IF @Flag = 'TOGGLE_TYPE_STATUS'
                    BEGIN
                        UPDATE [dbo].[CreditTypes_CYH]
                        SET IsActive = @IsActive
                        WHERE Id = @CreditTypeId;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 7. Toggle Type Paid/Free
                    ELSE IF @Flag = 'TOGGLE_TYPE_PAID'
                    BEGIN
                        UPDATE [dbo].[CreditTypes_CYH]
                        SET IsPaid = @IsPaid,
                            ServiceType = ISNULL(@ServiceType, CASE WHEN @IsPaid = 1 THEN 'Paid' ELSE 'Free' END)
                        WHERE Id = @CreditTypeId;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 8. Get Doctor Credits
                    ELSE IF @Flag = 'GET_DOCTOR_CREDITS'
                    BEGIN
                        SELECT 
                            dc.Id, 
                            dc.DoctorId, 
                            d.FullName AS DoctorName, 
                            dc.CreditTypeId, 
                            ct.Name AS CreditTypeName, 
                            ct.Code AS CreditTypeCode, 
                            ct.ServiceType, 
                            ct.IsPaid, 
                            dc.TotalCredits, 
                            dc.UsedCredits, 
                            (dc.TotalCredits - dc.UsedCredits) AS RemainingCredits, 
                            dc.IsActive, 
                            dc.LastUpdatedAt
                        FROM [dbo].[DoctorCredits_CYH] dc
                        INNER JOIN [dbo].[CreditTypes_CYH] ct ON dc.CreditTypeId = ct.Id
                        INNER JOIN [dbo].[DoctorProfiles] d ON dc.DoctorId = d.Id
                        WHERE dc.DoctorId = @DoctorId
                        ORDER BY ct.Id ASC;
                    END

                    -- 9. Admin Adjust Credits (Add or Deduct)
                    ELSE IF @Flag = 'ADMIN_ADJUST'
                    BEGIN
                        DECLARE @CurrentTotal INT = 0;
                        DECLARE @CurrentUsed INT = 0;
                        DECLARE @CurrentBalance INT = 0;
                        DECLARE @NewTotal INT = 0;
                        DECLARE @NewBalance INT = 0;
                        DECLARE @AdjustTxType NVARCHAR(50) = CASE WHEN @Amount >= 0 THEN 'ADMIN_ADD' ELSE 'ADMIN_DEDUCT' END;

                        -- Ensure row exists
                        IF NOT EXISTS (SELECT 1 FROM [dbo].[DoctorCredits_CYH] WHERE DoctorId = @DoctorId AND CreditTypeId = @CreditTypeId)
                        BEGIN
                            INSERT INTO [dbo].[DoctorCredits_CYH] (DoctorId, CreditTypeId, TotalCredits, UsedCredits, IsActive, LastUpdatedAt)
                            VALUES (@DoctorId, @CreditTypeId, 0, 0, 1, SYSUTCDATETIME());
                        END

                        SELECT @CurrentTotal = TotalCredits, @CurrentUsed = UsedCredits
                        FROM [dbo].[DoctorCredits_CYH]
                        WHERE DoctorId = @DoctorId AND CreditTypeId = @CreditTypeId;

                        SET @CurrentBalance = @CurrentTotal - @CurrentUsed;
                        SET @NewTotal = @CurrentTotal + @Amount;
                        IF @NewTotal < 0 SET @NewTotal = 0;
                        SET @NewBalance = @NewTotal - @CurrentUsed;

                        UPDATE [dbo].[DoctorCredits_CYH]
                        SET TotalCredits = @NewTotal,
                            LastUpdatedAt = SYSUTCDATETIME()
                        WHERE DoctorId = @DoctorId AND CreditTypeId = @CreditTypeId;

                        INSERT INTO [dbo].[CreditTransactions_CYH]
                        (DoctorId, CreditTypeId, TransactionType, CreditsChanged, BalanceBefore, BalanceAfter, ActionBy, Note, CreatedAt)
                        VALUES
                        (@DoctorId, @CreditTypeId, @AdjustTxType, @Amount, @CurrentBalance, @NewBalance, ISNULL(@ActionBy, 'Admin'), @Note, SYSUTCDATETIME());

                        SET @OutSuccess = 1;
                        SET @OutMessage = 'Credits updated successfully.';
                        SET @OutBalanceAfter = @NewBalance;
                    END

                    -- 10. Deduct on Publish (Template or AI Banner)
                    ELSE IF @Flag = 'DEDUCT_PUBLISH'
                    BEGIN
                        DECLARE @FoundTypeId INT = NULL;
                        DECLARE @CreditCostToDeduct INT = 1;
                        DECLARE @TypeActive BIT = 1;

                        SELECT @FoundTypeId = Id, @CreditCostToDeduct = CreditCost, @TypeActive = IsActive
                        FROM [dbo].[CreditTypes_CYH]
                        WHERE Code = @Code;

                        IF @FoundTypeId IS NULL
                        BEGIN
                            SET @OutSuccess = 0;
                            SET @OutMessage = 'Invalid credit service code.';
                            SET @OutBalanceAfter = 0;
                            RETURN;
                        END

                        IF @TypeActive = 0
                        BEGIN
                            SET @OutSuccess = 0;
                            SET @OutMessage = 'This credit service is currently inactive.';
                            SET @OutBalanceAfter = 0;
                            RETURN;
                        END

                        -- Ensure row exists for doctor
                        IF NOT EXISTS (SELECT 1 FROM [dbo].[DoctorCredits_CYH] WHERE DoctorId = @DoctorId AND CreditTypeId = @FoundTypeId)
                        BEGIN
                            INSERT INTO [dbo].[DoctorCredits_CYH] (DoctorId, CreditTypeId, TotalCredits, UsedCredits, IsActive, LastUpdatedAt)
                            VALUES (@DoctorId, @FoundTypeId, 0, 0, 1, SYSUTCDATETIME());
                        END

                        DECLARE @DocTotal INT = 0;
                        DECLARE @DocUsed INT = 0;
                        DECLARE @DocBalanceBefore INT = 0;
                        DECLARE @DocBalanceAfter INT = 0;
                        DECLARE @DocCreditActive BIT = 1;

                        SELECT @DocTotal = TotalCredits, @DocUsed = UsedCredits, @DocCreditActive = IsActive
                        FROM [dbo].[DoctorCredits_CYH]
                        WHERE DoctorId = @DoctorId AND CreditTypeId = @FoundTypeId;

                        IF @DocCreditActive = 0
                        BEGIN
                            SET @OutSuccess = 0;
                            SET @OutMessage = 'Your credit account for this service is disabled. Contact Admin.';
                            SET @OutBalanceAfter = @DocTotal - @DocUsed;
                            RETURN;
                        END

                        SET @DocBalanceBefore = @DocTotal - @DocUsed;

                        IF @DocBalanceBefore < @CreditCostToDeduct
                        BEGIN
                            SET @OutSuccess = 0;
                            SET @OutMessage = 'Insufficient credits (' + CAST(@DocBalanceBefore AS NVARCHAR(10)) + ' remaining). Please contact Admin to add credits.';
                            SET @OutBalanceAfter = @DocBalanceBefore;
                            RETURN;
                        END

                        -- Deduct credit
                        SET @DocUsed = @DocUsed + @CreditCostToDeduct;
                        SET @DocBalanceAfter = @DocTotal - @DocUsed;

                        UPDATE [dbo].[DoctorCredits_CYH]
                        SET UsedCredits = @DocUsed,
                            LastUpdatedAt = SYSUTCDATETIME()
                        WHERE DoctorId = @DoctorId AND CreditTypeId = @FoundTypeId;

                        INSERT INTO [dbo].[CreditTransactions_CYH]
                        (DoctorId, CreditTypeId, TransactionType, CreditsChanged, BalanceBefore, BalanceAfter, ActionBy, ReferenceId, Note, CreatedAt)
                        VALUES
                        (@DoctorId, @FoundTypeId, 'DEBIT_ON_PUBLISH', -@CreditCostToDeduct, @DocBalanceBefore, @DocBalanceAfter, ISNULL(@ActionBy, 'Doctor'), @ReferenceId, @Note, SYSUTCDATETIME());

                        SET @OutSuccess = 1;
                        SET @OutMessage = 'Credit deducted successfully.';
                        SET @OutBalanceAfter = @DocBalanceAfter;
                    END

                    -- 11. Get Transactions / History
                    ELSE IF @Flag = 'GET_TRANSACTIONS'
                    BEGIN
                        SELECT 
                            t.Id,
                            t.DoctorId,
                            d.FullName AS DoctorName,
                            t.CreditTypeId,
                            ct.Name AS CreditTypeName,
                            ct.Code AS CreditTypeCode,
                            ct.ServiceType,
                            t.TransactionType,
                            t.CreditsChanged,
                            t.BalanceBefore,
                            t.BalanceAfter,
                            t.ActionBy,
                            t.ReferenceId,
                            t.Note,
                            t.CreatedAt
                        FROM [dbo].[CreditTransactions_CYH] t
                        INNER JOIN [dbo].[CreditTypes_CYH] ct ON t.CreditTypeId = ct.Id
                        INNER JOIN [dbo].[DoctorProfiles] d ON t.DoctorId = d.Id
                        WHERE (@DoctorId IS NULL OR t.DoctorId = @DoctorId)
                          AND (@CreditTypeId IS NULL OR t.CreditTypeId = @CreditTypeId)
                          AND (@TransactionType IS NULL OR @TransactionType = 'ALL' OR t.TransactionType = @TransactionType)
                          AND (@FromDate IS NULL OR t.CreatedAt >= @FromDate)
                          AND (@ToDate IS NULL OR t.CreatedAt <= @ToDate)
                        ORDER BY t.CreatedAt DESC, t.Id DESC;
                    END
                END";

            await using (var cmdCredits = new SqlCommand(spCreditsSql, con))
            {
                await cmdCredits.ExecuteNonQueryAsync();
            }

            // 7.2. Ensure ManageDoctorSocialAccounts_CYH Stored Procedure exists & updated
            var spSocialAccountsSql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageDoctorSocialAccounts_CYH
                    @Flag NVARCHAR(50),
                    @Id INT = NULL,
                    @DoctorId INT = NULL,
                    @UserId NVARCHAR(450) = NULL,
                    @DoctorName NVARCHAR(200) = NULL,
                    @Platform NVARCHAR(50) = NULL,
                    @AccountName NVARCHAR(200) = NULL,
                    @PageOrAccountId NVARCHAR(200) = NULL,
                    @AccessToken NVARCHAR(MAX) = NULL,
                    @RefreshToken NVARCHAR(MAX) = NULL,
                    @TokenExpiresAt DATETIME2(7) = NULL,
                    @IsActive BIT = NULL,
                    @LastTestStatus NVARCHAR(50) = NULL,
                    @LastTestMessage NVARCHAR(MAX) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF @Flag = 'GET_BY_DOCTOR'
                    BEGIN
                        SELECT Id, DoctorId, UserId, DoctorName, Platform, AccountName, PageOrAccountId,
                               AccessToken, RefreshToken, TokenExpiresAt, IsActive, ConnectedAt,
                               LastTestedAt, LastTestStatus, LastTestMessage
                        FROM [dbo].[DoctorSocialAccounts_CYH]
                        WHERE (@DoctorId IS NOT NULL AND DoctorId = @DoctorId)
                           OR (@UserId IS NOT NULL AND UserId = @UserId)
                        ORDER BY ConnectedAt DESC, Id DESC;
                    END
                    ELSE IF @Flag = 'GET_BY_ID'
                    BEGIN
                        SELECT Id, DoctorId, UserId, DoctorName, Platform, AccountName, PageOrAccountId,
                               AccessToken, RefreshToken, TokenExpiresAt, IsActive, ConnectedAt,
                               LastTestedAt, LastTestStatus, LastTestMessage
                        FROM [dbo].[DoctorSocialAccounts_CYH]
                        WHERE Id = @Id;
                    END
                    ELSE IF @Flag = 'SAVE_ACCOUNT'
                    BEGIN
                        DECLARE @ExistingId INT = NULL;
                        IF @Id IS NOT NULL AND @Id > 0
                        BEGIN
                            SET @ExistingId = @Id;
                        END
                        ELSE
                        BEGIN
                            SELECT TOP 1 @ExistingId = Id
                            FROM [dbo].[DoctorSocialAccounts_CYH]
                            WHERE ((@DoctorId IS NOT NULL AND DoctorId = @DoctorId) OR (@UserId IS NOT NULL AND UserId = @UserId))
                              AND Platform = @Platform
                              AND PageOrAccountId = @PageOrAccountId;
                        END

                        IF @ExistingId IS NOT NULL
                        BEGIN
                            UPDATE [dbo].[DoctorSocialAccounts_CYH]
                            SET AccountName = ISNULL(@AccountName, AccountName),
                                AccessToken = ISNULL(@AccessToken, AccessToken),
                                RefreshToken = ISNULL(@RefreshToken, RefreshToken),
                                TokenExpiresAt = ISNULL(@TokenExpiresAt, TokenExpiresAt),
                                IsActive = ISNULL(@IsActive, 1),
                                LastTestedAt = SYSUTCDATETIME(),
                                LastTestStatus = 'Success',
                                LastTestMessage = 'Connected successfully'
                            WHERE Id = @ExistingId;

                            SELECT @ExistingId AS ResultId;
                        END
                        ELSE
                        BEGIN
                            INSERT INTO [dbo].[DoctorSocialAccounts_CYH]
                            (DoctorId, UserId, DoctorName, Platform, AccountName, PageOrAccountId, AccessToken, RefreshToken, TokenExpiresAt, IsActive, ConnectedAt, LastTestedAt, LastTestStatus, LastTestMessage)
                            VALUES
                            (@DoctorId, @UserId, @DoctorName, @Platform, @AccountName, @PageOrAccountId, @AccessToken, @RefreshToken, @TokenExpiresAt, ISNULL(@IsActive, 1), SYSUTCDATETIME(), SYSUTCDATETIME(), 'Success', 'Connected successfully');

                            SELECT SCOPE_IDENTITY() AS ResultId;
                        END
                    END
                    ELSE IF @Flag = 'UPDATE_TEST_STATUS'
                    BEGIN
                        UPDATE [dbo].[DoctorSocialAccounts_CYH]
                        SET LastTestedAt = SYSUTCDATETIME(),
                            LastTestStatus = @LastTestStatus,
                            LastTestMessage = @LastTestMessage
                        WHERE Id = @Id;
                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                    ELSE IF @Flag = 'DELETE'
                    BEGIN
                        DELETE FROM [dbo].[DoctorSocialAccounts_CYH] WHERE Id = @Id;
                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                    ELSE IF @Flag = 'TOGGLE_ACTIVE'
                    BEGIN
                        UPDATE [dbo].[DoctorSocialAccounts_CYH]
                        SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                        WHERE Id = @Id;
                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                END";

            await using (var cmdSocial = new SqlCommand(spSocialAccountsSql, con))
            {
                await cmdSocial.ExecuteNonQueryAsync();
            }

            // 7.3. Ensure ManageBannerSchedules_CYH Stored Procedure exists & updated
            var spBannerSchedulesSql = @"
                CREATE OR ALTER PROCEDURE dbo.ManageBannerSchedules_CYH
                    @Flag NVARCHAR(50),
                    @Id INT = NULL,
                    @BannerDesignId INT = NULL,
                    @DoctorId INT = NULL,
                    @UserId NVARCHAR(450) = NULL,
                    @DoctorName NVARCHAR(200) = NULL,
                    @HospitalName NVARCHAR(200) = NULL,
                    @BannerHeading NVARCHAR(500) = NULL,
                    @ImageUrl NVARCHAR(MAX) = NULL,
                    @Caption NVARCHAR(MAX) = NULL,
                    @Platforms NVARCHAR(200) = NULL,
                    @ScheduledAt DATETIME2(7) = NULL,
                    @Status NVARCHAR(50) = NULL,
                    @ErrorMessage NVARCHAR(MAX) = NULL,
                    @CreatedBy NVARCHAR(200) = NULL,
                    @ScheduleDetailId INT = NULL,
                    @Platform NVARCHAR(50) = NULL,
                    @TargetAccountName NVARCHAR(200) = NULL,
                    @TargetPageId NVARCHAR(200) = NULL,
                    @PlatformPostId NVARCHAR(200) = NULL,
                    @PostUrl NVARCHAR(MAX) = NULL,
                    @Search NVARCHAR(200) = NULL
                AS
                BEGIN
                    SET NOCOUNT ON;

                    -- 1. Create schedule entry with initial details
                    IF @Flag = 'CREATE'
                    BEGIN
                        INSERT INTO [dbo].[BannerSchedules_CYH]
                        (BannerDesignId, DoctorId, UserId, DoctorName, HospitalName, BannerHeading, ImageUrl, Caption, Platforms, ScheduledAt, Status, ExecutionAttempts, CreatedAt, CreatedBy)
                        VALUES
                        (@BannerDesignId, @DoctorId, @UserId, @DoctorName, @HospitalName, @BannerHeading, @ImageUrl, @Caption, @Platforms, @ScheduledAt, 'Pending', 0, SYSUTCDATETIME(), @CreatedBy);

                        DECLARE @NewScheduleId INT = SCOPE_IDENTITY();
                        SELECT @NewScheduleId AS ResultId;
                    END

                    -- 2. Add Detail for Platform
                    ELSE IF @Flag = 'ADD_DETAIL'
                    BEGIN
                        INSERT INTO [dbo].[BannerScheduleDetails_CYH]
                        (BannerScheduleId, Platform, TargetAccountName, TargetPageId, Status)
                        VALUES
                        (@Id, @Platform, @TargetAccountName, @TargetPageId, 'Pending');

                        SELECT SCOPE_IDENTITY() AS ResultId;
                    END

                    -- 3. Get Due Schedules for Background Worker (Locking with 'Processing')
                    ELSE IF @Flag = 'GET_DUE_SCHEDULES'
                    BEGIN
                        DECLARE @DueList TABLE (Id INT);

                        UPDATE TOP (10) [dbo].[BannerSchedules_CYH]
                        SET Status = 'Processing',
                            LastExecutedAt = SYSUTCDATETIME(),
                            ExecutionAttempts = ExecutionAttempts + 1
                        OUTPUT inserted.Id INTO @DueList
                        WHERE Status = 'Pending' 
                          AND ScheduledAt <= SYSUTCDATETIME();

                        -- Return the main schedule records
                        SELECT s.Id, s.BannerDesignId, s.DoctorId, s.UserId, s.DoctorName, s.HospitalName,
                               s.BannerHeading, s.ImageUrl, s.Caption, s.Platforms, s.ScheduledAt,
                               s.Status, s.ExecutionAttempts, s.LastExecutedAt, s.ErrorMessage, s.CreatedAt, s.CreatedBy
                        FROM [dbo].[BannerSchedules_CYH] s
                        INNER JOIN @DueList d ON s.Id = d.Id;

                        -- Return details for those schedules
                        SELECT d.Id, d.BannerScheduleId, d.Platform, d.TargetAccountName, d.TargetPageId,
                               d.Status, d.PlatformPostId, d.PostUrl, d.ErrorMessage, d.PublishedAt
                        FROM [dbo].[BannerScheduleDetails_CYH] d
                        INNER JOIN @DueList dl ON d.BannerScheduleId = dl.Id;
                    END

                    -- 4. Update Detail Status (from publisher service)
                    ELSE IF @Flag = 'UPDATE_DETAIL_STATUS'
                    BEGIN
                        UPDATE [dbo].[BannerScheduleDetails_CYH]
                        SET Status = @Status,
                            PlatformPostId = ISNULL(@PlatformPostId, PlatformPostId),
                            PostUrl = ISNULL(@PostUrl, PostUrl),
                            ErrorMessage = @ErrorMessage,
                            PublishedAt = CASE WHEN @Status = 'Success' THEN SYSUTCDATETIME() ELSE PublishedAt END
                        WHERE Id = @ScheduleDetailId;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 5. Mark Schedule Master Status
                    ELSE IF @Flag = 'UPDATE_SCHEDULE_STATUS'
                    BEGIN
                        UPDATE [dbo].[BannerSchedules_CYH]
                        SET Status = @Status,
                            ErrorMessage = @ErrorMessage,
                            LastExecutedAt = SYSUTCDATETIME()
                        WHERE Id = @Id;

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 6. Get All Schedules for Admin (with search & filters)
                    ELSE IF @Flag = 'GET_ALL_ADMIN'
                    BEGIN
                        SELECT s.Id, s.BannerDesignId, s.DoctorId, s.UserId, s.DoctorName, s.HospitalName,
                               s.BannerHeading, s.ImageUrl, s.Caption, s.Platforms, s.ScheduledAt,
                               s.Status, s.ExecutionAttempts, s.LastExecutedAt, s.ErrorMessage, s.CreatedAt, s.CreatedBy
                        FROM [dbo].[BannerSchedules_CYH] s
                        WHERE (@DoctorId IS NULL OR s.DoctorId = @DoctorId)
                          AND (@Status IS NULL OR @Status = 'All' OR s.Status = @Status)
                          AND (@Search IS NULL OR s.DoctorName LIKE '%' + @Search + '%' OR s.BannerHeading LIKE '%' + @Search + '%' OR s.Caption LIKE '%' + @Search + '%')
                        ORDER BY s.ScheduledAt DESC, s.Id DESC;

                        SELECT d.Id, d.BannerScheduleId, d.Platform, d.TargetAccountName, d.TargetPageId,
                               d.Status, d.PlatformPostId, d.PostUrl, d.ErrorMessage, d.PublishedAt
                        FROM [dbo].[BannerScheduleDetails_CYH] d
                        ORDER BY d.Id ASC;
                    END

                    -- 7. Get Schedules by Doctor
                    ELSE IF @Flag = 'GET_BY_DOCTOR'
                    BEGIN
                        SELECT s.Id, s.BannerDesignId, s.DoctorId, s.UserId, s.DoctorName, s.HospitalName,
                               s.BannerHeading, s.ImageUrl, s.Caption, s.Platforms, s.ScheduledAt,
                               s.Status, s.ExecutionAttempts, s.LastExecutedAt, s.ErrorMessage, s.CreatedAt, s.CreatedBy
                        FROM [dbo].[BannerSchedules_CYH] s
                        WHERE (@DoctorId IS NOT NULL AND s.DoctorId = @DoctorId)
                           OR (@UserId IS NOT NULL AND s.UserId = @UserId)
                        ORDER BY s.ScheduledAt DESC, s.Id DESC;

                        SELECT d.Id, d.BannerScheduleId, d.Platform, d.TargetAccountName, d.TargetPageId,
                               d.Status, d.PlatformPostId, d.PostUrl, d.ErrorMessage, d.PublishedAt
                        FROM [dbo].[BannerScheduleDetails_CYH] d
                        INNER JOIN [dbo].[BannerSchedules_CYH] s ON d.BannerScheduleId = s.Id
                        WHERE (@DoctorId IS NOT NULL AND s.DoctorId = @DoctorId)
                           OR (@UserId IS NOT NULL AND s.UserId = @UserId)
                        ORDER BY d.Id ASC;
                    END

                    -- 8. Cancel Schedule
                    ELSE IF @Flag = 'CANCEL_SCHEDULE'
                    BEGIN
                        UPDATE [dbo].[BannerSchedules_CYH]
                        SET Status = 'Cancelled',
                            ErrorMessage = 'Cancelled by user/admin'
                        WHERE Id = @Id AND Status = 'Pending';

                        UPDATE [dbo].[BannerScheduleDetails_CYH]
                        SET Status = 'Cancelled'
                        WHERE BannerScheduleId = @Id AND Status = 'Pending';

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END

                    -- 9. Retry Schedule
                    ELSE IF @Flag = 'RETRY_SCHEDULE'
                    BEGIN
                        UPDATE [dbo].[BannerSchedules_CYH]
                        SET Status = 'Pending',
                            ScheduledAt = SYSUTCDATETIME(),
                            ErrorMessage = NULL
                        WHERE Id = @Id;

                        UPDATE [dbo].[BannerScheduleDetails_CYH]
                        SET Status = 'Pending',
                            ErrorMessage = NULL
                        WHERE BannerScheduleId = @Id AND Status IN ('Failed', 'Cancelled');

                        SELECT @@ROWCOUNT AS RowsAffected;
                    END
                END";

            await using (var cmdSched = new SqlCommand(spBannerSchedulesSql, con))
            {
                await cmdSched.ExecuteNonQueryAsync();
            }

            // 8.1. Seed Credit System Types and Doctor Credits
            await SeedCreditSystemIfEmptyAsync(con, logger);

            await SeedBannerTemplatesIfEmptyAsync(con, logger);

            // 9. Initial seed via Stored Procedure
            try
            {
                await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SEED_INITIAL_DATA");
                await cmd.ExecuteNonQueryAsync();
                logger?.LogInformation("Database initialized successfully via ManageDoctor_CYH Stored Procedure.");
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "ManageDoctor_CYH initial seed was skipped or already present.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to initialize database tables.");
        }
    }

    private static async Task SeedBannerTemplatesIfEmptyAsync(SqlConnection con, ILogger? logger)
    {
        try
        {
            // 1. Check if categories already exist
            int categoryCount = 0;
            await using (var cmdCheck = new SqlCommand("SELECT COUNT(1) FROM [dbo].[BannerCategories_CYH]", con))
            {
                var val = await cmdCheck.ExecuteScalarAsync();
                categoryCount = Convert.ToInt32(val);
            }

            var categoryMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (categoryCount == 0)
            {
                logger?.LogInformation("Seeding initial 10 Dynamic Banner Categories into SQL...");
                var categories = TemplateSeedData.GetCategories();
                foreach (var cat in categories)
                {
                    await using var cmd = new SqlCommand("ManageBannerCategories_CYH", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "INSERT");
                    cmd.Parameters.AddWithValue("@CategoryName", cat.Name);
                    cmd.Parameters.AddWithValue("@Slug", cat.Slug);
                    cmd.Parameters.AddWithValue("@IconClass", cat.Icon);
                    cmd.Parameters.AddWithValue("@BadgeColor", cat.Color);
                    cmd.Parameters.AddWithValue("@DisplayOrder", cat.Order);
                    cmd.Parameters.AddWithValue("@IsActive", true);

                    var idObj = await cmd.ExecuteScalarAsync();
                    if (idObj != null && int.TryParse(idObj.ToString(), out int newId))
                    {
                        categoryMap[cat.Slug] = newId;
                    }
                }
            }
            else
            {
                // Load existing categories map
                await using var cmd = new SqlCommand("SELECT Id, Slug FROM [dbo].[BannerCategories_CYH]", con);
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    categoryMap[reader.GetString(1)] = reader.GetInt32(0);
                }
            }

            // 2. Check if templates already exist
            int templateCount = 0;
            await using (var cmdCheckTpl = new SqlCommand("SELECT COUNT(1) FROM [dbo].[BannerTemplates_CYH]", con))
            {
                var val = await cmdCheckTpl.ExecuteScalarAsync();
                templateCount = Convert.ToInt32(val);
            }

            if (templateCount == 0 && categoryMap.Count > 0)
            {
                logger?.LogInformation("Seeding 50 Healthcare Banner Templates into SQL...");
                var templates = TemplateSeedData.GetTemplates();
                var categories = TemplateSeedData.GetCategories().ToDictionary(c => c.Slug, c => c.Name);

                foreach (var tpl in templates)
                {
                    if (!categoryMap.TryGetValue(tpl.CategorySlug, out int catId))
                    {
                        catId = categoryMap.Values.FirstOrDefault();
                    }

                    string categoryName = categories.TryGetValue(tpl.CategorySlug, out var cn) ? cn : "Healthcare";
                    string configJson = TemplateSeedData.GenerateConfigJson(tpl, categoryName);

                    await using var cmd = new SqlCommand("ManageBannerTemplates_CYH", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Flag", "SAVE_TEMPLATE");
                    cmd.Parameters.AddWithValue("@CategoryId", catId);
                    cmd.Parameters.AddWithValue("@Title", tpl.Title);
                    cmd.Parameters.AddWithValue("@StyleTheme", tpl.StyleTheme);
                    cmd.Parameters.AddWithValue("@AspectRatio", tpl.AspectRatio);
                    cmd.Parameters.AddWithValue("@BackgroundImageUrl", tpl.ImageUrl);
                    cmd.Parameters.AddWithValue("@ThumbnailUrl", tpl.ImageUrl);
                    cmd.Parameters.AddWithValue("@PrimaryColor", tpl.PrimaryColor);
                    cmd.Parameters.AddWithValue("@SecondaryColor", tpl.SecondaryColor);
                    cmd.Parameters.AddWithValue("@TextColor", "#ffffff");
                    cmd.Parameters.AddWithValue("@DefaultHeading", tpl.Heading);
                    cmd.Parameters.AddWithValue("@DefaultSubtitle", (object?)tpl.Subtitle ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BadgeText", (object?)tpl.BadgeText ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ConfigJson", configJson);
                    cmd.Parameters.AddWithValue("@DisplayOrder", tpl.Order);
                    cmd.Parameters.AddWithValue("@IsActive", true);

                    await cmd.ExecuteScalarAsync();
                }
                logger?.LogInformation("50 Banner Templates successfully seeded into SQL Database.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error while seeding banner categories & templates in SQL.");
}
        }
    
    private static async Task SeedCreditSystemIfEmptyAsync(SqlConnection con, ILogger? logger)
    {
        try
        {
            // 1. Ensure Default Credit Types exist
            int typeCount = 0;
            await using (var cmdCheck = new SqlCommand("SELECT COUNT(1) FROM [dbo].[CreditTypes_CYH]", con))
            {
                var val = await cmdCheck.ExecuteScalarAsync();
                typeCount = Convert.ToInt32(val);
            }

            if (typeCount == 0)
            {
                logger?.LogInformation("Seeding default Credit Types into CreditTypes_CYH...");

                // Type 1: Template Photo (Free service, toggleable)
                await using (var cmd1 = new SqlCommand("ManageCredits_CYH", con))
                {
                    cmd1.CommandType = CommandType.StoredProcedure;
                    cmd1.Parameters.AddWithValue("@Flag", "SAVE_TYPE");
                    cmd1.Parameters.AddWithValue("@Name", "Template Photo Publish");
                    cmd1.Parameters.AddWithValue("@Code", "TEMPLATE_PUBLISH");
                    cmd1.Parameters.AddWithValue("@ServiceType", "Free");
                    cmd1.Parameters.AddWithValue("@IsPaid", false);
                    cmd1.Parameters.AddWithValue("@IsActive", true);
                    cmd1.Parameters.AddWithValue("@Description", "Standard healthcare template customization and publish");
                    cmd1.Parameters.AddWithValue("@CreditCost", 1);
                    await cmd1.ExecuteScalarAsync();
                }

                // Type 2: AI Banner (Paid service, toggleable)
                await using (var cmd2 = new SqlCommand("ManageCredits_CYH", con))
                {
                    cmd2.CommandType = CommandType.StoredProcedure;
                    cmd2.Parameters.AddWithValue("@Flag", "SAVE_TYPE");
                    cmd2.Parameters.AddWithValue("@Name", "AI Banner Creation & Publish");
                    cmd2.Parameters.AddWithValue("@Code", "AI_BANNER_PUBLISH");
                    cmd2.Parameters.AddWithValue("@ServiceType", "Paid");
                    cmd2.Parameters.AddWithValue("@IsPaid", true);
                    cmd2.Parameters.AddWithValue("@IsActive", true);
                    cmd2.Parameters.AddWithValue("@Description", "AI generated prompt healthcare banner customization and publish");
                    cmd2.Parameters.AddWithValue("@CreditCost", 1);
                    await cmd2.ExecuteScalarAsync();
                }

                logger?.LogInformation("Default Credit Types seeded successfully.");
            }

            // 2. Ensure all existing doctors have initialized 00 entries for all active credit types
            await using (var cmdDocs = new SqlCommand("SELECT Id FROM [dbo].[DoctorProfiles]", con))
            {
                var docIds = new List<int>();
                await using var reader = await cmdDocs.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    docIds.Add(reader.GetInt32(0));
                }
                await reader.CloseAsync();

                foreach (var docId in docIds)
                {
                    await using var cmdInit = new SqlCommand("ManageCredits_CYH", con);
                    cmdInit.CommandType = CommandType.StoredProcedure;
                    cmdInit.Parameters.AddWithValue("@Flag", "INIT_DOCTOR_CREDITS");
                    cmdInit.Parameters.AddWithValue("@DoctorId", docId);
                    await cmdInit.ExecuteNonQueryAsync();
                }
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error while seeding default Credit Types in SQL.");
        }
    }
}