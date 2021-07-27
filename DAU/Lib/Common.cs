using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentArchiveUtility.Lib
{
    public static class Common
    {
        public static IEnumerable<FileInfo> GetFilesBetween(string path, DateTime start, DateTime end)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();
            return files.Where(f => f.CreationTime.Between(start, end) ||
                                    f.LastWriteTime.Between(start, end));
        }

        public static IEnumerable<DirectoryInfo> GetFoldersBetween(string path, DateTime start, DateTime end)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] folders = di.GetDirectories();
            return folders.Where(f => f.CreationTime.Between(start, end) ||
                                    f.LastWriteTime.Between(start, end));
        }


        public static IEnumerable<DirectoryInfo> GetFoldersOlderThanXDays(string path, int olderThanLastXDays)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] folders = di.GetDirectories();
            return folders.Where(f => f.CreationTime < DateTime.Now.AddDays(olderThanLastXDays) ||
                                    f.LastWriteTime < DateTime.Now.AddDays(olderThanLastXDays));
        }

        public static bool IsZipValid(string path)
        {
            try
            {
                using (var zipFile = ZipFile.OpenRead(path))
                {
                    var entries = zipFile.Entries;
                    return true;
                }
            }
            catch (InvalidDataException ex)
            {
                Log.Error("Zip File is Invalid"+ path, ex.InnerException);
                return false;
            }
        }
             
    }
}
