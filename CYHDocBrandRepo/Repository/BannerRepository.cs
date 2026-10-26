using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class BannerRepository : IBannerRepository
{
    private readonly string _conn;
    private readonly ILogger<BannerRepository>? _logger;

    public BannerRepository(IConfiguration configuration, ILogger<BannerRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    private static string? Truncate(string? value, int maxLength, string? fallback = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength);
    }

    // ── ManageBannerDesigns_CYH Stored Procedure ─────────────────────────────

    public async Task<List<BannerDesignRecordDTO>> GetAllBannersAsync(string? userId = null, string? status = null, string? search = null)
    {
        var list = new List<BannerDesignRecordDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerDesigns_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_ALL");
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Search", (object?)search?.Trim() ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToRecord(reader));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting banner designs from database via ManageBannerDesigns_CYH");
            throw;
        }
        return list;
    }

    public async Task<BannerDesignRecordDTO?> GetBannerByIdAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerDesigns_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_ID");
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToRecord(reader);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting banner design with ID {Id} via ManageBannerDesigns_CYH", id);
            throw;
        }
        return null;
    }

    public async Task<int> SaveDraftAsync(BannerCustomizationState state, string? userId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            var json = JsonSerializer.Serialize(state);

            await using var cmd = new SqlCommand("ManageBannerDesigns_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "SAVE_DRAFT");
            cmd.Parameters.AddWithValue("@Id", state.Id > 0 ? state.Id : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DoctorName", Truncate(state.DoctorName, 200, "Dr. Specialist, MD")!);
            cmd.Parameters.AddWithValue("@HospitalName", (object?)Truncate(state.HospitalName, 200, null) ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TemplateType", Truncate(state.TemplateType, 100, "Doctor")!);
            cmd.Parameters.AddWithValue("@Heading", Truncate(state.Heading, 500, "Healthcare Consultation & Care")!);

            var subParam = cmd.Parameters.Add("@Subtitle", SqlDbType.NVarChar, -1);
            subParam.Value = (object?)state.Subtitle ?? DBNull.Value;

            cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(state.Status) ? "Draft" : Truncate(state.Status, 50, "Draft")!);
            cmd.Parameters.AddWithValue("@AspectRatio", Truncate(state.AspectRatio, 50, "1:1")!);
            cmd.Parameters.AddWithValue("@PrimaryColor", Truncate(state.PrimaryColor, 50, "#00f2fe")!);

            var bgParam = cmd.Parameters.Add("@BackgroundImageUrl", SqlDbType.NVarChar, -1);
            bgParam.Value = (object?)state.BackgroundImageUrl ?? DBNull.Value;

            var jsonParam = cmd.Parameters.Add("@ConfigJson", SqlDbType.NVarChar, -1);
            jsonParam.Value = json;

            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving banner draft to database via ManageBannerDesigns_CYH");
            throw;
        }
    }

    public async Task<bool> PublishBannerAsync(int id, string? userId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerDesigns_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "PUBLISH");
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Status", "Published");
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            var scalar = await cmd.ExecuteScalarAsync();
            return scalar != null && scalar != DBNull.Value && Convert.ToInt32(scalar) > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error publishing banner with ID {Id} via ManageBannerDesigns_CYH", id);
            throw;
        }
    }

    public async Task<bool> DeleteBannerAsync(int id, string? userId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            // Published banner delete lock: Only Admin can delete published banners.
            // If userId is provided (Doctor context), verify banner is not Published
            if (!string.IsNullOrEmpty(userId))
            {
                await using var checkCmd = new SqlCommand("SELECT Status FROM [dbo].[BannerDesigns_CYH] WHERE Id = @Id", con);
                checkCmd.Parameters.AddWithValue("@Id", id);
                var statusObj = await checkCmd.ExecuteScalarAsync();
                var status = statusObj?.ToString();
                if (string.Equals(status, "Published", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Published banner locked hai! Published banner ko sirf Administrator hi delete kar sakta hai.");
                }
            }

            await using var cmd = new SqlCommand("ManageBannerDesigns_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "DELETE");
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            var scalar = await cmd.ExecuteScalarAsync();
            return scalar != null && scalar != DBNull.Value && Convert.ToInt32(scalar) > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting banner with ID {Id} via ManageBannerDesigns_CYH", id);
            throw;
        }
    }

    private static BannerDesignRecordDTO MapReaderToRecord(SqlDataReader reader)
    {
        var configJson = string.Empty;
        try
        {
            var ord = reader.GetOrdinal("ConfigJson");
            if (ord >= 0 && !reader.IsDBNull(ord))
            {
                configJson = reader.GetString(ord);
            }
        }
        catch { }

        return new BannerDesignRecordDTO
        {
            Id = Convert.ToInt32(reader["Id"]),
            UserId = reader["UserId"] != DBNull.Value ? reader["UserId"].ToString() : null,
            DoctorName = reader["DoctorName"] != DBNull.Value ? reader["DoctorName"].ToString() ?? string.Empty : string.Empty,
            HospitalName = reader["HospitalName"] != DBNull.Value ? reader["HospitalName"].ToString() ?? string.Empty : string.Empty,
            TemplateType = reader["TemplateType"] != DBNull.Value ? reader["TemplateType"].ToString() ?? "Doctor" : "Doctor",
            Heading = reader["Heading"] != DBNull.Value ? reader["Heading"].ToString() ?? string.Empty : string.Empty,
            Subtitle = reader["Subtitle"] != DBNull.Value ? reader["Subtitle"].ToString() : null,
            Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() ?? "Draft" : "Draft",
            AspectRatio = reader["AspectRatio"] != DBNull.Value ? reader["AspectRatio"].ToString() ?? "1:1" : "1:1",
            PrimaryColor = reader["PrimaryColor"] != DBNull.Value ? reader["PrimaryColor"].ToString() ?? "#00f2fe" : "#00f2fe",
            BackgroundImageUrl = reader["BackgroundImageUrl"] != DBNull.Value ? reader["BackgroundImageUrl"].ToString() : null,
            ConfigJson = configJson,
            CreatedAt = reader["CreatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["CreatedAt"]) : DateTime.UtcNow,
            UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedAt"]) : DateTime.UtcNow,
            PublishedAt = reader["PublishedAt"] != DBNull.Value ? Convert.ToDateTime(reader["PublishedAt"]) : null
        };
    }
}
