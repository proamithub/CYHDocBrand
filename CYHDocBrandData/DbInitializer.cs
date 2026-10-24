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
                    ALTER TABLE [dbo].[DoctorProfiles] ADD [IsVerified] BIT NOT NULL DEFAULT (1);";

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

            // 7. Initial seed via Stored Procedure
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
}
