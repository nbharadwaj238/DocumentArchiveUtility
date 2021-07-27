using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DocumentArchiveUtility.Services;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using DocumentArchiveUtility.Models;
using DocumentArchiveUtility.Interfaces;
using Azure.Security.KeyVault.Keys;
using Azure.Core.Cryptography;
using Azure.Security.KeyVault.Keys.Cryptography;
using Serilog;

namespace DocumentArchiveUtility.Lib
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterDependencies(this IServiceCollection services, IConfiguration configuration, ISecretManager secretManager)
        {
            var keyVaultSecretClientClientFactory = InitializeSecretClientInstanceAsync(configuration, secretManager);
            services.AddSingleton<IKeyVaultSecretClientClientFactory>(keyVaultSecretClientClientFactory);
            services.AddSingleton<ISecretManager, SecretManager>();
        }

        private static KeyVaultSecretClientClientFactory InitializeSecretClientInstanceAsync(IConfiguration configuration, ISecretManager secretManager)
        {
            Log.Information("Initialize Secret Client ");
            string keyVaultUrl = configuration["KeyVault:KvURL"];

            TokenCredential credential = new DefaultAzureCredential();
#if DEBUG
            Log.Information("Set Credentials for Development Environment");
            credential = new ClientSecretCredential(configuration["KeyVault:TenantId"], configuration["KeyVault:ClientId"], configuration["KeyVault:ClientSecret"]);
#endif

            var secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
            var keyClient = new KeyClient(new Uri(keyVaultUrl), credential);

            var keyVaultSecretClientClientFactory = new KeyVaultSecretClientClientFactory(secretClient, keyClient);
            return keyVaultSecretClientClientFactory;
        }

    }
}
