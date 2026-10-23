using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class DashboardRepository : IDashboardRepository
{
    private readonly string _conn;
    private readonly ILogger<DashboardRepository>? _logger;

    public DashboardRepository(IConfiguration configuration, ILogger<DashboardRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=VRS-AMITPC\\SQLEXPRESS;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    public async Task<AdminDashboardDTO> GetAdminStatsAsync()
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("GetDashboardStats_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "ADMIN_STATS");

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AdminDashboardDTO
                {
                    TotalDoctors = reader.IsDBNull(reader.GetOrdinal("TotalDoctors")) ? 0 : Convert.ToInt32(reader["TotalDoctors"]),
                    ActiveDoctorsOnDuty = reader.IsDBNull(reader.GetOrdinal("ActiveDoctorsOnDuty")) ? 0 : Convert.ToInt32(reader["ActiveDoctorsOnDuty"]),
                    TotalDepartments = reader.IsDBNull(reader.GetOrdinal("TotalDepartments")) ? 0 : Convert.ToInt32(reader["TotalDepartments"]),
                    TotalUsers = reader.IsDBNull(reader.GetOrdinal("TotalUsers")) ? 0 : Convert.ToInt32(reader["TotalUsers"])
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetAdminStatsAsync failed via GetDashboardStats_CYH");
        }
        return new AdminDashboardDTO();
    }

    public async Task<DoctorDashboardDTO> GetDoctorStatsAsync(int doctorId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("GetDashboardStats_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "DOCTOR_STATS");
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new DoctorDashboardDTO
                {
                    DoctorId = doctorId,
                    DoctorName = reader["DoctorName"]?.ToString() ?? string.Empty,
                    Specialization = reader["Specialization"]?.ToString() ?? string.Empty,
                    LicenseNumber = reader["LicenseNumber"]?.ToString() ?? string.Empty,
                    ExperienceYears = reader.IsDBNull(reader.GetOrdinal("ExperienceYears")) ? 0 : Convert.ToInt32(reader["ExperienceYears"]),
                    ConsultationFee = reader.IsDBNull(reader.GetOrdinal("ConsultationFee")) ? 0m : Convert.ToDecimal(reader["ConsultationFee"]),
                    AvailableHours = reader["AvailableHours"]?.ToString() ?? string.Empty,
                    AvailableDays = reader["AvailableDays"]?.ToString() ?? string.Empty,
                    Bio = reader["Bio"]?.ToString() ?? string.Empty,
                    IsAvailableToday = !reader.IsDBNull(reader.GetOrdinal("IsAvailableToday")) && Convert.ToBoolean(reader["IsAvailableToday"])
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetDoctorStatsAsync failed via GetDashboardStats_CYH for DoctorId {DoctorId}", doctorId);
        }
        return new DoctorDashboardDTO { DoctorId = doctorId };
    }
}
