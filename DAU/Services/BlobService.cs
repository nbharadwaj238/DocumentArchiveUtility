using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Cryptography;
using Azure.Identity;
using Azure.Security.KeyVault.Keys;
using Azure.Security.KeyVault.Keys.Cryptography;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DocumentArchiveUtility.Interfaces;
using DocumentArchiveUtility.Models;
using Serilog;
using DocumentArchiveUtility.Lib;
using System.Text.RegularExpressions;

namespace DocumentArchiveUtility.Services
{
    public class BlobService : IBlobService
    {
        private readonly ILogger<BlobService> _logger;
        private readonly IOptions<MyConfig> _config;
        private readonly IConfiguration _configuration;
        private readonly ISecretManager _secretManager;

        public BlobService(ILogger<BlobService> logger, IOptions<MyConfig> config, IConfiguration custom_config, ISecretManager secretManager)
        {
            _logger = logger;
            _config = config;
            _configuration = custom_config;
            _secretManager = secretManager;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            Log.Information("BlobService:DoWork BlobService is working");
            _logger.LogInformation($"{DateTime.Now:hh:mm:ss} BlobService is working.");
            await Task.Delay(1000 * 20, cancellationToken);
        }

        //Use this method to upload directory & sub folders to archieve tier.
        public async Task<bool> UploadDirectoryAndSubFolder()
        {
            try
            {
                Log.Information("BlobService:UploadDirectoryAndSubFolder Started");
                KeyVaultKey rasKey = await _secretManager.GetKeyAsync(_configuration["KeyVault:KeyName"], _configuration["KeyVault:KeyVersion"]);

                var cred = _secretManager.GetTokenCredential();
                IKeyEncryptionKey key = new CryptographyClient(rasKey.Id, cred);
                IKeyEncryptionKeyResolver keyResolver = new KeyResolver(cred);

                ClientSideEncryptionOptions encryptionOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
                {
                    KeyEncryptionKey = key,
                    KeyResolver = keyResolver,
                    KeyWrapAlgorithm = "RSA1_5"
                };

                var con = await _secretManager.GetSecretAsync("CustDocCon");

                BlobClientOptions options = new SpecializedBlobClientOptions() { ClientSideEncryption = encryptionOptions };
                var storageAccount = new BlobServiceClient(con, options);
                var container = storageAccount.GetBlobContainerClient(_config.Value.Container);

                var directoryPath = _configuration["Directory:Path"];
                var uploadDir1 = new DirectoryInfo(directoryPath);
                var baseDirLength = uploadDir1.FullName.Length + 1;

                container.CreateIfNotExists();

                // IEnumerable<DirectoryInfo> directories = Common.GetFoldersOlderThanXDays(directoryPath, -4);
                var directories = uploadDir1.EnumerateDirectories("*", SearchOption.TopDirectoryOnly).Where(x => x.CreationTime < DateTime.Now.AddDays(Convert.ToDouble(_configuration["Services:Archived:OlderThan"])) ||
                                        x.LastWriteTime < DateTime.Now.AddDays(Convert.ToDouble(_configuration["Services:Archived:OlderThan"])));

                foreach (var directory in directories)
                {
                    var files = directory.EnumerateFiles("*", SearchOption.AllDirectories);

                    foreach (var file in files)
                    {
                        string partialName = file.FullName.Substring(baseDirLength);
                        Console.WriteLine("Writing {0}", partialName);
                        var blockBlob = container.GetBlobClient(partialName);

                        //If file is not alredy exists in blob & have valid zip file then upload.
                        if (!await blockBlob.ExistsAsync() && Common.IsZipValid(file.FullName))
                        {
                            Log.Information("BlobService: UploadDirectoryAndSubFolder Upload " + partialName);
                            using (var fileStream = file.OpenRead())
                            {
                                var uploadOptions = new BlobUploadOptions()
                                { AccessTier = AccessTier.Cool };
                                await blockBlob.UploadAsync(fileStream, uploadOptions);
                            }
                        }
                        else
                        {
                            Log.Information("BlobService:UploadDirectoryAndSubFolder -: Delete file from directory " + file.Name);
                            file.Delete();
                            //Delete directory if it is empty;
                            directory.Delete();

                            var dir = new DirectoryInfo("Z:\\Nitish\\Utility\\PolicyBazaar\\Documents\\FlexiOTPushedDocs");

                            foreach (var tifFile in dir.EnumerateFiles(partialName + "*.tif"))
                            {
                                tifFile.Delete();
                            }
                        }
                    }
                }

                Log.Information("BlobService:UploadDirectoryAndSubFolder Completed");


            }
            catch (Exception ex)
            {
                Log.Fatal("BlobService:UploadDirectoryAndSubFolder", ex.InnerException);
            }
            return true;
        }


