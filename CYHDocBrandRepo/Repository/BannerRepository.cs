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
            ?? "Server=VRS-AMITPC\\SQLEXPRESS;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
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
            cmd.Parameters.AddWithValue("@DoctorName", state.DoctorName ?? string.Empty);
            cmd.Parameters.AddWithValue("@HospitalName", state.HospitalName ?? string.Empty);
            cmd.Parameters.AddWithValue("@TemplateType", state.TemplateType ?? "Doctor");
            cmd.Parameters.AddWithValue("@Heading", state.Heading ?? "Specialist Consultation");
            cmd.Parameters.AddWithValue("@Subtitle", (object?)state.Subtitle ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", string.IsNullOrEmpty(state.Status) ? "Draft" : state.Status);
            cmd.Parameters.AddWithValue("@AspectRatio", state.AspectRatio ?? "1:1");
            cmd.Parameters.AddWithValue("@PrimaryColor", state.PrimaryColor ?? "#00f2fe");
            cmd.Parameters.AddWithValue("@BackgroundImageUrl", (object?)state.BackgroundImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ConfigJson", json);

            var result = await cmd.ExecuteScalarAsync();
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving banner draft to database via ManageBannerDesigns_CYH");
            return 0;
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
            return false;
        }
    }

    public async Task<bool> DeleteBannerAsync(int id, string? userId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

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
            return false;
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
            DoctorName = reader["DoctorName"].ToString() ?? string.Empty,
            HospitalName = reader["HospitalName"] != DBNull.Value ? reader["HospitalName"].ToString() ?? string.Empty : string.Empty,
            TemplateType = reader["TemplateType"].ToString() ?? "Doctor",
            Heading = reader["Heading"].ToString() ?? string.Empty,
            Subtitle = reader["Subtitle"] != DBNull.Value ? reader["Subtitle"].ToString() : null,
            Status = reader["Status"].ToString() ?? "Draft",
            AspectRatio = reader["AspectRatio"].ToString() ?? "1:1",
            PrimaryColor = reader["PrimaryColor"].ToString() ?? "#00f2fe",
            BackgroundImageUrl = reader["BackgroundImageUrl"] != DBNull.Value ? reader["BackgroundImageUrl"].ToString() : null,
            ConfigJson = configJson,
            CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
            UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"]),
            PublishedAt = reader["PublishedAt"] != DBNull.Value ? Convert.ToDateTime(reader["PublishedAt"]) : null
        };
    }
}
