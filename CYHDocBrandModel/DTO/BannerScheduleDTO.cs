namespace CYHDocBrandModel.DTO;

public class BannerScheduleDTO
{
    public int Id { get; set; }
    public int BannerDesignId { get; set; }
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? HospitalName { get; set; }
    public string? BannerHeading { get; set; }
    public string? ImageUrl { get; set; }
    public string? Caption { get; set; }
    public string Platforms { get; set; } = string.Empty; // Comma separated: "Facebook,Instagram"
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Success, Failed, Cancelled
    public int ExecutionAttempts { get; set; } = 0;
    public DateTime? LastExecutedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }

    // Child platform statuses
    public List<BannerScheduleDetailDTO> Details { get; set; } = new();

    // Helper for UI
    public bool IsDue => ScheduledAt <= DateTime.UtcNow && Status == "Pending";
    public string[] PlatformList => (Platforms ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public class BannerScheduleDetailDTO
{
    public int Id { get; set; }
    public int BannerScheduleId { get; set; }
    public string Platform { get; set; } = string.Empty; // Facebook, Instagram, LinkedIn, Twitter
    public string? TargetAccountName { get; set; }
    public string? TargetPageId { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Processing, Success, Failed
    public string? PlatformPostId { get; set; }
    public string? PostUrl { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class CreateBannerScheduleRequest
{
    public int BannerDesignId { get; set; }
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string? HospitalName { get; set; }
    public string? BannerHeading { get; set; }
    public string? ImageUrl { get; set; }
    public string? Caption { get; set; }
    public List<string> SelectedPlatforms { get; set; } = new();
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
}

public class AdminScheduleSummaryDTO
{
    public int TotalScheduled { get; set; }
    public int PendingCount { get; set; }
    public int ProcessingCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<BannerScheduleDTO> Schedules { get; set; } = new();
}
