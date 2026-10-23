using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandData.Entities;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;
using CYHDocBrandRepo.Utilities;

namespace CYHDocBrandRepo.Repository;

public class DoctorRepository : IDoctorRepository
{
    private readonly string _conn;
    private readonly ILogger<DoctorRepository>? _logger;

    public DoctorRepository(IConfiguration configuration, ILogger<DoctorRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=VRS-AMITPC\\SQLEXPRESS;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    // ── Auth via Stored Procedure AuthenticateDoctor_CYH (Pure ADO.NET) ───────────
    public async Task<CurrentUserDTO?> AuthenticateAsync(string email, string password)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("AuthenticateDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "LOGIN");
            cmd.Parameters.AddWithValue("@Email", email.Trim());
            cmd.Parameters.AddWithValue("@Password", AESEncryption.Encode(password));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CurrentUserDTO
                {
                    UserId = reader["UserId"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? email,
                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                    Role = reader["Role"]?.ToString() ?? "Doctor",
                    Department = reader["Department"] != DBNull.Value ? reader["Department"]?.ToString() : null,
                    DoctorProfileId = reader["DoctorProfileId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorProfileId"]) : null
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "AuthenticateAsync stored procedure failed for {Email}", email);
            return null;
        }
    }

    // ── ManageDoctor_CYH Stored Procedure (Pure ADO.NET) ──────────────────────────
    public async Task<List<DoctorProfile>> GetAllWithUsersAsync()
    {
        var list = new List<DoctorProfile>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_ALL");

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapDoctor(reader));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetAllWithUsersAsync failed via ManageDoctor_CYH");
        }
        return list;
    }

    public async Task<DoctorProfile?> GetByIdAsync(int doctorId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_ID");
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapDoctor(reader);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetByIdAsync failed for {DoctorId}", doctorId);
        }
        return null;
    }

    public async Task<DoctorProfile?> GetByUserIdAsync(string userId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_USERID");
            cmd.Parameters.AddWithValue("@UserId", userId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapDoctor(reader);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetByUserIdAsync failed for {UserId}", userId);
        }
        return null;
    }

    public async Task<bool> ToggleAvailabilityAsync(int doctorId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "TOGGLE_DUTY");
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ToggleAvailabilityAsync failed for {DoctorId}", doctorId);
            return false;
        }
    }

    public async Task<bool> UpdateProfileAsync(DoctorProfile profile)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand(@"
                UPDATE DoctorProfiles 
                SET FullName = @FullName, 
                    Specialization = @Specialization, 
                    LicenseNumber = @LicenseNumber, 
                    ExperienceYears = @ExperienceYears, 
                    ConsultationFee = @ConsultationFee, 
                    AvailableDays = @AvailableDays, 
                    AvailableHours = @AvailableHours, 
                    Bio = @Bio
                WHERE Id = @Id", con);

            cmd.Parameters.AddWithValue("@Id", profile.Id);
            cmd.Parameters.AddWithValue("@FullName", profile.FullName);
            cmd.Parameters.AddWithValue("@Specialization", profile.Specialization);
            cmd.Parameters.AddWithValue("@LicenseNumber", profile.LicenseNumber ?? string.Empty);
            cmd.Parameters.AddWithValue("@ExperienceYears", profile.ExperienceYears);
            cmd.Parameters.AddWithValue("@ConsultationFee", profile.ConsultationFee);
            cmd.Parameters.AddWithValue("@AvailableDays", profile.AvailableDays ?? string.Empty);
            cmd.Parameters.AddWithValue("@AvailableHours", profile.AvailableHours ?? string.Empty);
            cmd.Parameters.AddWithValue("@Bio", (object?)profile.Bio ?? DBNull.Value);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UpdateProfileAsync failed for DoctorId {Id}", profile.Id);
            return false;
        }
    }

    private static DoctorProfile MapDoctor(SqlDataReader reader)
    {
        var doc = new DoctorProfile
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            UserId = reader.GetString(reader.GetOrdinal("UserId")),
            FullName = reader.GetString(reader.GetOrdinal("FullName")),
            Specialization = reader.GetString(reader.GetOrdinal("Specialization")),
            LicenseNumber = reader.IsDBNull(reader.GetOrdinal("LicenseNumber")) ? string.Empty : reader.GetString(reader.GetOrdinal("LicenseNumber")),
            ExperienceYears = reader.IsDBNull(reader.GetOrdinal("ExperienceYears")) ? 0 : reader.GetInt32(reader.GetOrdinal("ExperienceYears")),
            ConsultationFee = reader.IsDBNull(reader.GetOrdinal("ConsultationFee")) ? 0m : reader.GetDecimal(reader.GetOrdinal("ConsultationFee")),
            AvailableDays = reader.IsDBNull(reader.GetOrdinal("AvailableDays")) ? string.Empty : reader.GetString(reader.GetOrdinal("AvailableDays")),
            AvailableHours = reader.IsDBNull(reader.GetOrdinal("AvailableHours")) ? string.Empty : reader.GetString(reader.GetOrdinal("AvailableHours")),
            Bio = reader.IsDBNull(reader.GetOrdinal("Bio")) ? null : reader.GetString(reader.GetOrdinal("Bio")),
            IsAvailableToday = !reader.IsDBNull(reader.GetOrdinal("IsAvailableToday")) && reader.GetBoolean(reader.GetOrdinal("IsAvailableToday")),
            CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
        };

        try
        {
            var userCol = reader.GetOrdinal("UserName");
            if (!reader.IsDBNull(userCol))
            {
                doc.User = new ApplicationUser
                {
                    Id = doc.UserId,
                    UserName = reader.GetString(userCol),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? string.Empty : reader.GetString(reader.GetOrdinal("FullName")),
                    Role = reader.IsDBNull(reader.GetOrdinal("Role")) ? "Doctor" : reader.GetString(reader.GetOrdinal("Role")),
                    Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString(reader.GetOrdinal("Department")),
                    IsActive = !reader.IsDBNull(reader.GetOrdinal("IsActive")) && reader.GetBoolean(reader.GetOrdinal("IsActive"))
                };
            }
        }
        catch
        {
            // Optional projection column
        }

        return doc;
    }
}