        public async Task<bool> UploadFiles()
        {
            try
            {
                Log.Information("BlobService:UploadFiles Started");
                KeyVaultKey rasKey = await _secretManager.GetKeyAsync(_configuration["KeyVault:KeyName"], _configuration["KeyVault:KeyVersion"]);
                var cred = _secretManager.GetTokenCredential();

                IKeyEncryptionKey key = new CryptographyClient(rasKey.Id, cred);
                IKeyEncryptionKeyResolver keyResolver = new KeyResolver(cred);

                ClientSideEncryptionOptions encryptionOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
                {
                    KeyEncryptionKey = key,
                    KeyResolver = keyResolver,
                    KeyWrapAlgorithm = "RSA1_5"
                };

                BlobClientOptions options = new SpecializedBlobClientOptions() { ClientSideEncryption = encryptionOptions };

                var con = await _secretManager.GetSecretAsync("CustDocCon");
                var storageAccount = new BlobServiceClient(con, options);
                var container = storageAccount.GetBlobContainerClient(_config.Value.Container);

                var directoryPath = _configuration["Directory:Path"];
                var uploadDir1 = new DirectoryInfo(directoryPath);
                var baseDirLength = uploadDir1.FullName.Length;

                container.CreateIfNotExists();

                // IEnumerable<DirectoryInfo> directories = Common.GetFoldersOlderThanXDays(directoryPath, -4);
                var files = uploadDir1.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(x => x.CreationTime < DateTime.Now.AddDays(Convert.ToDouble(_configuration["Services:Archived:OlderThan"])) ||
                                        x.LastWriteTime < DateTime.Now.AddDays(Convert.ToDouble(_configuration["Services:Archived:OlderThan"]))).Where(x => x.Extension == ".zip");

                foreach (var file in files)
                {
                    string partialName = file.FullName.Substring(baseDirLength);
                    Console.WriteLine("Writing {0}", partialName);
                    var blockBlob = container.GetBlobClient(partialName);

                    if (!await blockBlob.ExistsAsync())
                    {
                        Log.Information("BlobService: UploadFiles Upload" + partialName);
                        if (Common.IsZipValid(file.FullName))
                        {
                            using (var fileStream = file.OpenRead())
                            {
                                var uploadOptions = new BlobUploadOptions()
                                { AccessTier = AccessTier.Cool };
                                await blockBlob.UploadAsync(fileStream, uploadOptions);
                            }
                        }
                    }
                    else
                    {
                        if (Directory.Exists(Path.ChangeExtension(file.FullName, null)))
                        {
                            //Delete the main directory and files as Zip folder is already in place.
                            Log.Information("BlobService:UploadFiles : Delete the main directory and files as Zip folder is already in place" + file.FullName);
                            Directory.Delete(Path.ChangeExtension(file.FullName, null), true); // Delete the main directory
                           
                        }
                       
                        file.Delete(); //Delete the zip file
                        Log.Information("BlobService:UploadFiles : Deleted the Zip file " + file.FullName);

                        var FlexiOTPushedDocs = _configuration["OtherDirectory:Path"];
                        var dir = new DirectoryInfo(FlexiOTPushedDocs);
                        var fileName = Path.ChangeExtension(partialName, null);
                        var tifFiles = dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Where(x => x.Name.Contains(fileName));

                        foreach (var tifFile in tifFiles)
                        {
                            string tifFileName = tifFile.Name;
                            if (Regex.IsMatch(tifFileName, fileName))
                            {
                                tifFile.Delete();
                                Log.Information("Deleted : file " + fileName);
                            }
                        }
                    }

                    Log.Information("BlobService:UploadFiles Completed");
                }
            }
            catch (Exception ex)
            {
                Log.Fatal("BlobService: UploadFiles ", ex.InnerException);
            }


            return true;
        }


        public async Task<bool> DownloadArchiveCatalog(string directoryPath, string applicationNumber)
        {
            var cred = _secretManager.GetTokenCredential();
            KeyVaultKey rasKey = await _secretManager.GetKeyAsync(_configuration["KeyVault:KeyName"], _configuration["KeyVault:KeyVersion"]);

            IKeyEncryptionKey key = new CryptographyClient(rasKey.Id, cred);
            IKeyEncryptionKeyResolver keyResolver = new KeyResolver(cred);

            ClientSideEncryptionOptions encryptionOptions = new ClientSideEncryptionOptions(ClientSideEncryptionVersion.V1_0)
            {
                KeyEncryptionKey = key,
                KeyResolver = keyResolver,
                KeyWrapAlgorithm = "RSA1_5"
            };

            var con = await _secretManager.GetSecretAsync("CustDocCon");
            BlobClientOptions options = new SpecializedBlobClientOptions() { ClientSideEncryption = encryptionOptions };
            var storageAccount = new BlobServiceClient(con, options);
            var container = storageAccount.GetBlobContainerClient(_config.Value.Container);

            //string partialName = "F11036129\\F11036126_Paymentacknowledgement_CJ.pdf";

            string partialName = applicationNumber + ".zip";
            var downloadDirectory = directoryPath + ".zip";

            var blockBlob = container.GetBlobClient(partialName);
            try
            {
                using (var fileStream = System.IO.File.OpenWrite(downloadDirectory))
                {
                    blockBlob.DownloadTo(fileStream);
                }
                Log.Information(applicationNumber + "Downloaded from Blob");
            }
            catch (Exception ex)
            {
                Log.Error("Getting Error while downloading " + applicationNumber + " from Blob", ex.InnerException);
            }

            return true;
        }
    }
}

