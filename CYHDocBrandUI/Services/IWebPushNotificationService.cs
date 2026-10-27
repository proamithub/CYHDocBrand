using CYHDocBrandModel.DTO;

namespace CYHDocBrandUI.Services;

public interface IWebPushNotificationService
{
    string PublicKey { get; }
    Task<int> SendPushToDoctorAsync(int? doctorId, string? userId, string title, string message, BannerScheduleDTO schedule);
}
