using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using CYHDocBrandModel.DTO;
using CYHDocBrandRepo.IRepository;

namespace CYHDocBrandUI.Services;

/// <summary>
/// 100% Without-API Background Scheduler Worker.
/// Detects when a banner schedule has reached its scheduled time, transitions the status to ReadyToShare,
/// and cues up instant notifications and 1-click sharing (WhatsApp, Facebook, Instagram) for the doctor.
/// </summary>
public class BannerPublishBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BannerPublishBackgroundWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(25);

    public BannerPublishBackgroundWorker(IServiceProvider serviceProvider, ILogger<BannerPublishBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 CYH Banner Scheduler Background Worker started. Checking due schedules every {Interval}s.", _checkInterval.TotalSeconds);

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

        var dueSchedules = await scheduleRepo.GetDueSchedulesAsync();
        if (dueSchedules == null || dueSchedules.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Found {Count} due banner schedule(s) for 1-Click popup sharing & reminder.", dueSchedules.Count);

        foreach (var schedule in dueSchedules)
        {
            if (cancellationToken.IsCancellationRequested) break;

            foreach (var detail in schedule.Details)
            {
                await scheduleRepo.UpdateDetailStatusAsync(detail.Id, "ReadyToShare", null, null, "Ready for 1-Click Share");
            }

            await scheduleRepo.UpdateScheduleStatusAsync(schedule.Id, "ReadyToShare", null);
            _logger.LogInformation("Schedule #{Id} ('{Heading}') is now ReadyToShare on {Platforms} for Dr. {DoctorName}.", 
                schedule.Id, schedule.BannerHeading, schedule.Platforms, schedule.DoctorName);
        }
    }
}
