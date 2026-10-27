using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class BannerScheduleRepository : IBannerScheduleRepository
{
    private readonly string _conn;
    private readonly ILogger<BannerScheduleRepository>? _logger;

    public BannerScheduleRepository(IConfiguration configuration, ILogger<BannerScheduleRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    public async Task<int> CreateScheduleAsync(CreateBannerScheduleRequest request, string? createdBy)
    {
        return await CreateScheduleAsync(request, null, createdBy);
    }

    public async Task<int> CreateScheduleAsync(CreateBannerScheduleRequest request, List<DoctorSocialAccountDTO>? targetAccounts, string? createdBy)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            var platformsStr = string.Join(",", request.SelectedPlatforms);

            // 1. Insert master schedule
            await using var cmd = new SqlCommand("ManageBannerSchedules_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "CREATE");
            cmd.Parameters.AddWithValue("@BannerDesignId", request.BannerDesignId);
            cmd.Parameters.AddWithValue("@DoctorId", (object?)request.DoctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)request.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@DoctorName", request.DoctorName);
            cmd.Parameters.AddWithValue("@HospitalName", (object?)request.HospitalName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@BannerHeading", (object?)request.BannerHeading ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ImageUrl", (object?)request.ImageUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Caption", (object?)request.Caption ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Platforms", platformsStr);
            cmd.Parameters.AddWithValue("@ScheduledAt", request.ScheduledAt);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)createdBy ?? DBNull.Value);

            var result = await cmd.ExecuteScalarAsync();
            if (result == null || !int.TryParse(result.ToString(), out int scheduleId))
            {
                return 0;
            }

            // 2. Insert schedule detail for each targeted platform
            foreach (var platform in request.SelectedPlatforms)
            {
                var account = targetAccounts?.FirstOrDefault(a => a.Platform.Equals(platform, StringComparison.OrdinalIgnoreCase));
                
                await using var cmdDetail = new SqlCommand("ManageBannerSchedules_CYH", con);
                cmdDetail.CommandType = CommandType.StoredProcedure;
                cmdDetail.Parameters.AddWithValue("@Flag", "ADD_DETAIL");
                cmdDetail.Parameters.AddWithValue("@Id", scheduleId);
                cmdDetail.Parameters.AddWithValue("@Platform", platform);
                cmdDetail.Parameters.AddWithValue("@TargetAccountName", (object?)account?.AccountName ?? DBNull.Value);
                cmdDetail.Parameters.AddWithValue("@TargetPageId", (object?)account?.PageOrAccountId ?? DBNull.Value);
                await cmdDetail.ExecuteNonQueryAsync();
            }

            return scheduleId;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error creating banner schedule for BannerId {BannerDesignId}", request.BannerDesignId);
            return 0;
        }
    }

    public async Task<List<BannerScheduleDTO>> GetDueSchedulesAsync()
    {
        var schedules = new List<BannerScheduleDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            // Direct SQL query ensures immediate execution and picks up schedules whether saved in UTC or Local time
            const string sql = @"
                DECLARE @DueList TABLE (Id INT);

                UPDATE TOP (10) [dbo].[BannerSchedules_CYH]
                SET Status = 'Processing',
                    LastExecutedAt = DATEADD(minute, 330, SYSUTCDATETIME()),
                    ExecutionAttempts = ExecutionAttempts + 1
                OUTPUT inserted.Id INTO @DueList
                WHERE Status = 'Pending' 
                  AND (ScheduledAt <= DATEADD(minute, 330, SYSUTCDATETIME()) OR ScheduledAt <= SYSUTCDATETIME() OR ScheduledAt <= GETDATE() OR ScheduledAt <= GETUTCDATE());

                SELECT s.Id, s.BannerDesignId, s.DoctorId, s.UserId, s.DoctorName, s.HospitalName,
                       s.BannerHeading, s.ImageUrl, s.Caption, s.Platforms, s.ScheduledAt,
                       s.Status, s.ExecutionAttempts, s.LastExecutedAt, s.ErrorMessage, s.CreatedAt, s.CreatedBy
                FROM [dbo].[BannerSchedules_CYH] s
                INNER JOIN @DueList d ON s.Id = d.Id;

                SELECT d.Id, d.BannerScheduleId, d.Platform, d.TargetAccountName, d.TargetPageId,
                       d.Status, d.PlatformPostId, d.PostUrl, d.ErrorMessage, d.PublishedAt
                FROM [dbo].[BannerScheduleDetails_CYH] d
                INNER JOIN @DueList dl ON d.BannerScheduleId = dl.Id;";

            await using var cmd = new SqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();
            
            // Result Set 1: Master Schedules
            while (await reader.ReadAsync())
            {
                schedules.Add(ReadSchedule(reader));
            }

            // Result Set 2: Details
            if (await reader.NextResultAsync())
            {
                var details = new List<BannerScheduleDetailDTO>();
                while (await reader.ReadAsync())
                {
                    details.Add(ReadScheduleDetail(reader));
                }

                foreach (var s in schedules)
                {
                    s.Details = details.Where(d => d.BannerScheduleId == s.Id).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting due schedules from database");
        }
        return schedules;
    }

    public async Task<bool> UpdateDetailStatusAsync(int detailId, string status, string? platformPostId, string? postUrl, string? errorMessage)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerSchedules_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "UPDATE_DETAIL_STATUS");
            cmd.Parameters.AddWithValue("@ScheduleDetailId", detailId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@PlatformPostId", (object?)platformPostId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PostUrl", (object?)postUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating detail status for detailId {DetailId}", detailId);
            return false;
        }
    }

    public async Task<bool> UpdateScheduleStatusAsync(int scheduleId, string status, string? errorMessage)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerSchedules_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "UPDATE_SCHEDULE_STATUS");
            cmd.Parameters.AddWithValue("@Id", scheduleId);
            cmd.Parameters.AddWithValue("@Status", status);
            cmd.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error updating schedule status for scheduleId {ScheduleId}", scheduleId);
            return false;
        }
    }

    public async Task<List<BannerScheduleDTO>> GetByDoctorAsync(int? doctorId, string? userId)
    {
        var schedules = new List<BannerScheduleDTO>();
        if ((!doctorId.HasValue || doctorId.Value <= 0) && string.IsNullOrWhiteSpace(userId))
        {
            return schedules;
        }

        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            const string sql = @"
                SELECT s.Id, s.BannerDesignId, s.DoctorId, s.UserId, s.DoctorName, s.HospitalName,
                       s.BannerHeading, s.ImageUrl, s.Caption, s.Platforms, s.ScheduledAt,
                       s.Status, s.ExecutionAttempts, s.LastExecutedAt, s.ErrorMessage, s.CreatedAt, s.CreatedBy
                FROM [dbo].[BannerSchedules_CYH] s
                WHERE (@DoctorId IS NOT NULL AND s.DoctorId = @DoctorId)
                   OR (@UserId IS NOT NULL AND s.UserId = @UserId)
                ORDER BY s.ScheduledAt DESC, s.Id DESC;

                SELECT d.Id, d.BannerScheduleId, d.Platform, d.TargetAccountName, d.TargetPageId,
                       d.Status, d.PlatformPostId, d.PostUrl, d.ErrorMessage, d.PublishedAt
                FROM [dbo].[BannerScheduleDetails_CYH] d
                INNER JOIN [dbo].[BannerSchedules_CYH] s ON d.BannerScheduleId = s.Id
                WHERE (@DoctorId IS NOT NULL AND s.DoctorId = @DoctorId)
                   OR (@UserId IS NOT NULL AND s.UserId = @UserId)
                ORDER BY d.Id ASC;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                schedules.Add(ReadSchedule(reader));
            }

            if (await reader.NextResultAsync())
            {
                var details = new List<BannerScheduleDetailDTO>();
                while (await reader.ReadAsync())
                {
                    details.Add(ReadScheduleDetail(reader));
                }

                foreach (var s in schedules)
                {
                    s.Details = details.Where(d => d.BannerScheduleId == s.Id).ToList();
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting schedules for doctorId {DoctorId}, userId {UserId}", doctorId, userId);
        }
        return schedules;
    }

    public async Task<AdminScheduleSummaryDTO> GetAllAdminAsync(int? doctorId = null, string? status = null, string? search = null)
    {
        var summary = new AdminScheduleSummaryDTO();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerSchedules_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_ALL_ADMIN");
            cmd.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Search", (object?)search ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                summary.Schedules.Add(ReadSchedule(reader));
            }

            if (await reader.NextResultAsync())
            {
                var details = new List<BannerScheduleDetailDTO>();
                while (await reader.ReadAsync())
                {
                    details.Add(ReadScheduleDetail(reader));
                }

                foreach (var s in summary.Schedules)
                {
                    s.Details = details.Where(d => d.BannerScheduleId == s.Id).ToList();
                }
            }

            summary.TotalScheduled = summary.Schedules.Count;
            summary.PendingCount = summary.Schedules.Count(s => s.Status == "Pending");
            summary.ProcessingCount = summary.Schedules.Count(s => s.Status == "Processing");
            summary.SuccessCount = summary.Schedules.Count(s => s.Status == "Success");
            summary.FailedCount = summary.Schedules.Count(s => s.Status == "Failed");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting all schedules for admin");
        }
        return summary;
    }

    public async Task<bool> CancelScheduleAsync(int scheduleId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            const string sql = @"
                UPDATE [dbo].[BannerSchedules_CYH]
                SET Status = 'Cancelled',
                    ErrorMessage = 'Cancelled by user'
                WHERE Id = @Id;

                UPDATE [dbo].[BannerScheduleDetails_CYH]
                SET Status = 'Cancelled'
                WHERE BannerScheduleId = @Id;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", scheduleId);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error cancelling schedule Id {ScheduleId}", scheduleId);
            return false;
        }
    }

    public async Task<bool> SnoozeScheduleAsync(int scheduleId, int minutes = 5)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            const string sql = @"
                UPDATE [dbo].[BannerSchedules_CYH]
                SET Status = 'Pending',
                    ScheduledAt = DATEADD(minute, @Minutes, DATEADD(minute, 330, SYSUTCDATETIME())),
                    ErrorMessage = NULL
                WHERE Id = @Id;

                UPDATE [dbo].[BannerScheduleDetails_CYH]
                SET Status = 'Pending',
                    ErrorMessage = NULL
                WHERE BannerScheduleId = @Id;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", scheduleId);
            cmd.Parameters.AddWithValue("@Minutes", minutes);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error snoozing schedule Id {ScheduleId}", scheduleId);
            return false;
        }
    }

    public async Task<bool> RetryScheduleAsync(int scheduleId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageBannerSchedules_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "RETRY_SCHEDULE");
            cmd.Parameters.AddWithValue("@Id", scheduleId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error retrying schedule Id {ScheduleId}", scheduleId);
            return false;
        }
    }

    public async Task<bool> DeleteScheduleAsync(int scheduleId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            // Direct SQL deletion deletes child platforms first, then master schedule, ensuring 100% reliable deletion regardless of SP state
            const string sql = @"
                DELETE FROM [dbo].[BannerScheduleDetails_CYH] WHERE BannerScheduleId = @Id;
                DELETE FROM [dbo].[BannerSchedules_CYH] WHERE Id = @Id;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Id", scheduleId);

            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error deleting schedule Id {ScheduleId}", scheduleId);
            return false;
        }
    }

    private static BannerScheduleDTO ReadSchedule(SqlDataReader reader)
    {
        return new BannerScheduleDTO
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            BannerDesignId = reader.GetInt32(reader.GetOrdinal("BannerDesignId")),
            DoctorId = reader.IsDBNull(reader.GetOrdinal("DoctorId")) ? null : reader.GetInt32(reader.GetOrdinal("DoctorId")),
            UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? null : reader.GetString(reader.GetOrdinal("UserId")),
            DoctorName = reader.GetString(reader.GetOrdinal("DoctorName")),
            HospitalName = reader.IsDBNull(reader.GetOrdinal("HospitalName")) ? null : reader.GetString(reader.GetOrdinal("HospitalName")),
            BannerHeading = reader.IsDBNull(reader.GetOrdinal("BannerHeading")) ? null : reader.GetString(reader.GetOrdinal("BannerHeading")),
            ImageUrl = reader.IsDBNull(reader.GetOrdinal("ImageUrl")) ? null : reader.GetString(reader.GetOrdinal("ImageUrl")),
            Caption = reader.IsDBNull(reader.GetOrdinal("Caption")) ? null : reader.GetString(reader.GetOrdinal("Caption")),
            Platforms = reader.GetString(reader.GetOrdinal("Platforms")),
            ScheduledAt = reader.GetDateTime(reader.GetOrdinal("ScheduledAt")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            ExecutionAttempts = reader.GetInt32(reader.GetOrdinal("ExecutionAttempts")),
            LastExecutedAt = reader.IsDBNull(reader.GetOrdinal("LastExecutedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("LastExecutedAt")),
            ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString(reader.GetOrdinal("ErrorMessage")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
            CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetString(reader.GetOrdinal("CreatedBy"))
        };
    }

    private static BannerScheduleDetailDTO ReadScheduleDetail(SqlDataReader reader)
    {
        return new BannerScheduleDetailDTO
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            BannerScheduleId = reader.GetInt32(reader.GetOrdinal("BannerScheduleId")),
            Platform = reader.GetString(reader.GetOrdinal("Platform")),
            TargetAccountName = reader.IsDBNull(reader.GetOrdinal("TargetAccountName")) ? null : reader.GetString(reader.GetOrdinal("TargetAccountName")),
            TargetPageId = reader.IsDBNull(reader.GetOrdinal("TargetPageId")) ? null : reader.GetString(reader.GetOrdinal("TargetPageId")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            PlatformPostId = reader.IsDBNull(reader.GetOrdinal("PlatformPostId")) ? null : reader.GetString(reader.GetOrdinal("PlatformPostId")),
            PostUrl = reader.IsDBNull(reader.GetOrdinal("PostUrl")) ? null : reader.GetString(reader.GetOrdinal("PostUrl")),
            ErrorMessage = reader.IsDBNull(reader.GetOrdinal("ErrorMessage")) ? null : reader.GetString(reader.GetOrdinal("ErrorMessage")),
            PublishedAt = reader.IsDBNull(reader.GetOrdinal("PublishedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("PublishedAt"))
        };
    }
}
