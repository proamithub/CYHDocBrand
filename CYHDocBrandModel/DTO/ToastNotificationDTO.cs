namespace CYHDocBrandModel.DTO;

public class ToastNotificationDTO
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "save"; // "save", "edit", "delete", "error", "warning"

    public string Icon => Type switch
    {
        "save" => "bi-check-circle-fill",
        "edit" => "bi-pencil-square",
        "delete" => "bi-trash-fill",
        "error" => "bi-x-circle-fill",
        "warning" => "bi-exclamation-triangle-fill",
        _ => "bi-bell-fill"
    };

    public string BorderColor => Type switch
    {
        "save" => "rgba(16, 185, 129, 0.8)",
        "edit" => "rgba(0, 242, 254, 0.8)",
        "delete" => "rgba(239, 68, 68, 0.8)",
        "error" => "rgba(239, 68, 68, 0.8)",
        "warning" => "rgba(245, 158, 11, 0.8)",
        _ => "rgba(0, 242, 254, 0.8)"
    };

    public string GlowColor => Type switch
    {
        "save" => "rgba(16, 185, 129, 0.35)",
        "edit" => "rgba(0, 242, 254, 0.35)",
        "delete" => "rgba(239, 68, 68, 0.35)",
        "error" => "rgba(239, 68, 68, 0.35)",
        "warning" => "rgba(245, 158, 11, 0.35)",
        _ => "rgba(0, 242, 254, 0.35)"
    };

    public string IconBg => Type switch
    {
        "save" => "rgba(16, 185, 129, 0.2)",
        "edit" => "rgba(0, 242, 254, 0.2)",
        "delete" => "rgba(239, 68, 68, 0.2)",
        "error" => "rgba(239, 68, 68, 0.2)",
        "warning" => "rgba(245, 158, 11, 0.2)",
        _ => "rgba(0, 242, 254, 0.2)"
    };

    public string IconColor => Type switch
    {
        "save" => "#10b981",
        "edit" => "#00f2fe",
        "delete" => "#ef4444",
        "error" => "#ef4444",
        "warning" => "#f59e0b",
        _ => "#00f2fe"
    };
}
