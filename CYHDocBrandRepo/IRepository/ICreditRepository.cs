using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface ICreditRepository
{
    // Credit Types (Admin config)
    Task<List<CreditTypeDTO>> GetAllCreditTypesAsync(bool onlyActive = false);
    Task<CreditTypeDTO?> GetCreditTypeByIdAsync(int id);
    Task<CreditTypeDTO?> GetCreditTypeByCodeAsync(string code);
    Task<int> SaveCreditTypeAsync(CreditTypeDTO dto);
    Task<bool> ToggleCreditTypeStatusAsync(int id, bool isActive);
    Task<bool> ToggleCreditTypePaidStatusAsync(int id, bool isPaid);

    // Doctor Credits
    Task<List<DoctorCreditDTO>> GetDoctorCreditsAsync(int doctorId);
    Task<DoctorCreditDTO?> GetDoctorCreditByTypeAsync(int doctorId, string creditTypeCode);
    Task<bool> InitializeDoctorCreditsAsync(int doctorId);
    Task<(bool Success, string Message, int BalanceAfter)> AdjustDoctorCreditAsync(AdminAdjustCreditDTO dto);
    Task<(bool Success, string Message, int RemainingCredits)> DeductCreditOnPublishAsync(int doctorId, string creditTypeCode, string? referenceId = null, string? note = null);

    // Credit Transactions / History
    Task<List<CreditTransactionDTO>> GetTransactionsAsync(int? doctorId = null, int? creditTypeId = null, string? transactionType = null, DateTime? fromDate = null, DateTime? toDate = null);
}
