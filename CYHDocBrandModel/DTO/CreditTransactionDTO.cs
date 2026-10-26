namespace CYHDocBrandModel.DTO;

public class CreditTransactionDTO
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public int CreditTypeId { get; set; }
    public string CreditTypeName { get; set; } = string.Empty;
    public string CreditTypeCode { get; set; } = string.Empty;
    public string ServiceType { get; set; } = "Free";

    // ADMIN_ADD | ADMIN_DEDUCT | DEBIT_ON_PUBLISH | REFUND | SYSTEM_INIT
    public string TransactionType { get; set; } = string.Empty;

    public int CreditsChanged { get; set; }                    // +ve for add, -ve for deduct
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public string? ActionBy { get; set; }
    public string? ReferenceId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // UI Helpers
    public bool IsCredit => CreditsChanged > 0;
    public string SignedAmount => IsCredit ? $"+{CreditsChanged}" : $"{CreditsChanged}";
    public string BadgeClass => TransactionType switch
    {
        "ADMIN_ADD"         => "bg-success",
        "REFUND"            => "bg-info",
        "DEBIT_ON_PUBLISH"  => "bg-danger",
        "ADMIN_DEDUCT"      => "bg-warning",
        "SYSTEM_INIT"       => "bg-secondary",
        _                   => "bg-secondary"
    };
    public string TransactionLabel => TransactionType switch
    {
        "ADMIN_ADD"         => "Admin Add",
        "ADMIN_DEDUCT"      => "Admin Deduct",
        "DEBIT_ON_PUBLISH"  => "Published",
        "REFUND"            => "Refund",
        "SYSTEM_INIT"       => "Initial Setup",
        _                   => TransactionType
    };
}
