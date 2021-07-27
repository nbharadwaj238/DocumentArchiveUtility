using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DocumentArchiveUtility.Services;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using DocumentArchiveUtility.Interfaces;

namespace DocumentArchiveUtility.Jobs
{
    public class MyCronJob3 : CronJobService
    {
        private readonly ILogger<MyCronJob3> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MyCronJob3(IScheduleConfig<MyCronJob3> config, ILogger<MyCronJob3> logger, IServiceProvider serviceProvider)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 3 starts.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} CronJob 3 is working.");
            Log.Information($"{DateTime.Now:hh:mm:ss} CronJob 3 is working.");

            using var scope = _serviceProvider.CreateScope();
           // var svc = scope.ServiceProvider.GetRequiredService<IDBService>();
           // svc.DownloadDirectory();
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 3 is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
