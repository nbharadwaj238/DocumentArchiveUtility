using System;
using System.IO;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Logging.EventLog;
using Serilog;
using Serilog.Settings.Configuration;



namespace DocumentArchiveUtility
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Information("Started : Main method");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)

                .ConfigureAppConfiguration((context, config) =>
                {
                    if (context.HostingEnvironment.IsProduction())
                    {
                        Log.Information("Production Environemnt : Managed Identity");
                        //Managed identity
                        var builtConfig = config.Build();
                        var secretClient = new SecretClient(
                            new Uri($"https://{builtConfig["KeyVault:VaultName"]}.vault.azure.net/"),
                            new DefaultAzureCredential());
                        config.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                        Log.Information("Production Environemnt : Managed Identity : Authentication Successfully");
                    }
                    if (context.HostingEnvironment.IsDevelopment()) // different providers in dev
                    {
                        Log.Information("Development Environemnt : Managed Identity");
                        config.AddUserSecrets<Program>();
                    }

                })
                //  .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    Log.Information("Call  : Startup class");
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseSetting("detailedErrors", "true");
                    webBuilder.UseStartup<Startup>();
                }).UseWindowsService();

    }
}
