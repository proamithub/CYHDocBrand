using CYHDocBrandModel.Common;
using System.Text.Json;

namespace CYHDocBrandModel.DTO;

public class BannerTemplateDTO
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategoryIcon { get; set; } = "bi-folder-fill";
    public string Title { get; set; } = string.Empty;
    public string StyleTheme { get; set; } = "Modern Clean";
    public string AspectRatio { get; set; } = "1:1";
    public string BackgroundImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string PrimaryColor { get; set; } = "#00f2fe";
    public string SecondaryColor { get; set; } = "#0a192f";
    public string TextColor { get; set; } = "#ffffff";
    public string DefaultHeading { get; set; } = string.Empty;
    public string? DefaultSubtitle { get; set; }
    public string? BadgeText { get; set; }
    
    private string? _language;
    public string Language
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(_language)) return _language;
            bool hasHindi = System.Text.RegularExpressions.Regex.IsMatch((DefaultHeading ?? "") + " " + (DefaultSubtitle ?? "") + " " + (Title ?? ""), @"[\u0900-\u097F]");
            return hasHindi ? "hi" : "en";
        }
        set => _language = value;
    }

    public string ConfigJson { get; set; } = string.Empty;
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = IndianTimeHelper.Now;

    public BannerCustomizationState GetConfigState()
    {
        if (string.IsNullOrWhiteSpace(ConfigJson))
        {
            var st = new BannerCustomizationState();
            st.Language = Language;
            return st;
        }
        try
        {
            var st = JsonSerializer.Deserialize<BannerCustomizationState>(ConfigJson) ?? new BannerCustomizationState();
            if (string.IsNullOrWhiteSpace(st.Language))
                st.Language = Language;
            return st;
        }
        catch
        {
            var st = new BannerCustomizationState();
            st.Language = Language;
            return st;
        }
    }
}
