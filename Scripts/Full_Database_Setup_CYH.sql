-- ============================================================================
-- CYH DoctorHub — Complete Production Database Setup Script
-- Target: SQL Server (VPS Server: https://vps2.clickyourhospital.com)
-- Database: CYHDocBrandBD
-- ALL operations managed 100% via SQL Stored Procedures
-- ============================================================================

-- 1. Create Database (if not exists)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'CYHDocBrandBD')
BEGIN
    CREATE DATABASE [CYHDocBrandBD];
END
GO

USE [CYHDocBrandBD];
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- 2. Create AspNetUsers Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AspNetUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AspNetUsers] (
        [Id]                   NVARCHAR (450)     NOT NULL,
        [FullName]             NVARCHAR (MAX)     NOT NULL,
        [Role]                 NVARCHAR (MAX)     NOT NULL,
        [AvatarUrl]            NVARCHAR (MAX)     NULL,
        [Department]           NVARCHAR (MAX)     NULL,
        [CreatedAt]            DATETIME2 (7)      NOT NULL DEFAULT (SYSUTCDATETIME()),
        [IsActive]             BIT                NOT NULL DEFAULT (1),
        [UserName]             NVARCHAR (256)     NULL,
        [NormalizedUserName]   NVARCHAR (256)     NULL,
        [Email]                NVARCHAR (256)     NULL,
        [NormalizedEmail]      NVARCHAR (256)     NULL,
        [EmailConfirmed]       BIT                NOT NULL DEFAULT (1),
        [PasswordHash]         NVARCHAR (MAX)     NULL,
        [SecurityStamp]        NVARCHAR (MAX)     NULL,
        [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
        [PhoneNumber]          NVARCHAR (MAX)     NULL,
        [PhoneNumberConfirmed] BIT                NOT NULL DEFAULT (0),
        [TwoFactorEnabled]     BIT                NOT NULL DEFAULT (0),
        [LockoutEnd]           DATETIMEOFFSET (7) NULL,
        [LockoutEnabled]       BIT                NOT NULL DEFAULT (0),
        [AccessFailedCount]    INT                NOT NULL DEFAULT (0),
        [Password]             NVARCHAR (500)     NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 3. Create DoctorProfiles Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorProfiles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DoctorProfiles] (
        [Id]               INT             IDENTITY (1, 1) NOT NULL,
        [UserId]           NVARCHAR (450)  NOT NULL,
        [FullName]         NVARCHAR (100)  NOT NULL,
        [Specialization]   NVARCHAR (100)  NOT NULL,
        [LicenseNumber]    NVARCHAR (50)   NOT NULL,
        [ExperienceYears]  INT             NOT NULL DEFAULT (0),
        [ConsultationFee]  DECIMAL (18, 2) NOT NULL DEFAULT (0.00),
        [AvailableDays]    NVARCHAR (100)  NOT NULL DEFAULT ('Mon - Sat'),
        [AvailableHours]   NVARCHAR (50)   NOT NULL DEFAULT ('09:00 AM - 04:00 PM'),
        [Bio]              NVARCHAR (500)  NULL,
        [ProfilePic]       NVARCHAR (MAX)  NULL,
        [IsAvailableToday] BIT             NOT NULL DEFAULT (1),
        [CreatedAt]        DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_DoctorProfiles] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_DoctorProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

-- 4. Create BannerDesigns_CYH Table
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
GO

-- 5. Create DoctorAiSettings_CYH Table
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
GO

-- ============================================================================
-- 6. Stored Procedure: AuthenticateDoctor_CYH
-- Purpose: Authenticates user credentials (Admin or Doctor, Active or Inactive)
-- ============================================================================
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
END
GO

-- ============================================================================
-- 7. Stored Procedure: ManageDoctor_CYH
-- Purpose: Complete Doctor Profile & User Management CRUD via Stored Procedure
-- ============================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

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
    @ProfilePic NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Get all doctors with user accounts
    IF @Flag = 'GET_ALL'
    BEGIN
        SELECT 
            d.Id,
            d.UserId,
            d.FullName,
            d.Specialization,
            d.LicenseNumber,
            d.ExperienceYears,
            d.ConsultationFee,
            d.AvailableDays,
            d.AvailableHours,
            d.Bio,
            d.IsAvailableToday,
            d.CreatedAt,
            d.ProfilePic,
            u.Id AS AppUserId,
            u.UserName,
            u.Email,
            u.PhoneNumber,
            u.FullName AS UserFullName,
            u.Role,
            u.Department,
            u.IsActive,
            u.AvatarUrl,
            u.Password
        FROM DoctorProfiles d
        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
        ORDER BY d.FullName ASC;
    END

    -- 2. Get doctor by DoctorProfile Id
    ELSE IF @Flag = 'GET_BY_ID'
    BEGIN
        SELECT 
            d.Id,
            d.UserId,
            d.FullName,
            d.Specialization,
            d.LicenseNumber,
            d.ExperienceYears,
            d.ConsultationFee,
            d.AvailableDays,
            d.AvailableHours,
            d.Bio,
            d.IsAvailableToday,
            d.CreatedAt,
            d.ProfilePic,
            u.Id AS AppUserId,
            u.UserName,
            u.Email,
            u.PhoneNumber,
            u.FullName AS UserFullName,
            u.Role,
            u.Department,
            u.IsActive,
            u.AvatarUrl,
            u.Password
        FROM DoctorProfiles d
        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
        WHERE d.Id = @DoctorId;
    END

    -- 3. Get doctor by AspNetUsers UserId
    ELSE IF @Flag = 'GET_BY_USERID'
    BEGIN
        SELECT 
            d.Id,
            d.UserId,
            d.FullName,
            d.Specialization,
            d.LicenseNumber,
            d.ExperienceYears,
            d.ConsultationFee,
            d.AvailableDays,
            d.AvailableHours,
            d.Bio,
            d.IsAvailableToday,
            d.CreatedAt,
            d.ProfilePic,
            u.Id AS AppUserId,
            u.UserName,
            u.Email,
            u.PhoneNumber,
            u.FullName AS UserFullName,
            u.Role,
            u.Department,
            u.IsActive,
            u.AvatarUrl,
            u.Password
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
            ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt, ProfilePic
        )
        VALUES 
        (
            @UserId, @FullName, @Specialization, @LicenseNumber, @ExperienceYears,
            @ConsultationFee, @AvailableDays, @AvailableHours, @Bio, ISNULL(@IsAvailableToday, 1), SYSUTCDATETIME(), @ProfilePic
        );

        SELECT SCOPE_IDENTITY() AS NewId;
    END

    -- 8. Register new Doctor (Atomic transaction with inactive state by default)
    ELSE IF @Flag = 'REGISTER_DOCTOR'
    BEGIN
        -- Check if user already exists
        IF EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = @Email OR UserName = @Email)
        BEGIN
            SELECT 0 AS StatusCode, 'An account with this email address already exists. Please sign in or use another email.' AS Message, NULL AS UserId;
            RETURN;
        END

        DECLARE @NewUserId NVARCHAR(450) = ISNULL(@UserId, CAST(NEWID() AS NVARCHAR(450)));
        DECLARE @Now DATETIME2(7) = SYSUTCDATETIME();

        BEGIN TRANSACTION;
        BEGIN TRY
            -- Insert into AspNetUsers (Inactive: IsActive = 0)
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

            -- Insert into DoctorProfiles (Off duty: IsAvailableToday = 0)
            INSERT INTO DoctorProfiles 
            (
                UserId, FullName, Specialization, LicenseNumber, ExperienceYears, 
                ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt
            )
            VALUES 
            (
                @NewUserId, @FullName, @Specialization, ISNULL(@LicenseNumber, ''), ISNULL(@ExperienceYears, 0), 
                ISNULL(@ConsultationFee, 0.00), 
                ISNULL(NULLIF(@AvailableDays, ''), 'Mon - Sat'), 
                ISNULL(NULLIF(@AvailableHours, ''), '09:00 AM - 05:00 PM'), 
                @Bio, 0, @Now
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
        UPDATE DoctorProfiles 
        SET FullName = ISNULL(@FullName, FullName), 
            Specialization = ISNULL(@Specialization, Specialization), 
            LicenseNumber = ISNULL(@LicenseNumber, LicenseNumber), 
            ExperienceYears = ISNULL(@ExperienceYears, ExperienceYears), 
            ConsultationFee = ISNULL(@ConsultationFee, ConsultationFee), 
            AvailableDays = ISNULL(@AvailableDays, AvailableDays), 
            AvailableHours = ISNULL(@AvailableHours, AvailableHours), 
            Bio = @Bio,
            ProfilePic = COALESCE(@ProfilePic, ProfilePic)
        WHERE Id = @DoctorId;
        
        IF @ProfilePic IS NOT NULL AND @ProfilePic <> ''
        BEGIN
            UPDATE AspNetUsers
            SET AvatarUrl = @ProfilePic
            WHERE Id = (SELECT UserId FROM DoctorProfiles WHERE Id = @DoctorId);
        END

        SELECT @@ROWCOUNT AS RowsAffected;
    END

    -- 10. Initial Seed Data
    ELSE IF @Flag = 'SEED_INITIAL_DATA'
    BEGIN
        -- Admin User
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

        -- Doctor 1
        IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'doctor@cyhdoctorhub.com')
        BEGIN
            DECLARE @Doc1UserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, FullName, Role, Department, Password, CreatedAt, IsActive, AccessFailedCount, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled)
            VALUES (
                @Doc1UserId, 'doctor@cyhdoctorhub.com', 'DOCTOR@CYHDOCTORHUB.COM', 'doctor@cyhdoctorhub.com', 'DOCTOR@CYHDOCTORHUB.COM', 1,
                'Dr. A. Sharma', 'Doctor', 'Cardiology',
                'n7uoG53mus5DzCXmNgv7NQ==', SYSUTCDATETIME(), 1, 0, 0, 0, 0
            );

            IF NOT EXISTS (SELECT 1 FROM DoctorProfiles WHERE FullName = 'Dr. A. Sharma')
            BEGIN
                INSERT INTO DoctorProfiles (UserId, FullName, Specialization, LicenseNumber, ExperienceYears, ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt)
                VALUES (
                    @Doc1UserId, 'Dr. A. Sharma', 'Chief Cardiologist', 'MED-DEL-98421', 14, 85.00,
                    'Mon - Sat', '09:00 AM - 04:00 PM',
                    'Specializing in interventional cardiology, heart failure management, and preventive cardiovascular wellness.',
                    1, SYSUTCDATETIME()
                );
            END
        END
        ELSE
        BEGIN
            UPDATE AspNetUsers SET Password = 'n7uoG53mus5DzCXmNgv7NQ==' WHERE Email = 'doctor@cyhdoctorhub.com';
        END

        -- Doctor 2
        IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Email = 'priya.patel@cyhdoctorhub.com')
        BEGIN
            DECLARE @Doc2UserId NVARCHAR(450) = CAST(NEWID() AS NVARCHAR(450));
            INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, FullName, Role, Department, Password, CreatedAt, IsActive, AccessFailedCount, LockoutEnabled, PhoneNumberConfirmed, TwoFactorEnabled)
            VALUES (
                @Doc2UserId, 'priya.patel@cyhdoctorhub.com', 'PRIYA.PATEL@CYHDOCTORHUB.COM', 'priya.patel@cyhdoctorhub.com', 'PRIYA.PATEL@CYHDOCTORHUB.COM', 1,
                'Dr. Priya Patel', 'Doctor', 'Neurology',
                'n7uoG53mus5DzCXmNgv7NQ==', SYSUTCDATETIME(), 1, 0, 0, 0, 0
            );

            IF NOT EXISTS (SELECT 1 FROM DoctorProfiles WHERE FullName = 'Dr. Priya Patel')
            BEGIN
                INSERT INTO DoctorProfiles (UserId, FullName, Specialization, LicenseNumber, ExperienceYears, ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt)
                VALUES (
                    @Doc2UserId, 'Dr. Priya Patel', 'Senior Neurologist', 'MED-MUM-77319', 10, 90.00,
                    'Mon, Wed, Fri', '10:00 AM - 05:00 PM',
                    'Expert in neuro-vascular disorders, neuro-rehabilitation, stroke management, and migraines.',
                    1, SYSUTCDATETIME()
                );
            END
        END
        ELSE
        BEGIN
            UPDATE AspNetUsers SET Password = 'n7uoG53mus5DzCXmNgv7NQ==' WHERE Email = 'priya.patel@cyhdoctorhub.com';
        END

        SELECT 1 AS Status;
    END
