using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DocumentArchiveUtility.Services;
using Microsoft.Extensions.DependencyInjection;
using DocumentArchiveUtility.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DocumentArchiveUtility.Jobs
{
    public class MyCronJob1 : CronJobService
    {
        private readonly ILogger<MyCronJob1> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public MyCronJob1(IScheduleConfig<MyCronJob1> config, ILogger<MyCronJob1> logger, IServiceProvider serviceProvider, IConfiguration configuration)
            : base(config.CronExpression, config.TimeZoneInfo)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 1 starts.");
            Log.Information("CronJob 1 starts.");
            return base.StartAsync(cancellationToken);
        }

        public override Task DoWork(CancellationToken cancellationToken)
        {
            
            Log.Information("CronJob 1 : DoWork : Started.");
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} CronJob 1 is working.");
            List<string> exceptions = new List<string>();
            exceptions.Add(@".user");
            exceptions.Add(@".suo");
            exceptions.Add(@"\bin");
            exceptions.Add(@"\obj");
            exceptions.Add(@"\packages");

            using var scope = _serviceProvider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<ICompressionService>();
            var archiveName = string.Empty;

            var uploadDir1 = new DirectoryInfo(_configuration.GetValue<string>("Directory:Path"));
            var baseDirLength = uploadDir1.FullName.Length + 1;

            //IEnumerable<DirectoryInfo> directories = Common.GetFoldersOlderThanXDays(directoryPath, -4);
            var directories = uploadDir1.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Where(x => x.CreationTime < DateTime.Now.AddDays(Convert.ToDouble(_configuration["Services:Compressed:OlderThan"])) ||
                                    x.LastWriteTime < DateTime.Now.AddDays(Convert.ToDouble(_configuration["Services:Compressed:OlderThan"])));
            Task<int> filecount = Task.FromResult(0);
            foreach (var directory in directories)
            {
                archiveName = directory.ToString() + ".zip";
                filecount = svc.CreateArchive(directory.ToString(), exceptions, archiveName);
            }

            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} Compression completed FilesCount." + filecount.Result);
            Log.Information("CronJob 1 : DoWork :" + archiveName + "Compression completed FilesCount." + filecount.Result);
            return filecount;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("CronJob 1 is stopping.");
            Log.Information("CronJob 1 is stopping.");
            return base.StopAsync(cancellationToken);
        }
    }
}
