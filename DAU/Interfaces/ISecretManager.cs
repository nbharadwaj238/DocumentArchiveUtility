using Azure.Core;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace DocumentArchiveUtility.Interfaces
{
    public interface ISecretManager
    {
        Task<string> GetSecretAsync(string secretName);
        Task SetSecretAsync(string secretName, string secretValue);
        Task DeleteSecret(string secretName);
        Task UpdateSecret(string secretName, string secretValue);
        Task<KeyVaultKey> GetKeyAsync(string keyName, string version);
        string GetKey(string keyName, string version);
        TokenCredential GetTokenCredential();
    }
}