using System.Threading;
using System.Threading.Tasks;

namespace DocumentArchiveUtility.Interfaces
{
    public interface IBlobService
    {
        Task DoWork(CancellationToken cancellationToken);
        public Task<bool> UploadDirectoryAndSubFolder();
        public Task<bool> UploadFiles();
        public Task<bool> DownloadArchiveCatalog(string directoryPath, string applicationNumber);

    }

}