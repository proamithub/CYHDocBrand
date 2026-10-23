using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IDashboardRepository
{
    Task<AdminDashboardDTO> GetAdminStatsAsync();
    Task<DoctorDashboardDTO> GetDoctorStatsAsync(int doctorId);
}
