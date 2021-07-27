using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault.Keys;
using System.Threading.Tasks;

namespace DocumentArchiveUtility.Interfaces
{
    public interface IKeyVaultSecretClientClientFactory
    {
        public SecretClient SecretClient { get; }
        public KeyClient KeyClient { get; }
               
    }

}