END
GO

-- ============================================================================
-- 8. Stored Procedure: GetDashboardStats_CYH
-- Purpose: Admin and Doctor Dashboard KPI statistics
-- ============================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE dbo.GetDashboardStats_CYH
    @Flag NVARCHAR(50),
    @DoctorId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @Flag = 'ADMIN_STATS'
    BEGIN
        SELECT 
            (SELECT COUNT(*) FROM DoctorProfiles) AS TotalDoctors,
            (SELECT COUNT(*) FROM DoctorProfiles WHERE IsAvailableToday = 1) AS ActiveDoctorsOnDuty,
            (SELECT COUNT(DISTINCT Specialization) FROM DoctorProfiles) AS TotalDepartments,
            (SELECT COUNT(*) FROM AspNetUsers WHERE IsActive = 1) AS TotalUsers;
    END
    ELSE IF @Flag = 'DOCTOR_STATS'
    BEGIN
        SELECT 
            d.Id AS DoctorId,
            d.FullName AS DoctorName,
            d.Specialization AS Specialization,
            d.LicenseNumber AS LicenseNumber,
            d.ExperienceYears AS ExperienceYears,
            d.ConsultationFee AS ConsultationFee,
            d.AvailableHours AS AvailableHours,
            d.AvailableDays AS AvailableDays,
            d.Bio AS Bio,
            d.IsAvailableToday AS IsAvailableToday
        FROM DoctorProfiles d
        WHERE d.Id = @DoctorId;
    END
