using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IDoctorAiSettingRepository
{
    Task<string?> GetActiveApiKeyAsync(int? doctorId, string? userId, string provider);
    Task<List<DoctorAiSettingDTO>> GetSettingsForDoctorAsync(int? doctorId, string? userId);
    Task<List<DoctorAiSettingDTO>> GetAllSettingsAsync();
    Task<bool> SaveSettingAsync(DoctorAiSettingDTO setting);
    Task<bool> DeleteSettingAsync(int id);
}
