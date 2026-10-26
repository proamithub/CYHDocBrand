using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandRepo.Repository;

public class CreditRepository : ICreditRepository
{
    private readonly string _conn;
    private readonly ILogger<CreditRepository>? _logger;

    public CreditRepository(IConfiguration configuration, ILogger<CreditRepository>? logger = null)
    {
        _conn = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=AMITLAP;Database=CYHDocBrandBD;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        _logger = logger;
    }

    public async Task<List<CreditTypeDTO>> GetAllCreditTypesAsync(bool onlyActive = false)
    {
        var list = new List<CreditTypeDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_TYPES");
            cmd.Parameters.AddWithValue("@OnlyActive", onlyActive);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new CreditTypeDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    ServiceType = reader.IsDBNull(reader.GetOrdinal("ServiceType")) ? "Free" : reader.GetString(reader.GetOrdinal("ServiceType")),
                    IsPaid = !reader.IsDBNull(reader.GetOrdinal("IsPaid")) && reader.GetBoolean(reader.GetOrdinal("IsPaid")),
                    IsActive = !reader.IsDBNull(reader.GetOrdinal("IsActive")) && reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    CreditCost = reader.IsDBNull(reader.GetOrdinal("CreditCost")) ? 1 : reader.GetInt32(reader.GetOrdinal("CreditCost")),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.UtcNow : reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetAllCreditTypesAsync failed via ManageCredits_CYH");
        }
        return list;
    }

    public async Task<CreditTypeDTO?> GetCreditTypeByIdAsync(int id)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_TYPE_BY_ID");
            cmd.Parameters.AddWithValue("@CreditTypeId", id);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CreditTypeDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    ServiceType = reader.IsDBNull(reader.GetOrdinal("ServiceType")) ? "Free" : reader.GetString(reader.GetOrdinal("ServiceType")),
                    IsPaid = !reader.IsDBNull(reader.GetOrdinal("IsPaid")) && reader.GetBoolean(reader.GetOrdinal("IsPaid")),
                    IsActive = !reader.IsDBNull(reader.GetOrdinal("IsActive")) && reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    CreditCost = reader.IsDBNull(reader.GetOrdinal("CreditCost")) ? 1 : reader.GetInt32(reader.GetOrdinal("CreditCost")),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.UtcNow : reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetCreditTypeByIdAsync failed for Id {Id}", id);
        }
        return null;
    }

    public async Task<CreditTypeDTO?> GetCreditTypeByCodeAsync(string code)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_TYPE_BY_CODE");
            cmd.Parameters.AddWithValue("@Code", code);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CreditTypeDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    ServiceType = reader.IsDBNull(reader.GetOrdinal("ServiceType")) ? "Free" : reader.GetString(reader.GetOrdinal("ServiceType")),
                    IsPaid = !reader.IsDBNull(reader.GetOrdinal("IsPaid")) && reader.GetBoolean(reader.GetOrdinal("IsPaid")),
                    IsActive = !reader.IsDBNull(reader.GetOrdinal("IsActive")) && reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
                    CreditCost = reader.IsDBNull(reader.GetOrdinal("CreditCost")) ? 1 : reader.GetInt32(reader.GetOrdinal("CreditCost")),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? DateTime.UtcNow : reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetCreditTypeByCodeAsync failed for Code {Code}", code);
        }
        return null;
    }

    public async Task<int> SaveCreditTypeAsync(CreditTypeDTO dto)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "SAVE_TYPE");
            cmd.Parameters.AddWithValue("@CreditTypeId", dto.Id > 0 ? dto.Id : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Name", dto.Name);
            cmd.Parameters.AddWithValue("@Code", dto.Code);
            cmd.Parameters.AddWithValue("@ServiceType", dto.ServiceType);
            cmd.Parameters.AddWithValue("@IsPaid", dto.IsPaid);
            cmd.Parameters.AddWithValue("@IsActive", dto.IsActive);
            cmd.Parameters.AddWithValue("@Description", (object?)dto.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreditCost", dto.CreditCost);

            var res = await cmd.ExecuteScalarAsync();
            return res != null && int.TryParse(res.ToString(), out int newId) ? newId : 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "SaveCreditTypeAsync failed");
            return 0;
        }
    }

    public async Task<bool> ToggleCreditTypeStatusAsync(int id, bool isActive)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "TOGGLE_TYPE_STATUS");
            cmd.Parameters.AddWithValue("@CreditTypeId", id);
            cmd.Parameters.AddWithValue("@IsActive", isActive);

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ToggleCreditTypeStatusAsync failed for Id {Id}", id);
            return false;
        }
    }

    public async Task<bool> ToggleCreditTypePaidStatusAsync(int id, bool isPaid)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "TOGGLE_TYPE_PAID");
            cmd.Parameters.AddWithValue("@CreditTypeId", id);
            cmd.Parameters.AddWithValue("@IsPaid", isPaid);
            cmd.Parameters.AddWithValue("@ServiceType", isPaid ? "Paid" : "Free");

            var rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ToggleCreditTypePaidStatusAsync failed for Id {Id}", id);
            return false;
        }
    }

    public async Task<List<DoctorCreditDTO>> GetDoctorCreditsAsync(int doctorId)
    {
        var list = new List<DoctorCreditDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            // First ensure missing credit types are initialized for this doctor with 0 credits
            await using var initCmd = new SqlCommand("ManageCredits_CYH", con);
            initCmd.CommandType = CommandType.StoredProcedure;
            initCmd.Parameters.AddWithValue("@Flag", "INIT_DOCTOR_CREDITS");
            initCmd.Parameters.AddWithValue("@DoctorId", doctorId);
            await initCmd.ExecuteNonQueryAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_DOCTOR_CREDITS");
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new DoctorCreditDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                    DoctorName = reader.IsDBNull(reader.GetOrdinal("DoctorName")) ? string.Empty : reader.GetString(reader.GetOrdinal("DoctorName")),
                    CreditTypeId = reader.GetInt32(reader.GetOrdinal("CreditTypeId")),
                    CreditTypeName = reader.GetString(reader.GetOrdinal("CreditTypeName")),
                    CreditTypeCode = reader.GetString(reader.GetOrdinal("CreditTypeCode")),
                    ServiceType = reader.IsDBNull(reader.GetOrdinal("ServiceType")) ? "Free" : reader.GetString(reader.GetOrdinal("ServiceType")),
                    IsPaid = !reader.IsDBNull(reader.GetOrdinal("IsPaid")) && reader.GetBoolean(reader.GetOrdinal("IsPaid")),
                    TotalCredits = reader.GetInt32(reader.GetOrdinal("TotalCredits")),
                    UsedCredits = reader.GetInt32(reader.GetOrdinal("UsedCredits")),
                    IsActive = !reader.IsDBNull(reader.GetOrdinal("IsActive")) && reader.GetBoolean(reader.GetOrdinal("IsActive")),
                    LastUpdatedAt = reader.IsDBNull(reader.GetOrdinal("LastUpdatedAt")) ? DateTime.UtcNow : reader.GetDateTime(reader.GetOrdinal("LastUpdatedAt"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetDoctorCreditsAsync failed for DoctorId {DoctorId}", doctorId);
        }
        return list;
    }

    public async Task<DoctorCreditDTO?> GetDoctorCreditByTypeAsync(int doctorId, string creditTypeCode)
    {
        var credits = await GetDoctorCreditsAsync(doctorId);
        return credits.FirstOrDefault(c => string.Equals(c.CreditTypeCode, creditTypeCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> InitializeDoctorCreditsAsync(int doctorId)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "INIT_DOCTOR_CREDITS");
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);

            var rows = await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "InitializeDoctorCreditsAsync failed for DoctorId {DoctorId}", doctorId);
            return false;
        }
    }

    public async Task<(bool Success, string Message, int BalanceAfter)> AdjustDoctorCreditAsync(AdminAdjustCreditDTO dto)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "ADMIN_ADJUST");
            cmd.Parameters.AddWithValue("@DoctorId", dto.DoctorId);
            cmd.Parameters.AddWithValue("@CreditTypeId", dto.CreditTypeId);
            cmd.Parameters.AddWithValue("@Amount", dto.Amount);
            cmd.Parameters.AddWithValue("@ActionBy", string.IsNullOrEmpty(dto.ActionBy) ? "Admin" : dto.ActionBy);
            cmd.Parameters.AddWithValue("@Note", (object?)dto.Note ?? DBNull.Value);

            var outSuccess = cmd.Parameters.Add("@OutSuccess", SqlDbType.Bit);
            outSuccess.Direction = ParameterDirection.Output;

            var outMsg = cmd.Parameters.Add("@OutMessage", SqlDbType.NVarChar, 500);
            outMsg.Direction = ParameterDirection.Output;

            var outBalance = cmd.Parameters.Add("@OutBalanceAfter", SqlDbType.Int);
            outBalance.Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            bool success = outSuccess.Value != DBNull.Value && Convert.ToBoolean(outSuccess.Value);
            string message = outMsg.Value != DBNull.Value ? outMsg.Value.ToString() ?? "" : "";
            int balance = outBalance.Value != DBNull.Value ? Convert.ToInt32(outBalance.Value) : 0;

            return (success, message, balance);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "AdjustDoctorCreditAsync failed");
            return (false, ex.Message, 0);
        }
    }

    public async Task<(bool Success, string Message, int RemainingCredits)> DeductCreditOnPublishAsync(int doctorId, string creditTypeCode, string? referenceId = null, string? note = null)
    {
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "DEDUCT_PUBLISH");
            cmd.Parameters.AddWithValue("@DoctorId", doctorId);
            cmd.Parameters.AddWithValue("@Code", creditTypeCode);
            cmd.Parameters.AddWithValue("@ReferenceId", (object?)referenceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Note", (object?)note ?? DBNull.Value);

            var outSuccess = cmd.Parameters.Add("@OutSuccess", SqlDbType.Bit);
            outSuccess.Direction = ParameterDirection.Output;

            var outMsg = cmd.Parameters.Add("@OutMessage", SqlDbType.NVarChar, 500);
            outMsg.Direction = ParameterDirection.Output;

            var outBalance = cmd.Parameters.Add("@OutBalanceAfter", SqlDbType.Int);
            outBalance.Direction = ParameterDirection.Output;

            await cmd.ExecuteNonQueryAsync();

            bool success = outSuccess.Value != DBNull.Value && Convert.ToBoolean(outSuccess.Value);
            string message = outMsg.Value != DBNull.Value ? outMsg.Value.ToString() ?? "" : "";
            int balance = outBalance.Value != DBNull.Value ? Convert.ToInt32(outBalance.Value) : 0;

            return (success, message, balance);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "DeductCreditOnPublishAsync failed for DoctorId {DoctorId}, Code {Code}", doctorId, creditTypeCode);
            return (false, ex.Message, 0);
        }
    }

    public async Task<List<CreditTransactionDTO>> GetTransactionsAsync(int? doctorId = null, int? creditTypeId = null, string? transactionType = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var list = new List<CreditTransactionDTO>();
        try
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync();

            await using var cmd = new SqlCommand("ManageCredits_CYH", con);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Flag", "GET_TRANSACTIONS");
            cmd.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreditTypeId", (object?)creditTypeId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TransactionType", (object?)transactionType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@FromDate", (object?)fromDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ToDate", (object?)toDate ?? DBNull.Value);

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new CreditTransactionDTO
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                    DoctorName = reader.IsDBNull(reader.GetOrdinal("DoctorName")) ? string.Empty : reader.GetString(reader.GetOrdinal("DoctorName")),
                    CreditTypeId = reader.GetInt32(reader.GetOrdinal("CreditTypeId")),
                    CreditTypeName = reader.GetString(reader.GetOrdinal("CreditTypeName")),
                    CreditTypeCode = reader.GetString(reader.GetOrdinal("CreditTypeCode")),
                    ServiceType = reader.IsDBNull(reader.GetOrdinal("ServiceType")) ? "Free" : reader.GetString(reader.GetOrdinal("ServiceType")),
                    TransactionType = reader.GetString(reader.GetOrdinal("TransactionType")),
                    CreditsChanged = reader.GetInt32(reader.GetOrdinal("CreditsChanged")),
                    BalanceBefore = reader.GetInt32(reader.GetOrdinal("BalanceBefore")),
                    BalanceAfter = reader.GetInt32(reader.GetOrdinal("BalanceAfter")),
                    ActionBy = reader.IsDBNull(reader.GetOrdinal("ActionBy")) ? null : reader.GetString(reader.GetOrdinal("ActionBy")),
                    ReferenceId = reader.IsDBNull(reader.GetOrdinal("ReferenceId")) ? null : reader.GetString(reader.GetOrdinal("ReferenceId")),
                    Note = reader.IsDBNull(reader.GetOrdinal("Note")) ? null : reader.GetString(reader.GetOrdinal("Note")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                });
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "GetTransactionsAsync failed");
        }
        return list;
    }
}
