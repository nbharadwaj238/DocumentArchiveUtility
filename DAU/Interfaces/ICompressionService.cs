using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DocumentArchiveUtility.Interfaces
{
    public interface ICompressionService
    {

        public Task<int> CreateArchive(string folderFullPath,
                IList<string> exceptions, string archiveName);
    }
}