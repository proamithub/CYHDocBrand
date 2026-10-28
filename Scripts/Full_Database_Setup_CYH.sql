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
        [CreatedAt]            DATETIME2 (7)      NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [IsActive]             BIT                NOT NULL DEFAULT (1),
        [UserName]             NVARCHAR (256)     NULL,
        [NormalizedUserName]   NVARCHAR (256)     NULL,
        [Email]                NVARCHAR (256)     NULL,
        [NormalizedEmail]      NVARCHAR (256)     NULL,
        [EmailConfirmed]       BIT                NOT NULL DEFAULT (1),
        [PasswordHash]         NVARCHAR (MAX)     NULL,
        [SecurityStamp]        NVARCHAR (MAX)     NULL,
        [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
                [PhoneNumber]          NVARCHAR (50)      NULL,
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

-- Unique indexes on Email and PhoneNumber to prevent duplicate user registrations
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_AspNetUsers_Email' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_AspNetUsers_Email] ON [dbo].[AspNetUsers] ([Email]) WHERE [Email] IS NOT NULL;
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_AspNetUsers_PhoneNumber' AND object_id = OBJECT_ID('AspNetUsers'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_AspNetUsers_PhoneNumber] ON [dbo].[AspNetUsers] ([PhoneNumber]) WHERE [PhoneNumber] IS NOT NULL;
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
        [DisplayName]      NVARCHAR (100)  NULL,
        [Qualifications]   NVARCHAR (200)  NULL,
        [MobileNo]         NVARCHAR (50)   NULL,
        [ClinicName]       NVARCHAR (200)  NULL,
        [ClinicAddress]    NVARCHAR (500)  NULL,
        [City]             NVARCHAR (100)  NULL,
        [State]            NVARCHAR (100)  NULL,
        [PinCode]          NVARCHAR (20)   NULL,
        [WebsiteUrl]       NVARCHAR (500)  NULL,
        [ClinicLogo]       NVARCHAR (MAX)  NULL,
        [PrimaryColorHex]  NVARCHAR (50)   NULL DEFAULT ('#00f2fe'),
        [SecondaryColorHex] NVARCHAR (50)  NULL DEFAULT ('#4facfe'),
        [PreferredFont]    NVARCHAR (50)   NULL DEFAULT ('Inter'),
        [AiCredits]        INT             NOT NULL DEFAULT (25),
        [IsVerified]       BIT             NOT NULL DEFAULT (1),
        [IsAvailableToday] BIT             NOT NULL DEFAULT (1),
        [CreatedAt]        DATETIME2 (7)   NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
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
        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [UpdatedAt]          DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [PublishedAt]        DATETIME2(7) NULL,
        CONSTRAINT [PK_BannerDesigns_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 4.1 Create BannerCategories_CYH Table (Dynamic Categories)
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
        [CreatedAt]    DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        CONSTRAINT [PK_BannerCategories_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 4.2 Create BannerTemplates_CYH Table (50 Medical & Branding Templates)
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
        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        CONSTRAINT [PK_BannerTemplates_CYH] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_BannerTemplates_Category] FOREIGN KEY ([CategoryId]) 
            REFERENCES [dbo].[BannerCategories_CYH] ([Id])
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
        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [UpdatedAt]          DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
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
            @ConsultationFee, @AvailableDays, @AvailableHours, @Bio, ISNULL(@IsAvailableToday, 1), DATEADD(minute, 330, SYSUTCDATETIME()), @ProfilePic
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
        DECLARE @Now DATETIME2(7) = DATEADD(minute, 330, SYSUTCDATETIME());

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
                'L4Yq5jzhlap0wTagsBRCUg==', DATEADD(minute, 330, SYSUTCDATETIME()), 1, 0, 0, 0, 0
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
                'n7uoG53mus5DzCXmNgv7NQ==', DATEADD(minute, 330, SYSUTCDATETIME()), 1, 0, 0, 0, 0
            );

            IF NOT EXISTS (SELECT 1 FROM DoctorProfiles WHERE FullName = 'Dr. A. Sharma')
            BEGIN
                INSERT INTO DoctorProfiles (UserId, FullName, Specialization, LicenseNumber, ExperienceYears, ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt)
                VALUES (
                    @Doc1UserId, 'Dr. A. Sharma', 'Chief Cardiologist', 'MED-DEL-98421', 14, 85.00,
                    'Mon - Sat', '09:00 AM - 04:00 PM',
                    'Specializing in interventional cardiology, heart failure management, and preventive cardiovascular wellness.',
                    1, DATEADD(minute, 330, SYSUTCDATETIME())
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
                'n7uoG53mus5DzCXmNgv7NQ==', DATEADD(minute, 330, SYSUTCDATETIME()), 1, 0, 0, 0, 0
            );

            IF NOT EXISTS (SELECT 1 FROM DoctorProfiles WHERE FullName = 'Dr. Priya Patel')
            BEGIN
                INSERT INTO DoctorProfiles (UserId, FullName, Specialization, LicenseNumber, ExperienceYears, ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt)
                VALUES (
                    @Doc2UserId, 'Dr. Priya Patel', 'Senior Neurologist', 'MED-MUM-77319', 10, 90.00,
                    'Mon, Wed, Fri', '10:00 AM - 05:00 PM',
                    'Expert in neuro-vascular disorders, neuro-rehabilitation, stroke management, and migraines.',
                    1, DATEADD(minute, 330, SYSUTCDATETIME())
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
                UpdatedAt = DATEADD(minute, 330, SYSUTCDATETIME())
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
                DATEADD(minute, 330, SYSUTCDATETIME()), 
                DATEADD(minute, 330, SYSUTCDATETIME())
            );

            SELECT SCOPE_IDENTITY() AS SavedId;
        END
    END

    -- 4. Publish banner
    ELSE IF @Flag = 'PUBLISH'
    BEGIN
        UPDATE [dbo].[BannerDesigns_CYH]
        SET Status = 'Published',
            PublishedAt = DATEADD(minute, 330, SYSUTCDATETIME()),
            UpdatedAt = DATEADD(minute, 330, SYSUTCDATETIME())
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
                UpdatedAt = DATEADD(minute, 330, SYSUTCDATETIME())
            WHERE Id = @ExistingId;

            SELECT @ExistingId AS ResultId;
        END
        ELSE
        BEGIN
            INSERT INTO DoctorAiSettings_CYH (DoctorId, UserId, DoctorName, Provider, ApiKey, ModelName, BaseUrl, IsActive, CreatedAt, UpdatedAt)
            VALUES (@DoctorId, @UserId, @DoctorName, @Provider, @ApiKey, @ModelName, @BaseUrl, ISNULL(@IsActive, 1), DATEADD(minute, 330, SYSUTCDATETIME()), DATEADD(minute, 330, SYSUTCDATETIME()));

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
                ELSE 'powershell -File "D:\MyAppAi\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + ISNULL(u.Password, '') + '"'
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
            'powershell -File "D:\MyAppAi\CYHDocBrand\Scripts\DecryptPassword.ps1" -EncryptedPassword "' + @Cipher + '"' AS DecryptedPassword;
    END CATCH
END
GO

-- ============================================================================
-- 12. Stored Procedure: ManageBannerCategories_CYH
-- Purpose: Complete Dynamic Category Management for Templates (CRUD & Live Counts)
-- ============================================================================
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
        (@CategoryName, @Slug, ISNULL(@IconClass, 'bi-folder-fill'), ISNULL(@BadgeColor, '#00f2fe'), ISNULL(@DisplayOrder, 0), ISNULL(@IsActive, 1), DATEADD(minute, 330, SYSUTCDATETIME()));

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
END
GO

-- ============================================================================
-- 13. Stored Procedure: ManageBannerTemplates_CYH
-- Purpose: Complete Dynamic Template Management (Filter by Category, Search, CRUD)
-- ============================================================================
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
                ISNULL(@DisplayOrder, 0), ISNULL(@IsActive, 1), DATEADD(minute, 330, SYSUTCDATETIME())
            );

            SELECT SCOPE_IDENTITY() AS ResultId;
        END
    END

    ELSE IF @Flag = 'DELETE'
    BEGIN
        DELETE FROM [dbo].[BannerTemplates_CYH] WHERE Id = @Id;
        SELECT @@ROWCOUNT AS RowsAffected;
    END
