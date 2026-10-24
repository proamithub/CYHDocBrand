namespace CYHDocBrandModel.DTO;

public class DoctorAiSettingDTO
{
    public int Id { get; set; }
    public int? DoctorId { get; set; }
    public string? UserId { get; set; }
    public string? DoctorName { get; set; }
    public string Provider { get; set; } = "Gemini"; // Gemini, OpenAI, Claude, etc.
    public string ApiKey { get; set; } = string.Empty;
    public string? ModelName { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Helper display
    public string MaskedApiKey => string.IsNullOrWhiteSpace(ApiKey) 
        ? "Not Configured" 
        : (ApiKey.Length > 8 ? $"{ApiKey[..4]}...{ApiKey[^4..]}" : "********");
}
