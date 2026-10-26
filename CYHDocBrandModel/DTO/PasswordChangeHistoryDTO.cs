namespace CYHDocBrandModel.DTO;

public class PasswordChangeHistoryDTO
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserRole { get; set; }
    public string? IpAddress { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Country { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public string? LocationSummary { get; set; }
    public string? UserAgent { get; set; }
    public string? Remarks { get; set; }
    public DateTime ChangedAt { get; set; }
}
