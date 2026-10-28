using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IMediaPlatformRepository
{
    Task<List<MediaPlatformDTO>> GetAllAsync();
    Task<List<MediaPlatformDTO>> GetActiveAsync();
    Task<MediaPlatformDTO?> GetByIdAsync(int id);
    Task<bool> ToggleActiveAsync(int id, string? updatedBy);
    Task<bool> UpdateGlobalApiAsync(UpdateMediaPlatformApiRequest request);
}
