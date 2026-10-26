namespace CYHDocBrandModel.DTO;

public class BannerGenerateRequest
{
    public string Provider { get; set; } = "Gemini"; // Gemini, OpenAI, Custom
    public string AspectRatio { get; set; } = "1:1"; // 1:1, 4:5, 9:16, 16:9
    public string TemplateType { get; set; } = "Hospital"; // Hospital, Doctor, Clinic, Lab, Awareness, Promotional
    public string PromptTheme { get; set; } = "Modern clean hospital aesthetic with soft medical lighting";
    public string CustomPrompt { get; set; } = string.Empty;

    // Doctor & SQL Mapping
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string? DoctorName { get; set; }
    public string? ApiKeyOverride { get; set; }
}

public class BannerGenerationResult
{
    public bool Success { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageBase64 { get; set; }
    public string? ErrorMessage { get; set; }
    public string ProviderUsed { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class BannerOverlayImage
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Overlay Image";
    public string ImageUrl { get; set; } = string.Empty;
    public double X { get; set; } = 100;
    public double Y { get; set; } = 480;
    public double Width { get; set; } = 180;
    public double Height { get; set; } = 180;
    public bool IsCircle { get; set; } = true;
    public bool IsVisible { get; set; } = true;
}

public class BannerCustomizationState
{
    public int Id { get; set; } = 0; // 0 = New, >0 = Saved in Database
    public string Status { get; set; } = "Draft"; // "Draft" or "Published"

    // Content
    public string TemplateType { get; set; } = "Doctor";
    public string Heading { get; set; } = "World Heart Day Awareness";
    public string Subtitle { get; set; } = "Keep your heart healthy with regular preventive cardiac screenings.";
    public string DoctorName { get; set; } = "Dr. Rajesh Sharma, MD, DM";
    public string Specialization { get; set; } = "Chief Consultant - Interventional Cardiology";
    public string HospitalName { get; set; } = "CYH Super Specialty Hospital";
    public string ContactPhone { get; set; } = "+91 98765 43210";
    public string EmergencyPhone { get; set; } = "Helpline: 1800-123-4567";
    public string Address { get; set; } = "Main Boulevard, Healthcare City";
    public string Website { get; set; } = "https://www.clickyourhospital.com";
    public string BadgeText { get; set; } = "HEALTH MATTERS";

    // Appearance & Layout
    public string AspectRatio { get; set; } = "1:1"; // 1:1, 4:5, 9:16, 16:9
    public string PrimaryColor { get; set; } = "#00f2fe"; // Cyan accent
    public string SecondaryColor { get; set; } = "#0a192f"; // Dark Navy overlay
    public string TextColor { get; set; } = "#ffffff";
    public string AccentBadgeColor { get; set; } = "#10b981"; // Emerald
    public string FontFamily { get; set; } = "'Poppins', 'Noto Sans Devanagari', sans-serif";

    // Language Support & Multi-language Typography
    public string Language { get; set; } = "en"; // "en" (English) or "hi" (Hindi)
    public string HeadingFontFamily { get; set; } = "'Poppins', 'Noto Sans Devanagari', sans-serif";
    public string SubtitleFontFamily { get; set; } = "'Poppins', 'Noto Sans Devanagari', sans-serif";

    // Typography & Font Sizing
    public int HeadingFontSize { get; set; } = 54; // px
    public int SubtitleFontSize { get; set; } = 28; // px
    public int SubtitleLineHeight { get; set; } = 38; // px
    public string SubtitleFontWeight { get; set; } = "400"; // 400, 500, 600

    // Doctor Card Sizing, Font & Spacing
    public double DoctorCardScale { get; set; } = 1.0; // 0.6 to 1.5
    public int? DoctorCardWidth { get; set; } = null; // null = auto full width
    public int? DoctorCardHeight { get; set; } = null; // null = auto
    public int DoctorCardPadding { get; set; } = 20; // px
    public int DoctorCardBorderRadius { get; set; } = 20; // px
    public int DoctorNameFontSize { get; set; } = 34; // px
    public int DoctorSpecFontSize { get; set; } = 22; // px
    public string DoctorCardFontFamily { get; set; } = "Inter, sans-serif";
    public string DoctorCardBgColor { get; set; } = "rgba(15, 23, 42, 0.85)";
    public string DoctorCardBorderColor { get; set; } = "rgba(255, 255, 255, 0.15)";
    public bool ShowDoctorCardAccentBar { get; set; } = true;

    // Background Image & Compression Info
    public string? BackgroundImageFileName { get; set; }
    public long? BackgroundImageSizeBytes { get; set; }
    
    // Branding
    public string LogoUrl { get; set; } = "images/branding/cyh-logo.svg";
    public string LogoPosition { get; set; } = "bottom-center"; // bottom-center, bottom-left, bottom-right
    public int LogoSizePercent { get; set; } = 14; // % of banner width
    public int LogoPaddingBottom { get; set; } = 30; // px
    public bool ShowLogoBadge { get; set; } = true;
    public string LogoBadgeText { get; set; } = "CYH DOCTORHUB OFFICIAL BRANDING";

    // Active AI Background
    public string? BackgroundImageUrl { get; set; }
    public string BackgroundStyle { get; set; } = "gradient"; // "ai-image" or "gradient"
    public string GradientType { get; set; } = "medical-navy";

    // Drag & Drop Positions (Custom coordinates modified by mouse)
    public double? HeadingX { get; set; }
    public double? HeadingY { get; set; }
    public double? SubtitleX { get; set; }
    public double? SubtitleY { get; set; }
    public double? DoctorCardX { get; set; }
    public double? DoctorCardY { get; set; }
    public double? BadgeX { get; set; }
    public double? BadgeY { get; set; }
    public double? HospitalNameX { get; set; }
    public double? HospitalNameY { get; set; }

    // Multiple Added Images (Doctor photo, clinic badges, stamps, QR codes)
    public List<BannerOverlayImage> OverlayImages { get; set; } = new();
}

public class BannerDesignRecordDTO
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string HospitalName { get; set; } = string.Empty;
    public string TemplateType { get; set; } = "Doctor";
    public string Heading { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string Status { get; set; } = "Draft"; // "Draft", "Published"
    public string AspectRatio { get; set; } = "1:1";
    public string PrimaryColor { get; set; } = "#00f2fe";
    public string? BackgroundImageUrl { get; set; }
    public string ConfigJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public BannerCustomizationState GetConfigState()
    {
        if (!string.IsNullOrWhiteSpace(ConfigJson))
        {
            try
            {
                var parsed = System.Text.Json.JsonSerializer.Deserialize<BannerCustomizationState>(ConfigJson);
                if (parsed != null)
                {
                    parsed.Id = Id;
                    parsed.Status = Status;
                    if (!string.IsNullOrWhiteSpace(Heading)) parsed.Heading = Heading;
                    if (!string.IsNullOrWhiteSpace(DoctorName)) parsed.DoctorName = DoctorName;
                    if (!string.IsNullOrWhiteSpace(HospitalName)) parsed.HospitalName = HospitalName;
                    if (!string.IsNullOrWhiteSpace(AspectRatio)) parsed.AspectRatio = AspectRatio;
                    if (!string.IsNullOrWhiteSpace(BackgroundImageUrl)) parsed.BackgroundImageUrl = BackgroundImageUrl;
                    return parsed;
                }
            }
            catch { }
        }

        return new BannerCustomizationState
        {
            Id = Id,
            Status = Status,
            TemplateType = TemplateType,
            Heading = Heading,
            Subtitle = Subtitle ?? string.Empty,
            DoctorName = DoctorName,
            HospitalName = HospitalName,
            AspectRatio = AspectRatio,
            PrimaryColor = PrimaryColor,
            BackgroundImageUrl = BackgroundImageUrl
        };
    }
}
