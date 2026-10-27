using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class PushNotificationRepository : IPushNotificationRepository
{
    private readonly string _conn;

    public PushNotificationRepository(IConfiguration config)
    {
        _conn = config.GetConnectionString("DefaultConnection") 
            ?? "Data Source=208.91.198.196; Initial Catalog=CYHDocBrand; User Id=CYHDocBrandSA; Password=CYHDocBrand@PassAK; pooling=true;TrustServerCertificate=True";
    }

    public async Task<bool> SaveSubscriptionAsync(PushSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Endpoint) || string.IsNullOrWhiteSpace(request.P256dh) || string.IsNullOrWhiteSpace(request.Auth))
        {
            return false;
        }

        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            const string sql = @"
                IF EXISTS (SELECT 1 FROM [dbo].[DoctorPushSubscriptions_CYH] WHERE [Endpoint] = @Endpoint)
                BEGIN
                    UPDATE [dbo].[DoctorPushSubscriptions_CYH]
                    SET [DoctorId] = ISNULL(@DoctorId, [DoctorId]),
                        [UserId] = ISNULL(@UserId, [UserId]),
                        [P256dh] = @P256dh,
                        [Auth] = @Auth,
                        [UserAgent] = @UserAgent,
                        [LastActiveAt] = DATEADD(minute, 330, SYSUTCDATETIME())
                    WHERE [Endpoint] = @Endpoint;
                END
                ELSE
                BEGIN
                    INSERT INTO [dbo].[DoctorPushSubscriptions_CYH] 
                        ([DoctorId], [UserId], [Endpoint], [P256dh], [Auth], [UserAgent], [CreatedAt], [LastActiveAt])
                    VALUES 
                        (@DoctorId, @UserId, @Endpoint, @P256dh, @Auth, @UserAgent, 
                         DATEADD(minute, 330, SYSUTCDATETIME()), DATEADD(minute, 330, SYSUTCDATETIME()));
                END";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@DoctorId", (object?)request.DoctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)request.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Endpoint", request.Endpoint);
            cmd.Parameters.AddWithValue("@P256dh", request.P256dh);
            cmd.Parameters.AddWithValue("@Auth", request.Auth);
            cmd.Parameters.AddWithValue("@UserAgent", (object?)request.UserAgent ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<DoctorPushSubscriptionDTO>> GetSubscriptionsByDoctorAsync(int? doctorId, string? userId)
    {
        var list = new List<DoctorPushSubscriptionDTO>();
        if ((!doctorId.HasValue || doctorId.Value <= 0) && string.IsNullOrWhiteSpace(userId))
        {
            return list;
        }

        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            const string sql = @"
                SELECT [Id], [DoctorId], [UserId], [Endpoint], [P256dh], [Auth], [UserAgent], [CreatedAt], [LastActiveAt]
                FROM [dbo].[DoctorPushSubscriptions_CYH]
                WHERE (@DoctorId IS NOT NULL AND [DoctorId] = @DoctorId)
                   OR (@UserId IS NOT NULL AND [UserId] = @UserId)
                ORDER BY [LastActiveAt] DESC;";

            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DoctorPushSubscriptionDTO
                {
                    Id = reader.GetInt32(0),
                    DoctorId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    UserId = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Endpoint = reader.GetString(3),
                    P256dh = reader.GetString(4),
                    Auth = reader.GetString(5),
                    UserAgent = reader.IsDBNull(6) ? null : reader.GetString(6),
                    CreatedAt = reader.GetDateTime(7),
                    LastActiveAt = reader.GetDateTime(8)
                });
            }
        }
        catch
        {
            // Fail safely
        }

        return list;
    }

    public async Task<bool> DeleteSubscriptionAsync(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint)) return false;

        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            const string sql = "DELETE FROM [dbo].[DoctorPushSubscriptions_CYH] WHERE [Endpoint] = @Endpoint;";
            await using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Endpoint", endpoint);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch
        {
            return false;
        }
    }
}
