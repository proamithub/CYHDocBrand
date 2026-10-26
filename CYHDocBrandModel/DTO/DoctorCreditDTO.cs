namespace CYHDocBrandModel.DTO;

public class DoctorCreditDTO
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int CreditTypeId { get; set; }
    public string CreditTypeName { get; set; } = string.Empty;
    public string CreditTypeCode { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Free";          // "Free" / "Paid"
    public bool IsPaid { get; set; } = false;
    public int TotalCredits { get; set; } = 0;
    public int UsedCredits { get; set; } = 0;
    public int RemainingCredits => TotalCredits - UsedCredits;
    public bool IsActive { get; set; } = true;
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
}

public class AdminAdjustCreditDTO
{
    public int DoctorId { get; set; }
    public int CreditTypeId { get; set; }
    public int Amount { get; set; }
    public string ActionBy { get; set; } = string.Empty;
    public string? Note { get; set; }
}
