using CYHDocBrandModel.DTO;

namespace CYHDocBrandRepo.IRepository;

public interface IPushNotificationRepository
{
    Task<bool> SaveSubscriptionAsync(PushSubscriptionRequest request);
    Task<List<DoctorPushSubscriptionDTO>> GetSubscriptionsByDoctorAsync(int? doctorId, string? userId);
    Task<bool> DeleteSubscriptionAsync(string endpoint);
}
