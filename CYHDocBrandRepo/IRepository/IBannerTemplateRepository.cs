using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IBannerTemplateRepository
{
    Task<List<BannerCategoryDTO>> GetCategoriesAsync(bool onlyActive = true);
    Task<List<BannerTemplateDTO>> GetAllTemplatesAsync(int? categoryId = null, string? style = null, string? search = null);
    Task<BannerTemplateDTO?> GetTemplateByIdAsync(int id);
    Task<int> SaveTemplateAsync(BannerTemplateDTO template);
    Task<bool> DeleteTemplateAsync(int id);
}
