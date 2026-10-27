using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface ISocialAccountRepository
{
    Task<List<DoctorSocialAccountDTO>> GetByDoctorAsync(int? doctorId, string? userId);
    Task<DoctorSocialAccountDTO?> GetByIdAsync(int id);
    Task<int> SaveAccountAsync(SocialAccountSaveRequest request);
    Task<bool> UpdateTestStatusAsync(int id, string status, string? message);
    Task<bool> DeleteAccountAsync(int id);
    Task<bool> ToggleActiveAsync(int id);
}