END
GO

-- ============================================================================
-- 9. Stored Procedure: ManageBannerDesigns_CYH
-- Purpose: Complete Banner Studio Designs CRUD via Stored Procedure
-- ============================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

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

    -- 1. Get all banners with user/status/search filtering
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
                OR TemplateType LIKE '%' + @Search + '%'
              )
        ORDER BY UpdatedAt DESC;
    END

    -- 2. Get banner by ID
    ELSE IF @Flag = 'GET_BY_ID'
    BEGIN
        SELECT 
            Id, UserId, DoctorName, HospitalName, TemplateType, Heading, Subtitle, 
            Status, AspectRatio, PrimaryColor, BackgroundImageUrl, ConfigJson, 
            CreatedAt, UpdatedAt, PublishedAt
        FROM [dbo].[BannerDesigns_CYH]
        WHERE Id = @Id;
    END

    -- 3. Save draft (Insert or Update)
    ELSE IF @Flag = 'SAVE_DRAFT'
    BEGIN
        IF @Id IS NOT NULL AND @Id > 0
        BEGIN
            UPDATE [dbo].[BannerDesigns_CYH]
            SET DoctorName = ISNULL(@DoctorName, DoctorName),
                HospitalName = ISNULL(@HospitalName, HospitalName),
                TemplateType = ISNULL(@TemplateType, TemplateType),
                Heading = ISNULL(@Heading, Heading),
                Subtitle = @Subtitle,
                Status = ISNULL(@Status, Status),
                AspectRatio = ISNULL(@AspectRatio, AspectRatio),
                PrimaryColor = ISNULL(@PrimaryColor, PrimaryColor),
                BackgroundImageUrl = @BackgroundImageUrl,
                ConfigJson = ISNULL(@ConfigJson, ConfigJson),
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;

            SELECT @Id AS SavedId;
        END
        ELSE
        BEGIN
            INSERT INTO [dbo].[BannerDesigns_CYH]
            (
                UserId, DoctorName, HospitalName, TemplateType, Heading, Subtitle, 
                Status, AspectRatio, PrimaryColor, BackgroundImageUrl, ConfigJson, 
                CreatedAt, UpdatedAt
            )
            VALUES
            (
                @UserId, 
                ISNULL(@DoctorName, ''), 
                ISNULL(@HospitalName, ''), 
                ISNULL(@TemplateType, 'Doctor'), 
                ISNULL(@Heading, 'Specialist Consultation'), 
                @Subtitle, 
                ISNULL(@Status, 'Draft'), 
                ISNULL(@AspectRatio, '1:1'), 
                ISNULL(@PrimaryColor, '#00f2fe'), 
                @BackgroundImageUrl, 
                ISNULL(@ConfigJson, '{}'), 
                SYSUTCDATETIME(), 
                SYSUTCDATETIME()
            );

            SELECT SCOPE_IDENTITY() AS SavedId;
        END
    END

    -- 4. Publish banner
    ELSE IF @Flag = 'PUBLISH'
    BEGIN
        UPDATE [dbo].[BannerDesigns_CYH]
        SET Status = 'Published',
            PublishedAt = SYSUTCDATETIME(),
            UpdatedAt = SYSUTCDATETIME()
        WHERE Id = @Id
          AND (@UserId IS NULL OR UserId = @UserId OR UserId IS NULL);

        SELECT @@ROWCOUNT AS RowsAffected;
    END

    -- 5. Delete banner
    ELSE IF @Flag = 'DELETE'
    BEGIN
        DELETE FROM [dbo].[BannerDesigns_CYH] 
        WHERE Id = @Id
          AND (@UserId IS NULL OR UserId = @UserId OR UserId IS NULL);

        SELECT @@ROWCOUNT AS RowsAffected;
    END
