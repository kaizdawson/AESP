// AESP.Service/BackgroundJobs/ProgressAnalyticsBackgroundService.cs
using AESP.Repository.Contract;
using AESP.Repository.Models;
using AESP.Service.Contract;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AESP.Service.BackgroundJobs
{
    public class ProgressAnalyticsBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ProgressAnalyticsBackgroundService> _logger;
        private readonly TimeSpan _delay = TimeSpan.FromMinutes(1); // chu kỳ chạy

        public ProgressAnalyticsBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ProgressAnalyticsBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ProgressAnalyticsBackgroundService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var learnerProfileRepo = scope.ServiceProvider
                        .GetRequiredService<IGenericRepository<LearnerProfile>>();

                    var progressService = scope.ServiceProvider
                        .GetRequiredService<IProgressAnalyticsService>();

                    // lấy toàn bộ learnerProfile còn active
                    var learners = await learnerProfileRepo.AsQueryable()
                        .Where(lp => !lp.IsDeleted)
                        .Select(lp => lp.LearnerProfileId)
                        .ToListAsync(stoppingToken);

                    foreach (var learnerProfileId in learners)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        await progressService.UpdateLifetimeAsync(learnerProfileId);
                    }

                    _logger.LogInformation("ProgressAnalyticsBackgroundService: cập nhật xong vòng lặp.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi cập nhật ProgressAnalytics.");
                }

                try
                {
                    await Task.Delay(_delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // ignore
                }
            }

            _logger.LogInformation("ProgressAnalyticsBackgroundService stopped.");
        }
    }
}
