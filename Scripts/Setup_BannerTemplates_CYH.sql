-- =========================================================================
-- CYHDocBrand: Categories & 50 Banner Templates Setup Script
-- =========================================================================

-- 1. Create BannerCategories_CYH Table
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

-- 2. Create BannerTemplates_CYH Table
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

-- 3. Stored Procedure: ManageBannerCategories_CYH
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

-- 4. Stored Procedure: ManageBannerTemplates_CYH
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