END
GO

-- ============================================================================
-- 10. Stored Procedure: ManageDoctorAiSettings_CYH
-- Purpose: AI API Key configuration management (Doctor and Hospital Global)
-- ============================================================================
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

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
    @IsActive BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Get active key for execution (Priority: Doctor-specific key first, then Global Hospital fallback)
    IF @Flag = 'GET_ACTIVE_KEY'
    BEGIN
        DECLARE @Key NVARCHAR(500) = NULL;
        DECLARE @SourceDoctorName NVARCHAR(200) = NULL;

        SELECT TOP 1 
            @Key = ApiKey,
            @SourceDoctorName = DoctorName
        FROM DoctorAiSettings_CYH
        WHERE IsActive = 1
          AND UPPER(LTRIM(RTRIM(Provider))) = UPPER(LTRIM(RTRIM(@Provider)))
          AND ((@DoctorId IS NOT NULL AND DoctorId = @DoctorId) OR (@UserId IS NOT NULL AND UserId = @UserId))
          AND LTRIM(RTRIM(ApiKey)) <> ''
        ORDER BY UpdatedAt DESC;

        IF @Key IS NULL OR @Key = ''
        BEGIN
            SELECT TOP 1 
                @Key = ApiKey,
                @SourceDoctorName = 'Hospital Global'
            FROM DoctorAiSettings_CYH
            WHERE IsActive = 1
              AND UPPER(LTRIM(RTRIM(Provider))) = UPPER(LTRIM(RTRIM(@Provider)))
              AND DoctorId IS NULL
              AND LTRIM(RTRIM(ApiKey)) <> ''
            ORDER BY UpdatedAt DESC;
        END

        SELECT @Key AS ApiKey, @SourceDoctorName AS SourceDoctorName;
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
END
GO

-- ============================================================================
-- 11. Stored Procedure: DecryptDocPassword
-- Purpose: Decrypts doctor / user password in SQL Server
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
                ELSE 'powershell -File "D:\Myd\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + ISNULL(u.Password, '') + '"'
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
        DECLARE @cmd NVARCHAR(4000) = 'powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Myd\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + @Cipher + '"';
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
            'powershell -File "D:\Myd\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + @Cipher + '"' AS DecryptedPassword;
    END CATCH
END
GO
