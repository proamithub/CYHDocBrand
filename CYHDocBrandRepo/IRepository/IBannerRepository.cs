using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IBannerRepository
{
    Task<List<BannerDesignRecordDTO>> GetAllBannersAsync(string? userId = null, string? status = null, string? search = null);
    Task<BannerDesignRecordDTO?> GetBannerByIdAsync(int id);
    Task<int> SaveDraftAsync(BannerCustomizationState state, string? userId);
    Task<bool> PublishBannerAsync(int id, string? userId);
    Task<bool> DeleteBannerAsync(int id, string? userId);
}
