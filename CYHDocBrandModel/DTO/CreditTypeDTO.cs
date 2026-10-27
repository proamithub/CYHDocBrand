using CYHDocBrandModel.Common;
namespace CYHDocBrandModel.DTO;

public class CreditTypeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;           // e.g. TEMPLATE_PUBLISH, AI_BANNER_PUBLISH
    public string ServiceType { get; set; } = "Free";          // "Free" / "Paid"
    public bool IsPaid { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }
    public int CreditCost { get; set; } = 1;                   // credits deducted per action
    public DateTime CreatedAt { get; set; } = IndianTimeHelper.Now;
}
