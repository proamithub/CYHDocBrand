using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandUI.Services;

public class BannerPublishBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BannerPublishBackgroundWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(30);

    public BannerPublishBackgroundWorker(IServiceProvider serviceProvider, ILogger<BannerPublishBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 CYH Banner Publish Background Worker started. Checking due schedules every {Interval}s.", _checkInterval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueSchedulesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Banner Publish Background Worker loop.");
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

        _logger.LogInformation("🛑 CYH Banner Publish Background Worker stopped.");
    }

    private async Task ProcessDueSchedulesAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var scheduleRepo = scope.ServiceProvider.GetRequiredService<IBannerScheduleRepository>();
        var socialRepo = scope.ServiceProvider.GetRequiredService<ISocialAccountRepository>();
        var publisher = scope.ServiceProvider.GetRequiredService<ISocialMediaPublisherService>();

        var dueSchedules = await scheduleRepo.GetDueSchedulesAsync();
        if (dueSchedules == null || dueSchedules.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} due banner schedule(s) to publish.", dueSchedules.Count);

        foreach (var schedule in dueSchedules)
        {
            if (cancellationToken.IsCancellationRequested) break;

            var doctorAccounts = await socialRepo.GetByDoctorAsync(schedule.DoctorId, schedule.UserId);
            int successCount = 0;
            int failedCount = 0;
            var failureMessages = new List<string>();

            foreach (var detail in schedule.Details)
            {
                // Find account matching platform
                var account = doctorAccounts.FirstOrDefault(a => 
                    a.IsActive && a.Platform.Equals(detail.Platform, StringComparison.OrdinalIgnoreCase));

                if (account == null)
                {
                    var msg = $"No active {detail.Platform} connection found for {schedule.DoctorName}.";
                    await scheduleRepo.UpdateDetailStatusAsync(detail.Id, "Failed", null, null, msg);
                    failedCount++;
                    failureMessages.Add(msg);
                    continue;
                }

                // Call Social Media Publisher
                var result = await publisher.PublishBannerAsync(account, schedule.ImageUrl, schedule.Caption);

                if (result.IsSuccess)
                {
                    successCount++;
                    await scheduleRepo.UpdateDetailStatusAsync(detail.Id, "Success", result.PlatformPostId, result.PostUrl, null);
                    _logger.LogInformation("Successfully published Schedule #{Id} to {Platform}: {PostUrl}", schedule.Id, detail.Platform, result.PostUrl);
                }
                else
                {
                    failedCount++;
                    var errMsg = result.ErrorMessage ?? "Publishing failed.";
                    failureMessages.Add($"{detail.Platform}: {errMsg}");
                    await scheduleRepo.UpdateDetailStatusAsync(detail.Id, "Failed", null, null, errMsg);
                    _logger.LogWarning("Failed to publish Schedule #{Id} to {Platform}: {Error}", schedule.Id, detail.Platform, errMsg);
                }
            }

            // Determine Master Schedule Status
            string finalStatus;
            string? combinedError = null;

            if (failedCount == 0 && successCount > 0)
            {
                finalStatus = "Success";
            }
            else if (successCount > 0 && failedCount > 0)
            {
                finalStatus = "Partial";
                combinedError = string.Join("; ", failureMessages);
            }
            else
            {
                finalStatus = "Failed";
                combinedError = string.Join("; ", failureMessages);
            }

            await scheduleRepo.UpdateScheduleStatusAsync(schedule.Id, finalStatus, combinedError);
        }
    }
}
