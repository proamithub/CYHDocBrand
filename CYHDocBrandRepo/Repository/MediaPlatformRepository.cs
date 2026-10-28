using CYHDocBrandModel.Common;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class MediaPlatformRepository : IMediaPlatformRepository
{
    private readonly string _conn;
    private readonly ILogger<MediaPlatformRepository>? _logger;

    public MediaPlatformRepository(IConfiguration configuration, ILogger<MediaPlatformRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    public async Task<List<MediaPlatformDTO>> GetAllAsync()
    {
        var list = new List<MediaPlatformDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageMediaPlatforms_CYH", con);
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
            _logger?.LogError(ex, "Error getting all media platforms from SQL");
        }
        return list;
    }

    public async Task<List<MediaPlatformDTO>> GetActiveAsync()
    {
        var list = new List<MediaPlatformDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageMediaPlatforms_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_ACTIVE");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToDTO(reader));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting active media platforms from SQL");
        }
        return list;
    }

    public async Task<MediaPlatformDTO?> GetByIdAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageMediaPlatforms_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_ID");
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToDTO(reader);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting media platform by ID {Id}", id);
        }
        return null;
    }

    public async Task<bool> ToggleActiveAsync(int id, string? updatedBy)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageMediaPlatforms_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "TOGGLE_ACTIVE");
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@UpdatedBy", (object?)updatedBy ?? DBNull.Value);

            var res = await cmd.ExecuteScalarAsync();
            return res != null && res != DBNull.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling active status for media platform ID {Id}", id);
            return false;
        }
    }

    public async Task<bool> UpdateGlobalApiAsync(UpdateMediaPlatformApiRequest request)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageMediaPlatforms_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "UPDATE_GLOBAL_API");
            cmd.Parameters.AddWithValue("@Id", request.Id);
            cmd.Parameters.AddWithValue("@GlobalApiEndpoint", (object?)request.GlobalApiEndpoint ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalApiKey", (object?)request.GlobalApiKey ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalAppId", (object?)request.GlobalAppId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalAppSecret", (object?)request.GlobalAppSecret ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalAccessToken", (object?)request.GlobalAccessToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalPageOrAccountId", (object?)request.GlobalPageOrAccountId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalPhoneNumberId", (object?)request.GlobalPhoneNumberId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalRefreshToken", (object?)request.GlobalRefreshToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@GlobalTokenExpiresAt", (object?)request.GlobalTokenExpiresAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DefaultMessageTemplate", (object?)request.DefaultMessageTemplate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ApiNotes", (object?)request.ApiNotes ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedBy", (object?)request.UpdatedBy ?? DBNull.Value);

            var res = await cmd.ExecuteScalarAsync();
            return res != null && res != DBNull.Value;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating global API for media platform ID {Id}", request.Id);
            return false;
        }
    }

    private static MediaPlatformDTO MapReaderToDTO(SqlDataReader reader)
    {
        return new MediaPlatformDTO
        {
            Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
            PlatformCode = reader["PlatformCode"]?.ToString() ?? string.Empty,
            PlatformName = reader["PlatformName"]?.ToString() ?? string.Empty,
            Category = reader["Category"] != DBNull.Value ? reader["Category"].ToString()! : string.Empty,
            IconClass = reader["IconClass"] != DBNull.Value ? reader["IconClass"].ToString()! : "bi-share",
            ColorHex = reader["ColorHex"] != DBNull.Value ? reader["ColorHex"].ToString()! : "#ffffff",
            SortOrder = reader["SortOrder"] != DBNull.Value ? Convert.ToInt32(reader["SortOrder"]) : 0,
            IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
            AllowGlobalFallback = reader["AllowGlobalFallback"] != DBNull.Value && Convert.ToBoolean(reader["AllowGlobalFallback"]),
            RequiresDoctorAccount = reader["RequiresDoctorAccount"] != DBNull.Value && Convert.ToBoolean(reader["RequiresDoctorAccount"]),
            GlobalApiEndpoint = reader["GlobalApiEndpoint"] != DBNull.Value ? reader["GlobalApiEndpoint"].ToString() : null,
            GlobalApiKey = reader["GlobalApiKey"] != DBNull.Value ? reader["GlobalApiKey"].ToString() : null,
            GlobalAppId = reader["GlobalAppId"] != DBNull.Value ? reader["GlobalAppId"].ToString() : null,
            GlobalAppSecret = reader["GlobalAppSecret"] != DBNull.Value ? reader["GlobalAppSecret"].ToString() : null,
            GlobalAccessToken = reader["GlobalAccessToken"] != DBNull.Value ? reader["GlobalAccessToken"].ToString() : null,
            GlobalPageOrAccountId = reader["GlobalPageOrAccountId"] != DBNull.Value ? reader["GlobalPageOrAccountId"].ToString() : null,
            GlobalPhoneNumberId = reader["GlobalPhoneNumberId"] != DBNull.Value ? reader["GlobalPhoneNumberId"].ToString() : null,
            GlobalRefreshToken = reader["GlobalRefreshToken"] != DBNull.Value ? reader["GlobalRefreshToken"].ToString() : null,
            GlobalTokenExpiresAt = reader["GlobalTokenExpiresAt"] != DBNull.Value ? Convert.ToDateTime(reader["GlobalTokenExpiresAt"]) : null,
            DefaultMessageTemplate = reader["DefaultMessageTemplate"] != DBNull.Value ? reader["DefaultMessageTemplate"].ToString() : null,
            ApiNotes = reader["ApiNotes"] != DBNull.Value ? reader["ApiNotes"].ToString() : null,
            UpdatedAt = reader["UpdatedAt"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedAt"]) : IndianTimeHelper.Now,
            UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? reader["UpdatedBy"].ToString() : null
        };
    }
}
