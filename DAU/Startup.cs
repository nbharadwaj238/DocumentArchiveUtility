using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DocumentArchiveUtility.Services;
using DocumentArchiveUtility.Jobs;
using DocumentArchiveUtility.Lib;
using DocumentArchiveUtility.Interfaces;
using DocumentArchiveUtility.Models;
using Serilog;
using DocumentArchiveUtility.Middleware;

namespace DocumentArchiveUtility
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public ISecretManager SecretManager { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddScoped<IBlobService, BlobService>();
            services.AddScoped<ICompressionService, CompressionService>();
            services.Configure<MyConfig>(Configuration.GetSection("MyConfig"));
            services.RegisterDependencies(Configuration, SecretManager);
            Log.Information("StartUp: Configure The Jobs");
            services.AddCronJob<MyCronJob1>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = Configuration["ScheduleJob:MyCronJob1:CronExpression"];
            });
            // MyCronJob2 calls the scoped service Blob Service
            services.AddCronJob<MyCronJob2>(c =>
            {
                c.TimeZoneInfo = TimeZoneInfo.Local;
                c.CronExpression = Configuration["ScheduleJob:MyCronJob2:CronExpression"];
            });

            //services.AddCronJob<MyCronJob3>(c =>
            //{
            //    c.TimeZoneInfo = TimeZoneInfo.Local;
            //    c.CronExpression = Configuration["ScheduleJob:MyCronJob3:CronExpression"]; ;
            //});

            Log.Logger = new LoggerConfiguration()
             .MinimumLevel.Verbose()
             .WriteTo.File(Configuration["LogFile:Path"], rollingInterval: RollingInterval.Day)
             .CreateLogger();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                Log.Information("Development Environment :: Configure");
                app.UseDeveloperExceptionPage();
            }
            app.UseGlobalExceptionMiddleware();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
        }
    }
}
