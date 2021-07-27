using Azure.Security.KeyVault.Secrets;
using System.Threading.Tasks;
using DocumentArchiveUtility.Interfaces;
using Azure.Security.KeyVault.Keys;
using Azure.Core.Cryptography;
using Azure.Security.KeyVault.Keys.Cryptography;

namespace DocumentArchiveUtility.Models
{
    public class KeyVaultSecretClientClientFactory : IKeyVaultSecretClientClientFactory
    {
        public KeyVaultSecretClientClientFactory(SecretClient secretClient, KeyClient keyClient)
        {
            SecretClient = secretClient;
            KeyClient = keyClient;
        
        }
        public SecretClient SecretClient { get; }
        public KeyClient KeyClient { get; }
        
    }

}

