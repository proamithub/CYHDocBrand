using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandUI.Services;

public class WebPushNotificationService : IWebPushNotificationService
{
    private readonly IPushNotificationRepository _pushRepo;
    private readonly ILogger<WebPushNotificationService> _logger;
    private readonly VapidDetails _vapidDetails;

    public string PublicKey => _vapidDetails.PublicKey;

    public WebPushNotificationService(
        IPushNotificationRepository pushRepo,
        IConfiguration config,
        ILogger<WebPushNotificationService> logger)
    {
        _pushRepo = pushRepo;
        _logger = logger;

        var subject = config["Vapid:Subject"] ?? "mailto:support@cyhdoctorhub.com";
        var publicKey = config["Vapid:PublicKey"] ?? "BGRetf6tCBjK2Tb7kBSZ1hNGBPQODZ3GJlL--LbhxOgojAXo7F6F3WOhH3wnt0hof5iitcVIttzuxnXib2BA8ZY";
        var privateKey = config["Vapid:PrivateKey"] ?? "go7Cd6MWUdO_RunOLYqAO2PEKZiHbhlKrbGlOL9FMQ8";

        _vapidDetails = new VapidDetails(subject, publicKey, privateKey);
    }

    public async Task<int> SendPushToDoctorAsync(int? doctorId, string? userId, string title, string message, BannerScheduleDTO schedule)
    {
        var subscriptions = await _pushRepo.GetSubscriptionsByDoctorAsync(doctorId, userId);
        if (subscriptions == null || subscriptions.Count == 0)
        {
            _logger.LogInformation("No push subscription devices registered for DoctorId {DocId} / UserId {UserId}", doctorId, userId);
            return 0;
        }

        var client = new WebPushClient();

        var whatsappMsg = $"{schedule.BannerHeading}\n\n{schedule.Caption}";
        if (!string.IsNullOrEmpty(schedule.ImageUrl))
        {
            whatsappMsg += $"\n\n📷 Banner Image: {schedule.ImageUrl}";
        }
        var whatsappUrl = "https://api.whatsapp.com/send?text=" + Uri.EscapeDataString(whatsappMsg);

        var payloadObj = new
        {
            title = title,
            body = message,
            icon = "/images/branding/icon-192.png",
            badge = "/favicon.png",
            image = schedule.ImageUrl,
            tag = $"schedule-{schedule.Id}",
            requireInteraction = true,
            data = new
            {
                scheduleId = schedule.Id,
                bannerHeading = schedule.BannerHeading,
                whatsappUrl = whatsappUrl,
                actionUrl = "/doctor/schedules"
            }
        };

        var payloadJson = JsonSerializer.Serialize(payloadObj);
        int successfulPushes = 0;

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await client.SendNotificationAsync(pushSub, payloadJson, _vapidDetails);
                successfulPushes++;
                _logger.LogInformation("✅ Web push successfully delivered to device #{SubId} for Dr. {DoctorName}", sub.Id, schedule.DoctorName);
            }
            catch (WebPushException ex)
            {
                _logger.LogWarning("Push service responded with status {StatusCode}: {Message}", ex.StatusCode, ex.Message);
                if (ex.StatusCode == System.Net.HttpStatusCode.Gone || ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Device unsubscribed or token expired - clean it up
                    await _pushRepo.DeleteSubscriptionAsync(sub.Endpoint);
                    _logger.LogInformation("Cleaned up expired push subscription for endpoint {Endpoint}", sub.Endpoint);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error delivering web push to device #{SubId}", sub.Id);
            }
        }

        return successfulPushes;
    }
}
