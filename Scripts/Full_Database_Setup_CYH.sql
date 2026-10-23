-- ============================================================================
-- CYH DoctorHub — Complete Production Database Setup Script
-- Target: SQL Server (VPS Server: https://vps2.clickyourhospital.com)
-- Database: CYHDocBrandBD
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
        [IsAvailableToday] BIT             NOT NULL DEFAULT (1),
        [CreatedAt]        DATETIME2 (7)   NOT NULL DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_DoctorProfiles] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_DoctorProfiles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END
GO

-- ============================================================================
-- 4. Stored Procedure: AuthenticateDoctor_CYH
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
            d.Id AS DoctorProfileId
        FROM AspNetUsers u
        LEFT JOIN DoctorProfiles d ON u.Id = d.UserId
        WHERE (u.Email = @Email OR u.UserName = @Email)
          AND u.Password = @Password
          AND u.IsActive = 1;
    END
END
GO

-- ============================================================================
-- 5. Stored Procedure: ManageDoctor_CYH
-- ============================================================================
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
    @Bio NVARCHAR(500) = NULL,
    @IsAvailableToday BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

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
            u.Id AS AppUserId,
            u.UserName,
            u.Email,
            u.PhoneNumber,
            u.FullName AS UserFullName,
            u.Role,
            u.Department,
            u.IsActive
        FROM DoctorProfiles d
        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
        ORDER BY d.FullName ASC;
    END
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
            u.Id AS AppUserId,
            u.UserName,
            u.Email,
            u.PhoneNumber,
            u.FullName AS UserFullName,
            u.Role,
            u.Department,
            u.IsActive
        FROM DoctorProfiles d
        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
        WHERE d.Id = @DoctorId;
    END
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
            u.Id AS AppUserId,
            u.UserName,
            u.Email,
            u.PhoneNumber,
            u.FullName AS UserFullName,
            u.Role,
            u.Department,
            u.IsActive
        FROM DoctorProfiles d
        LEFT JOIN AspNetUsers u ON d.UserId = u.Id
        WHERE d.UserId = @UserId;
    END
    ELSE IF @Flag = 'TOGGLE_DUTY'
    BEGIN
        UPDATE DoctorProfiles
        SET IsAvailableToday = CASE WHEN IsAvailableToday = 1 THEN 0 ELSE 1 END
        WHERE Id = @DoctorId;

        SELECT @@ROWCOUNT AS RowsAffected;
    END
    ELSE IF @Flag = 'INSERT'
    BEGIN
        INSERT INTO DoctorProfiles 
        (
            UserId, FullName, Specialization, LicenseNumber, ExperienceYears, 
            ConsultationFee, AvailableDays, AvailableHours, Bio, IsAvailableToday, CreatedAt
        )
        VALUES 
        (
            @UserId, @FullName, @Specialization, @LicenseNumber, @ExperienceYears,
            @ConsultationFee, @AvailableDays, @AvailableHours, @Bio, ISNULL(@IsAvailableToday, 1), SYSUTCDATETIME()
        );

        SELECT SCOPE_IDENTITY() AS NewId;
    END
    ELSE IF @Flag = 'SEED_INITIAL_DATA'
    BEGIN
        -- 1. Hospital Admin User (Password: Admin@12345 in AES-256)
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

        -- 2. Doctor 1: Dr. A. Sharma (Password: Doctor@12345 in AES-256)
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

        -- 3. Doctor 2: Dr. Priya Patel (Password: Doctor@12345 in AES-256)
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
-- 6. Stored Procedure: GetDashboardStats_CYH
-- ============================================================================
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
-- 7. Execute Initial Seed (Users & Doctors with AES-256 Passwords)
-- ============================================================================
EXEC dbo.ManageDoctor_CYH @Flag = 'SEED_INITIAL_DATA';
GO
