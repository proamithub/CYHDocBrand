using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IBannerScheduleRepository
{
    Task<int> CreateScheduleAsync(CreateBannerScheduleRequest request, string? createdBy);
    Task<int> CreateScheduleAsync(CreateBannerScheduleRequest request, List<DoctorSocialAccountDTO>? targetAccounts, string? createdBy);
    Task<List<BannerScheduleDTO>> GetDueSchedulesAsync();
    Task<bool> UpdateDetailStatusAsync(int detailId, string status, string? platformPostId, string? postUrl, string? errorMessage);
    Task<bool> UpdateScheduleStatusAsync(int scheduleId, string status, string? errorMessage);
    Task<List<BannerScheduleDTO>> GetByDoctorAsync(int? doctorId, string? userId);
    Task<AdminScheduleSummaryDTO> GetAllAdminAsync(int? doctorId = null, string? status = null, string? search = null);
    Task<bool> CancelScheduleAsync(int scheduleId);
    Task<bool> SnoozeScheduleAsync(int scheduleId, int minutes = 5);
    Task<bool> RetryScheduleAsync(int scheduleId);
    Task<bool> DeleteScheduleAsync(int scheduleId);
}
