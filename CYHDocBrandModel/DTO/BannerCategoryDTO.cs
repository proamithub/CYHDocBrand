namespace CYHDocBrandModel.DTO;

public class BannerCategoryDTO
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string IconClass { get; set; } = "bi-folder-fill";
    public string BadgeColor { get; set; } = "#00f2fe";
    public int DisplayOrder { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public int TemplateCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
