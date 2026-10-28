using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;
using CYHDocBrandModel.Common;

namespace CYHDocBrandUI.Services;

/// <summary>
/// Banner Scheduler Background Worker with Dynamic Multi-Platform & Fallback Logic:
/// 1. Runs on Indian Standard Time (IST).
/// 2. Checks due schedules where ScheduledAt <= Current IST Time.
/// 3. For each targeted platform:
///    - Checks if Doctor has connected their own account (DoctorSocialAccounts_CYH).
///    - If not connected, checks AllowGlobalFallback:
///      * Google (AllowGlobalFallback=0): Global fallback is strictly disabled; requires doctor account.
///      * WhatsApp / Facebook / Instagram / Twitter / LinkedIn (AllowGlobalFallback=1): Auto-falls back to Admin Global API Config.
/// 4. Dispatches Instant Web-Push Notification to Doctor's devices for 1-click sharing or automated delivery.
/// </summary>
public class BannerPublishBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BannerPublishBackgroundWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5);

    public BannerPublishBackgroundWorker(IServiceProvider serviceProvider, ILogger<BannerPublishBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 CYH Banner Scheduler Background Worker started. Checking due schedules every {Interval}s (IST: {Now:yyyy-MM-dd HH:mm:ss}).", 
            _checkInterval.TotalSeconds, IndianTimeHelper.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueSchedulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Banner Scheduler Background Worker loop.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("🛑 CYH Banner Scheduler Background Worker stopped.");
    }

    private async Task ProcessDueSchedulesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scheduleRepo = scope.ServiceProvider.GetRequiredService<IBannerScheduleRepository>();
        var platformRepo = scope.ServiceProvider.GetRequiredService<IMediaPlatformRepository>();
        var socialRepo = scope.ServiceProvider.GetRequiredService<ISocialAccountRepository>();
        var pushService = scope.ServiceProvider.GetService<IWebPushNotificationService>();

        var dueSchedules = await scheduleRepo.GetDueSchedulesAsync();
        if (dueSchedules == null || dueSchedules.Count == 0)
        {
            return;
        }

        _logger.LogInformation("⏰ Found {Count} due banner schedule(s) at {Now:dd MMM, hh:mm tt} IST.", 
            dueSchedules.Count, IndianTimeHelper.Now);

        // Fetch active platforms once per batch
        var activePlatforms = await platformRepo.GetActiveAsync();
        var platformMap = activePlatforms.ToDictionary(p => p.PlatformCode.ToUpperInvariant(), p => p);

        foreach (var schedule in dueSchedules)
        {
            if (cancellationToken.IsCancellationRequested) break;

            // Fetch doctor's connected social accounts
            List<DoctorSocialAccountDTO> doctorAccounts = new();
            try
            {
                doctorAccounts = await socialRepo.GetByDoctorAsync(schedule.DoctorId, schedule.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not fetch doctor social accounts for Dr. {Doctor}", schedule.DoctorName);
            }

            foreach (var detail in schedule.Details)
            {
                var platformKey = detail.Platform.Trim().ToUpperInvariant();
                platformMap.TryGetValue(platformKey, out var platformConfig);

                // Find doctor-specific connected account
                var docAccount = doctorAccounts.FirstOrDefault(a => 
                    a.IsActive && (a.Platform.Equals(detail.Platform, StringComparison.OrdinalIgnoreCase) ||
                                  a.Platform.Equals(platformKey, StringComparison.OrdinalIgnoreCase)));

                if (docAccount != null)
                {
                    // Case A: Doctor has connected their own account
                    await scheduleRepo.UpdateDetailStatusAsync(
                        detail.Id, 
                        "ReadyToShare", 
                        null, 
                        null, 
                        $"Doctor's own account ({docAccount.AccountName}) connected."
                    );
                    _logger.LogInformation("Schedule #{Id} [{Platform}]: Using Dr. {Doctor}'s account '{Account}'.",
                        schedule.Id, detail.Platform, schedule.DoctorName, docAccount.AccountName);
                }
                else
                {
                    // Case B: Doctor does not have account connected -> check fallback rules
                    if (platformConfig != null && !platformConfig.AllowGlobalFallback)
                    {
                        // Google or platforms that strictly prohibit global fallback
                        await scheduleRepo.UpdateDetailStatusAsync(
                            detail.Id,
                            "Failed",
                            null,
                            null,
                            $"Doctor's {platformConfig.PlatformName} account is not linked. Global fallback is disabled for this platform."
                        );
                        _logger.LogWarning("Schedule #{Id} [{Platform}]: Skipped global fallback for Dr. {Doctor} (Doctor account required).",
                            schedule.Id, detail.Platform, schedule.DoctorName);
                    }
                    else if (platformConfig != null && platformConfig.HasGlobalApiConfigured)
                    {
                        // Global Admin Fallback API available
                        await scheduleRepo.UpdateDetailStatusAsync(
                            detail.Id,
                            "ReadyToShare",
                            null,
                            null,
                            $"Using Hospital Common API fallback ({platformConfig.PlatformName})."
                        );
                        _logger.LogInformation("Schedule #{Id} [{Platform}]: Dr. {Doctor} has no account; falling back to Hospital Global API.",
                            schedule.Id, detail.Platform, schedule.DoctorName);
                    }
                    else
                    {
                        // Standard 1-Click popup share
                        await scheduleRepo.UpdateDetailStatusAsync(
                            detail.Id,
                            "ReadyToShare",
                            null,
                            null,
                            "Ready for 1-Click popup sharing."
                        );
                    }
                }
            }

            await scheduleRepo.UpdateScheduleStatusAsync(schedule.Id, "ReadyToShare", null);
            _logger.LogInformation("Schedule #{Id} ('{Heading}') marked ReadyToShare for Dr. {DoctorName} on {Platforms}.", 
                schedule.Id, schedule.BannerHeading, schedule.Platforms, schedule.DoctorName);

            // 🚀 Send Doctor-Wise Web Push Notification (Works even when browser is completely CLOSED!)
            if (pushService != null)
            {
                try
                {
                    var pushedCount = await pushService.SendPushToDoctorAsync(
                        schedule.DoctorId,
                        schedule.UserId,
                        $"⏰ Scheduled Banner: {schedule.BannerHeading}",
                        $"Dr. {schedule.DoctorName}, your scheduled post is ready! Tap to share on {schedule.Platforms}.",
                        schedule
                    );
                    _logger.LogInformation("Delivered Web-Push to {Count} device(s) for Dr. {DoctorName}.", pushedCount, schedule.DoctorName);
                }
                catch (Exception pushEx)
                {
                    _logger.LogError(pushEx, "Failed to send background Web-Push for schedule #{Id}", schedule.Id);
                }
            }
        }
    }
}
