using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandUI.Services;

public interface IAnalyticsService
{
    Task<AdminDashboardDTO> GetAdminStatsAsync();
    Task<DoctorDashboardDTO> GetDoctorStatsAsync(int doctorId);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly IDashboardRepository _dashboardRepo;

    public AnalyticsService(IDashboardRepository dashboardRepo)
    {
        _dashboardRepo = dashboardRepo;
    }

    public async Task<AdminDashboardDTO> GetAdminStatsAsync()
    {
        return await _dashboardRepo.GetAdminStatsAsync();
    }

    public async Task<DoctorDashboardDTO> GetDoctorStatsAsync(int doctorId)
    {
        return await _dashboardRepo.GetDoctorStatsAsync(doctorId);
    }
}
