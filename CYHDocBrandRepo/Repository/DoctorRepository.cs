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
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    // ── 1. Authenticate via Stored Procedure AuthenticateDoctor_CYH ─────────────
    public async Task<CurrentUserDTO?> AuthenticateAsync(string email, string password)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            var encodedPwd = AESEncryption.Encode(password);

            await using var cmd = new SqlCommand("AuthenticateDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "LOGIN");
            cmd.Parameters.AddWithValue("@Email", email.Trim());
            cmd.Parameters.AddWithValue("@Password", encodedPwd);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var avatarCol = -1;
                try { avatarCol = reader.GetOrdinal("AvatarUrl"); } catch { }
                var activeCol = -1;
                try { activeCol = reader.GetOrdinal("IsActive"); } catch { }

                return new CurrentUserDTO
                {
                    UserId = reader["UserId"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? email,
                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                    Role = reader["Role"]?.ToString() ?? "Doctor",
                    Department = reader["Department"] != DBNull.Value ? reader["Department"]?.ToString() : null,
                    DoctorProfileId = reader["DoctorProfileId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorProfileId"]) : null,
                    AvatarUrl = avatarCol >= 0 && !reader.IsDBNull(avatarCol) ? reader.GetString(avatarCol) : null,
                    IsActive = activeCol >= 0 && !reader.IsDBNull(activeCol) ? Convert.ToBoolean(reader.GetValue(activeCol)) : true
                };
            }

            return null;
        }
                catch (Exception ex)
        {
            _logger?.LogError(ex, "AuthenticateAsync failed via AuthenticateDoctor_CYH for {Email}", email);
            return null;
        }
    }

    public async Task<CurrentUserDTO?> GetByPhoneAsync(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return null;

        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            var cleanPhone = new string(phoneNumber.Where(char.IsDigit).ToArray());
            if (cleanPhone.Length > 10) cleanPhone = cleanPhone.Substring(cleanPhone.Length - 10);

            await using var cmd = new SqlCommand("AuthenticateDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_BY_PHONE");
            cmd.Parameters.AddWithValue("@PhoneNumber", cleanPhone);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var avatarCol = -1;
                try { avatarCol = reader.GetOrdinal("AvatarUrl"); } catch { }
                var activeCol = -1;
                try { activeCol = reader.GetOrdinal("IsActive"); } catch { }

                return new CurrentUserDTO
                {
                    UserId = reader["UserId"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? string.Empty,
                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                    Role = reader["Role"]?.ToString() ?? "Doctor",
                    Department = reader["Department"] != DBNull.Value ? reader["Department"]?.ToString() : null,
                    DoctorProfileId = reader["DoctorProfileId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorProfileId"]) : null,
                    AvatarUrl = avatarCol >= 0 && !reader.IsDBNull(avatarCol) ? reader.GetString(avatarCol) : null,
                    IsActive = activeCol >= 0 && !reader.IsDBNull(activeCol) ? Convert.ToBoolean(reader.GetValue(activeCol)) : true
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetByPhoneAsync failed via AuthenticateDoctor_CYH for {Phone}", phoneNumber);
            return null;
        }
    }

    public async Task<CurrentUserDTO?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return null;

        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("SELECT TOP 1 u.Id AS UserId, u.Email, u.FullName, u.Role, u.Department, u.IsActive, u.AvatarUrl, d.Id AS DoctorProfileId FROM AspNetUsers u LEFT JOIN DoctorProfiles d ON u.Id = d.UserId WHERE u.Email = @Email OR u.UserName = @Email", con);
            cmd.Parameters.AddWithValue("@Email", email.Trim());

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var avatarCol = -1;
                try { avatarCol = reader.GetOrdinal("AvatarUrl"); } catch { }
                var activeCol = -1;
                try { activeCol = reader.GetOrdinal("IsActive"); } catch { }

                return new CurrentUserDTO
                {
                    UserId = reader["UserId"]?.ToString() ?? string.Empty,
                    Email = reader["Email"]?.ToString() ?? email,
                    FullName = reader["FullName"]?.ToString() ?? string.Empty,
                    Role = reader["Role"]?.ToString() ?? "Doctor",
                    Department = reader["Department"] != DBNull.Value ? reader["Department"]?.ToString() : null,
                    DoctorProfileId = reader["DoctorProfileId"] != DBNull.Value ? Convert.ToInt32(reader["DoctorProfileId"]) : null,
                    AvatarUrl = avatarCol >= 0 && !reader.IsDBNull(avatarCol) ? reader.GetString(avatarCol) : null,
                    IsActive = activeCol >= 0 && !reader.IsDBNull(activeCol) ? Convert.ToBoolean(reader.GetValue(activeCol)) : true
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetByEmailAsync failed for {Email}", email);
            return null;
        }
    }

    // ── 2. ManageDoctor_CYH Stored Procedure Operations ──────────────────────────

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
            _logger?.LogError(ex, "GetByIdAsync failed via ManageDoctor_CYH for {DoctorId}", doctorId);
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
            _logger?.LogError(ex, "GetByUserIdAsync failed via ManageDoctor_CYH for {UserId}", userId);
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

            var scalar = await cmd.ExecuteScalarAsync();
            if (scalar != null && scalar != DBNull.Value)
            {
                return Convert.ToInt32(scalar) > 0;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ToggleAvailabilityAsync failed via ManageDoctor_CYH for {DoctorId}", doctorId);
            return false;
        }
    }

    public async Task<bool> ToggleDoctorActiveStatusAsync(string userId, bool isActive)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "SET_ACTIVE_STATUS");
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@IsActive", isActive);

            var scalar = await cmd.ExecuteScalarAsync();
            if (scalar != null && scalar != DBNull.Value)
            {
                return Convert.ToInt32(scalar) > 0;
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ToggleDoctorActiveStatusAsync failed via ManageDoctor_CYH for {UserId}", userId);
            return false;
        }
    }

    public async Task<(bool Success, string? ErrorMessage)> RegisterDoctorAsync(DoctorRegistrationDTO dto)
    {
        if (dto == null) return (false, "Registration data is required.");

        var rawPhone = dto.PhoneNumber?.Trim() ?? string.Empty;
        var cleanPhone = new string(rawPhone.Where(char.IsDigit).ToArray());
        if (cleanPhone.Length == 12 && cleanPhone.StartsWith("91")) cleanPhone = cleanPhone.Substring(2);
        if (cleanPhone.Length == 11 && cleanPhone.StartsWith("0")) cleanPhone = cleanPhone.Substring(1);

        if (cleanPhone.Length != 10)
        {
            return (false, "Mobile number must be exactly 10 digits.");
        }

        if (!"6789".Contains(cleanPhone[0]))
        {
            return (false, "Mobile number must start with 6, 7, 8, or 9.");
        }

        // 1. Check for duplicate Email
        var existingEmail = await GetByEmailAsync(dto.Email.Trim());
        if (existingEmail != null)
        {
            return (false, "An account with this email address already exists. Please sign in or use another email.");
        }

        // 2. Check for duplicate Mobile Number
        var existingPhone = await GetByPhoneAsync(cleanPhone);
        if (existingPhone != null)
        {
            return (false, "An account with this mobile number already exists. Please sign in with OTP or use another number.");
        }

        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            var encodedPassword = AESEncryption.Encode(dto.Password);

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "REGISTER_DOCTOR");
            cmd.Parameters.AddWithValue("@FullName", dto.FullName.Trim());
            cmd.Parameters.AddWithValue("@Email", dto.Email.Trim());
            cmd.Parameters.AddWithValue("@PhoneNumber", cleanPhone);
            cmd.Parameters.AddWithValue("@Specialization", dto.Specialization.Trim());
            cmd.Parameters.AddWithValue("@LicenseNumber", dto.LicenseNumber.Trim());
            cmd.Parameters.AddWithValue("@ExperienceYears", dto.ExperienceYears);
            cmd.Parameters.AddWithValue("@ConsultationFee", dto.ConsultationFee);
            cmd.Parameters.AddWithValue("@AvailableDays", string.IsNullOrWhiteSpace(dto.AvailableDays) ? "Mon - Sat" : dto.AvailableDays.Trim());
            cmd.Parameters.AddWithValue("@AvailableHours", string.IsNullOrWhiteSpace(dto.AvailableHours) ? "09:00 AM - 05:00 PM" : dto.AvailableHours.Trim());
            cmd.Parameters.AddWithValue("@Bio", (object?)dto.Bio?.Trim() ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Password", encodedPassword);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var statusCode = Convert.ToInt32(reader["StatusCode"]);
                var message = reader["Message"]?.ToString();
                return (statusCode == 1, statusCode == 1 ? null : message);
            }

            return (false, "No response returned from registration procedure.");
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "RegisterDoctorAsync failed via ManageDoctor_CYH for {Email}", dto.Email);
            return (false, "Registration failed: " + ex.Message);
        }
    }

    public async Task<bool> UpdateProfileAsync(DoctorProfile profile)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageDoctor_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "UPDATE_PROFILE");
            cmd.Parameters.AddWithValue("@DoctorId", profile.Id);
            cmd.Parameters.AddWithValue("@UserId", (object?)profile.UserId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FullName", profile.FullName ?? string.Empty);
            cmd.Parameters.AddWithValue("@DisplayName", (object?)profile.DisplayName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Specialization", profile.Specialization ?? "General Medicine");
            cmd.Parameters.AddWithValue("@Qualifications", (object?)profile.Qualifications ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@LicenseNumber", profile.LicenseNumber ?? string.Empty);
            cmd.Parameters.AddWithValue("@ExperienceYears", profile.ExperienceYears);
            cmd.Parameters.AddWithValue("@ConsultationFee", profile.ConsultationFee);
            cmd.Parameters.AddWithValue("@AvailableDays", profile.AvailableDays ?? "Mon - Sat");
            cmd.Parameters.AddWithValue("@AvailableHours", profile.AvailableHours ?? "09:00 AM - 05:00 PM");
            cmd.Parameters.AddWithValue("@Bio", (object?)profile.Bio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePic", (object?)profile.ProfilePic ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@MobileNo", (object?)profile.MobileNo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClinicName", (object?)profile.ClinicName ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClinicAddress", (object?)profile.ClinicAddress ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@City", (object?)profile.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@State", (object?)profile.State ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PinCode", (object?)profile.PinCode ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WebsiteUrl", (object?)profile.WebsiteUrl ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ClinicLogo", (object?)profile.ClinicLogo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrimaryColorHex", (object?)profile.PrimaryColorHex ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@SecondaryColorHex", (object?)profile.SecondaryColorHex ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PreferredFont", (object?)profile.PreferredFont ?? DBNull.Value);

            var scalar = await cmd.ExecuteScalarAsync();
            if (scalar != null && scalar != DBNull.Value)
            {
                var rows = Convert.ToInt32(scalar);
                return rows > 0;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "UpdateProfileAsync failed via ManageDoctor_CYH for DoctorId {Id}", profile.Id);
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
            var picCol = reader.GetOrdinal("ProfilePic");
            if (!reader.IsDBNull(picCol))
            {
                doc.ProfilePic = reader.GetString(picCol);
            }
        }
        catch { }

        try
        {
            var userCol = reader.GetOrdinal("UserName");
            if (!reader.IsDBNull(userCol))
            {
                var avatarCol = -1;
                try { avatarCol = reader.GetOrdinal("AvatarUrl"); } catch { }
                var pwdCol = -1;
                try { pwdCol = reader.GetOrdinal("Password"); } catch { }

                doc.User = new ApplicationUser
                {
                    Id = doc.UserId,
                    UserName = reader.GetString(userCol),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
                    PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                    FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? string.Empty : reader.GetString(reader.GetOrdinal("FullName")),
                    Role = reader.IsDBNull(reader.GetOrdinal("Role")) ? "Doctor" : reader.GetString(reader.GetOrdinal("Role")),
                    Department = reader.IsDBNull(reader.GetOrdinal("Department")) ? null : reader.GetString(reader.GetOrdinal("Department")),
                    IsActive = !reader.IsDBNull(reader.GetOrdinal("IsActive")) && reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    AvatarUrl = avatarCol >= 0 && !reader.IsDBNull(avatarCol) ? reader.GetString(avatarCol) : null,
                    Password = pwdCol >= 0 && !reader.IsDBNull(pwdCol) ? reader.GetString(pwdCol) : null
                };

                if (string.IsNullOrEmpty(doc.ProfilePic) && !string.IsNullOrEmpty(doc.User.AvatarUrl))
                {
                    doc.ProfilePic = doc.User.AvatarUrl;
                }
            }
        }
        catch
        {
            // Optional projection column
        }

        try
        {
            var col = -1;
            try { col = reader.GetOrdinal("DisplayName"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.DisplayName = reader.GetString(col);
            if (string.IsNullOrWhiteSpace(doc.DisplayName)) doc.DisplayName = doc.FullName;

            try { col = reader.GetOrdinal("Qualifications"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.Qualifications = reader.GetString(col);

            try { col = reader.GetOrdinal("MobileNo"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.MobileNo = reader.GetString(col);

            try { col = reader.GetOrdinal("ClinicName"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.ClinicName = reader.GetString(col);

            try { col = reader.GetOrdinal("ClinicAddress"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.ClinicAddress = reader.GetString(col);

            try { col = reader.GetOrdinal("City"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.City = reader.GetString(col);

            try { col = reader.GetOrdinal("State"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.State = reader.GetString(col);

            try { col = reader.GetOrdinal("PinCode"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.PinCode = reader.GetString(col);

            try { col = reader.GetOrdinal("WebsiteUrl"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.WebsiteUrl = reader.GetString(col);

            try { col = reader.GetOrdinal("ClinicLogo"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.ClinicLogo = reader.GetString(col);

            try { col = reader.GetOrdinal("PrimaryColorHex"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.PrimaryColorHex = reader.GetString(col);

            try { col = reader.GetOrdinal("SecondaryColorHex"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.SecondaryColorHex = reader.GetString(col);

            try { col = reader.GetOrdinal("PreferredFont"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.PreferredFont = reader.GetString(col);

            try { col = reader.GetOrdinal("AiCredits"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.AiCredits = Convert.ToInt32(reader.GetValue(col));

            try { col = reader.GetOrdinal("IsVerified"); } catch { }
            if (col >= 0 && !reader.IsDBNull(col)) doc.IsVerified = Convert.ToBoolean(reader.GetValue(col));
        }
        catch { }

        return doc;
    }
}
