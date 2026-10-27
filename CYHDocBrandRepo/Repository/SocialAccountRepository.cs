using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class SocialAccountRepository : ISocialAccountRepository
{
    private readonly string _conn;
    private readonly ILogger<SocialAccountRepository>? _logger;

    public SocialAccountRepository(IConfiguration configuration, ILogger<SocialAccountRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    public async Task<List<DoctorSocialAccountDTO>> GetByDoctorAsync(int? doctorId, string? userId)
    {
        var list = new List<DoctorSocialAccountDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorSocialAccounts_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_DOCTOR");
            cmd.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(ReadSocialAccount(reader));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching social accounts for DoctorId {DoctorId}, UserId {UserId}", doctorId, userId);
        }
        return list;
    }

    public async Task<DoctorSocialAccountDTO?> GetByIdAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorSocialAccounts_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_ID");
            cmd.Parameters.AddWithValue("@Id", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return ReadSocialAccount(reader);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error fetching social account by Id {Id}", id);
        }
        return null;
    }

    public async Task<int> SaveAccountAsync(SocialAccountSaveRequest request)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorSocialAccounts_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "SAVE_ACCOUNT");
            cmd.Parameters.AddWithValue("@Id", request.Id > 0 ? (object)request.Id : DBNull.Value);
            cmd.Parameters.AddWithValue("@DoctorId", (object?)request.DoctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)request.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DoctorName", (object?)request.DoctorName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Platform", request.Platform);
            cmd.Parameters.AddWithValue("@AccountName", request.AccountName);
            cmd.Parameters.AddWithValue("@PageOrAccountId", request.PageOrAccountId);
            cmd.Parameters.AddWithValue("@AccessToken", request.AccessToken);
            cmd.Parameters.AddWithValue("@RefreshToken", (object?)request.RefreshToken ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TokenExpiresAt", (object?)request.TokenExpiresAt ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", request.IsActive);

            var result = await cmd.ExecuteScalarAsync();
            if (result != null && int.TryParse(result.ToString(), out int newId))
            {
                return newId;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error saving social account for DoctorId {DoctorId}, Platform {Platform}", request.DoctorId, request.Platform);
        }
        return 0;
    }

    public async Task<bool> UpdateTestStatusAsync(int id, string status, string? message)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorSocialAccounts_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "UPDATE_TEST_STATUS");
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@LastTestStatus", status);
            cmd.Parameters.AddWithValue("@LastTestMessage", (object?)message ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating test status for account Id {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeleteAccountAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorSocialAccounts_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "DELETE");
            cmd.Parameters.AddWithValue("@Id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting social account Id {Id}", id);
            return false;
        }
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctorSocialAccounts_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "TOGGLE_ACTIVE");
            cmd.Parameters.AddWithValue("@Id", id);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error toggling social account active state for Id {Id}", id);
            return false;
        }
    }

    private static DoctorSocialAccountDTO ReadSocialAccount(SqlDataReader reader)
    {
        return new DoctorSocialAccountDTO
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            DoctorId = reader.IsDBNull(reader.GetOrdinal("DoctorId")) ? null : reader.GetInt32(reader.GetOrdinal("DoctorId")),
            UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
            DoctorName = reader.IsDBNull(reader.GetOrdinal("DoctorName")) ? null : reader.GetString(reader.GetOrdinal("DoctorName")),
            Platform = reader.GetString(reader.GetOrdinal("Platform")),
            AccountName = reader.GetString(reader.GetOrdinal("AccountName")),
            PageOrAccountId = reader.GetString(reader.GetOrdinal("PageOrAccountId")),
            AccessToken = reader.GetString(reader.GetOrdinal("AccessToken")),
            RefreshToken = reader.IsDBNull(reader.GetOrdinal("RefreshToken")) ? null : reader.GetString(reader.GetOrdinal("RefreshToken")),
            TokenExpiresAt = reader.IsDBNull(reader.GetOrdinal("TokenExpiresAt")) ? null : reader.GetDateTime(reader.GetOrdinal("TokenExpiresAt")),
            IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
            ConnectedAt = reader.GetDateTime(reader.GetOrdinal("ConnectedAt")),
            LastTestedAt = reader.IsDBNull(reader.GetOrdinal("LastTestedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("LastTestedAt")),
            LastTestStatus = reader.IsDBNull(reader.GetOrdinal("LastTestStatus")) ? null : reader.GetString(reader.GetOrdinal("LastTestStatus")),
            LastTestMessage = reader.IsDBNull(reader.GetOrdinal("LastTestMessage")) ? null : reader.GetString(reader.GetOrdinal("LastTestMessage"))
        };
    }
}
