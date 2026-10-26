using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class DoctorAiSettingRepository : IDoctorAiSettingRepository
{
    private readonly string _conn;
    private readonly ILogger<DoctorAiSettingRepository>? _logger;

    public DoctorAiSettingRepository(IConfiguration configuration, ILogger<DoctorAiSettingRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    public async Task<string?> GetActiveApiKeyAsync(int? doctorId, string? userId, string provider)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorAiSettings_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_ACTIVE_KEY");
            cmd.Parameters.AddWithValue("@Provider", provider.Trim());
            cmd.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var key = reader["ApiKey"]?.ToString();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    return key.Trim();
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting active AI key from SQL for Provider {Provider}, DoctorId {DoctorId}", provider, doctorId);
        }
        return null;
    }

    public async Task<List<DoctorAiSettingDTO>> GetSettingsForDoctorAsync(int? doctorId, string? userId)
    {
        var list = new List<DoctorAiSettingDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorAiSettings_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_DOCTOR");
            cmd.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToDTO(reader));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting doctor AI settings for DoctorId {DoctorId}", doctorId);
        }
        return list;
    }

    public async Task<List<DoctorAiSettingDTO>> GetAllSettingsAsync()
    {
        var list = new List<DoctorAiSettingDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorAiSettings_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_ALL");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToDTO(reader));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting all AI settings from SQL");
        }
        return list;
    }

    public async Task<bool> SaveSettingAsync(DoctorAiSettingDTO setting)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorAiSettings_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "UPSERT");
            cmd.Parameters.AddWithValue("@Id", setting.Id > 0 ? setting.Id : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@DoctorId", (object?)setting.DoctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)setting.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DoctorName", (object?)setting.DoctorName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Provider", setting.Provider.Trim());
            cmd.Parameters.AddWithValue("@ApiKey", setting.ApiKey.Trim());
            cmd.Parameters.AddWithValue("@ModelName", (object?)setting.ModelName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BaseUrl", (object?)setting.BaseUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", setting.IsActive);

            var res = await cmd.ExecuteScalarAsync();
            return res != null && res != DBNull.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving doctor AI setting in SQL for Provider {Provider}", setting.Provider);
            return false;
        }
    }

    public async Task<bool> DeleteSettingAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorAiSettings_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "DELETE");
            cmd.Parameters.AddWithValue("@Id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting doctor AI setting with ID {Id}", id);
            return false;
        }
    }

    private static DoctorAiSettingDTO MapReaderToDTO(SqlDataReader reader)
    {
        return new DoctorAiSettingDTO
        {
            Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
            DoctorId = reader["DoctorId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorId"]) : null,
            UserId = reader["UserId"] != DBNull.Value ? reader["UserId"].ToString() : null,
            DoctorName = reader["DoctorName"] != DBNull.Value ? reader["DoctorName"].ToString() : null,
            Provider = reader["Provider"]?.ToString() ?? "Gemini",
            ApiKey = reader["ApiKey"]?.ToString() ?? string.Empty,
            ModelName = reader["ModelName"] != DBNull.Value ? reader["ModelName"].ToString() : null,
            BaseUrl = reader["BaseUrl"] != DBNull.Value ? reader["BaseUrl"].ToString() : null,
            IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
            CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.UtcNow,
            UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedAt"]) : DateTime.UtcNow
        };
    }
}
