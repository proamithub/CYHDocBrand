namespace CYHDocBrandModel.DTO;

public class PushSubscriptionRequest
{
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string? UserAgent { get; set; }
}

public class DoctorPushSubscriptionDTO
{
    public int Id { get; set; }
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
}