END
GO

-- =========================================================================================
-- CREDIT SYSTEM TABLES & STORED PROCEDURES (Database-Managed, Doctor-Wise)
-- =========================================================================================

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
        [CreatedAt]    DATETIME2(7)  NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        CONSTRAINT [PK_CreditTypes_CYH] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_CreditTypes_Code] UNIQUE ([Code])
    );
END
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorCredits_CYH]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DoctorCredits_CYH] (
        [Id]             INT IDENTITY(1,1) NOT NULL,
        [DoctorId]       INT NOT NULL,
        [CreditTypeId]   INT NOT NULL,
        [TotalCredits]   INT NOT NULL DEFAULT (0),
        [UsedCredits]    INT NOT NULL DEFAULT (0),
        [IsActive]       BIT NOT NULL DEFAULT (1),
        [LastUpdatedAt]  DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        CONSTRAINT [PK_DoctorCredits_CYH] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UQ_Doctor_CreditType] UNIQUE ([DoctorId], [CreditTypeId])
    );
    CREATE NONCLUSTERED INDEX [IX_DoctorCredits_DoctorId] ON [dbo].[DoctorCredits_CYH]([DoctorId]);
END
GO

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
        [CreatedAt]       DATETIME2(7)  NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        CONSTRAINT [PK_CreditTransactions_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE NONCLUSTERED INDEX [IX_CreditTransactions_DoctorId] ON [dbo].[CreditTransactions_CYH]([DoctorId]);
    CREATE NONCLUSTERED INDEX [IX_CreditTransactions_CreatedAt] ON [dbo].[CreditTransactions_CYH]([CreatedAt] DESC);
END
GO

-- 22. Create PasswordChangeHistory_CYH Table (Audit Log for Passwords with Geolocation & IP)
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
        [ChangedAt]        DATETIME2(7)      NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        CONSTRAINT [PK_PasswordChangeHistory_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE NONCLUSTERED INDEX [IX_PasswordChangeHistory_UserId] ON [dbo].[PasswordChangeHistory_CYH]([UserId]);
    CREATE NONCLUSTERED INDEX [IX_PasswordChangeHistory_ChangedAt] ON [dbo].[PasswordChangeHistory_CYH]([ChangedAt] DESC);
END
GO

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
        SELECT @DoctorId, ct.Id, 0, 0, 1, DATEADD(minute, 330, SYSUTCDATETIME())
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
            SELECT @DoctorId, ct.Id, 'SYSTEM_INIT', 0, 0, 0, 'System', 'Doctor account credit initialization at 0', DATEADD(minute, 330, SYSUTCDATETIME())
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
            (@Name, @Code, ISNULL(@ServiceType, 'Free'), ISNULL(@IsPaid, 0), ISNULL(@IsActive, 1), @Description, ISNULL(@CreditCost, 1), DATEADD(minute, 330, SYSUTCDATETIME()));

            DECLARE @NewTypeId INT = SCOPE_IDENTITY();

            -- Auto add this new credit type to all existing doctors with 0
            INSERT INTO [dbo].[DoctorCredits_CYH] (DoctorId, CreditTypeId, TotalCredits, UsedCredits, IsActive, LastUpdatedAt)
            SELECT d.Id, @NewTypeId, 0, 0, 1, DATEADD(minute, 330, SYSUTCDATETIME())
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
            VALUES (@DoctorId, @CreditTypeId, 0, 0, 1, DATEADD(minute, 330, SYSUTCDATETIME()));
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
            LastUpdatedAt = DATEADD(minute, 330, SYSUTCDATETIME())
        WHERE DoctorId = @DoctorId AND CreditTypeId = @CreditTypeId;

        INSERT INTO [dbo].[CreditTransactions_CYH]
        (DoctorId, CreditTypeId, TransactionType, CreditsChanged, BalanceBefore, BalanceAfter, ActionBy, Note, CreatedAt)
        VALUES
        (@DoctorId, @CreditTypeId, @AdjustTxType, @Amount, @CurrentBalance, @NewBalance, ISNULL(@ActionBy, 'Admin'), @Note, DATEADD(minute, 330, SYSUTCDATETIME()));

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
            VALUES (@DoctorId, @FoundTypeId, 0, 0, 1, DATEADD(minute, 330, SYSUTCDATETIME()));
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
            LastUpdatedAt = DATEADD(minute, 330, SYSUTCDATETIME())
        WHERE DoctorId = @DoctorId AND CreditTypeId = @FoundTypeId;

        INSERT INTO [dbo].[CreditTransactions_CYH]
        (DoctorId, CreditTypeId, TransactionType, CreditsChanged, BalanceBefore, BalanceAfter, ActionBy, ReferenceId, Note, CreatedAt)
        VALUES
        (@DoctorId, @FoundTypeId, 'DEBIT_ON_PUBLISH', -@CreditCostToDeduct, @DocBalanceBefore, @DocBalanceAfter, ISNULL(@ActionBy, 'Doctor'), @ReferenceId, @Note, DATEADD(minute, 330, SYSUTCDATETIME()));

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
END
GO


-- ============================================================================
-- 10. Doctor Social Accounts Table
-- ============================================================================
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
        [ConnectedAt]      DATETIME2(7)  NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [LastTestedAt]     DATETIME2(7)  NULL,
        [LastTestStatus]   NVARCHAR(50)  NULL,
        [LastTestMessage]  NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_DoctorSocialAccounts_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE NONCLUSTERED INDEX [IX_DoctorSocialAccounts_DoctorId] ON [dbo].[DoctorSocialAccounts_CYH]([DoctorId]);
    CREATE NONCLUSTERED INDEX [IX_DoctorSocialAccounts_UserId] ON [dbo].[DoctorSocialAccounts_CYH]([UserId]);
END
GO

-- ============================================================================
-- 11. Banner Schedules Master Table
-- ============================================================================
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
        [CreatedAt]          DATETIME2(7) NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [CreatedBy]          NVARCHAR(200) NULL,
        CONSTRAINT [PK_BannerSchedules_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE NONCLUSTERED INDEX [IX_BannerSchedules_ScheduledAt_Status] ON [dbo].[BannerSchedules_CYH]([ScheduledAt], [Status]);
    CREATE NONCLUSTERED INDEX [IX_BannerSchedules_DoctorId] ON [dbo].[BannerSchedules_CYH]([DoctorId]);
END
GO

-- ============================================================================
-- 12. Banner Schedule Details (Per-Platform Breakdown)
-- ============================================================================
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
END
GO

-- 15. Create DoctorPushSubscriptions_CYH Table (Web Push VAPID Device Subscriptions)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DoctorPushSubscriptions_CYH]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[DoctorPushSubscriptions_CYH] (
        [Id]           INT IDENTITY(1,1) NOT NULL,
        [DoctorId]     INT               NULL,
        [UserId]       NVARCHAR(450)     NULL,
        [Endpoint]     NVARCHAR(MAX)     NOT NULL,
        [P256dh]       NVARCHAR(500)     NOT NULL,
        [Auth]         NVARCHAR(500)     NOT NULL,
        [UserAgent]    NVARCHAR(500)     NULL,
        [CreatedAt]    DATETIME2(7)      NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        [LastActiveAt] DATETIME2(7)      NOT NULL DEFAULT (DATEADD(minute, 330, SYSUTCDATETIME())),
        CONSTRAINT [PK_DoctorPushSubscriptions_CYH] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
    CREATE NONCLUSTERED INDEX [IX_DoctorPush_DoctorId] ON [dbo].[DoctorPushSubscriptions_CYH]([DoctorId]);
    CREATE NONCLUSTERED INDEX [IX_DoctorPush_UserId] ON [dbo].[DoctorPushSubscriptions_CYH]([UserId]);
END
GO
