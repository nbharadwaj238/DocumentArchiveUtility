using System.Threading.Tasks;
using DocumentArchiveUtility.Interfaces;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DocumentArchiveUtility.Services
{

    public class SecretManager : ISecretManager
    {
        protected readonly IKeyVaultSecretClientClientFactory _keyVaultSecretClientClientFactory;
        private readonly SecretClient _secretClient;
        private readonly KeyClient _keyClient;

        public SecretManager(IKeyVaultSecretClientClientFactory keyVaultSecretClientClientFactory)
        {
            _keyVaultSecretClientClientFactory = keyVaultSecretClientClientFactory;
            _secretClient = _keyVaultSecretClientClientFactory.SecretClient;
            _keyClient = keyVaultSecretClientClientFactory.KeyClient;
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            if (secret != null)
            {
                return secret.Value;
            }

            else
            {
                return string.Empty;
            }
        }

        public async Task SetSecretAsync(string secretName, string secretValue)
        {
            await _secretClient.SetSecretAsync(secretName, secretValue);
        }

        public async Task DeleteSecret(string secretName)
        {
            DeleteSecretOperation operation = await _secretClient.StartDeleteSecretAsync(secretName);
        }

        public async Task UpdateSecret(string secretName, string secretValue)
        {
            await SetSecretAsync(secretName, secretValue);
        }

        public async Task<KeyVaultKey> GetKeyAsync(string keyName, string version)
        {
            KeyVaultKey key = await _keyClient.GetKeyAsync(keyName, version);
            if (key != null)
            {
                return key;
            }

            else
            {
                return null;
            }
        }

        public string GetKey(string keyName, string version)
        {
            KeyVaultKey key = _keyClient.GetKey(keyName, version);
            if (key != null)
            {
                return key.Name;
            }

            else
            {
                return string.Empty;
            }
        }

        public TokenCredential GetTokenCredential()
        {

            TokenCredential credential = new DefaultAzureCredential();
#if DEBUG
            Log.Information("Get Credentials for Development Environment");
            var clientId = GetSecretAsync("ClientId").Result;
            credential = new ClientSecretCredential(GetSecretAsync("TenantId").Result,
                                                    GetSecretAsync("ClientId").Result,
                                                    GetSecretAsync("ClientSecret").Result);
            Log.Information("Return Credentials for Development Environment");
#endif
            return credential;
        }
    }
}

