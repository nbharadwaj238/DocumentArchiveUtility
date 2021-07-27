using DocumentArchiveUtility.Interfaces;
using DocumentArchiveUtility.Lib;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace DocumentArchiveUtility.Services
{
    public class CompressionService : ICompressionService
    {
        public Task<int> CreateArchive(string folderFullPath, IList<string> exceptions, string archiveName)
        {
            int filesCount = 0;
            try
            {
                Log.Information("CompressionService: CreateArchive service starts.");
                string archivePath = Path.Combine(folderFullPath, archiveName);
                if (File.Exists(archivePath))
                {
                    Log.Information("CompressionService: CreateArchive" + archivePath + "Alredy Exist.");
                    if (!Common.IsZipValid(archivePath))
                    {
                        File.Delete(archivePath); // If zip file is not valid, delete the file.
                        Log.Information(archivePath + " is Invalid zip file.");
                        Log.Information("Delete invalid Zip " + archivePath);
                    }
                    else
                    {
                        //Abort the operation - as valid zip is already in place.
                        Log.Information("Valid Zip is already in place " + archivePath);
                        return Task.FromResult(filesCount); //Abort the operation.
                    }
                }
                IEnumerable<string> files = Directory.EnumerateFiles(folderFullPath,
                        "*.*", SearchOption.AllDirectories);

                using (ZipArchive archive = ZipFile.Open(archivePath, ZipArchiveMode.Create))
                {
                    foreach (string file in files)
                    {
                        if (!Excluded(file, exceptions))
                        {
                            try
                            {
                                var addFile = Path.GetFullPath(file);
                                if (addFile != archivePath)
                                {
                                    addFile = addFile.Substring(folderFullPath.Length + 1);
                                    Console.WriteLine("Adding " + addFile);
                                    Log.Information("CompressionService: CreateArchive :Adding " + addFile);
                                    archive.CreateEntryFromFile(file, addFile);
                                    filesCount++;
                                }
                            }
                            catch (IOException ex)
                            {
                                Log.Fatal(@"Failed to add {0} due to error:{1} \n Ignoring it!", file, ex.Message);
                                Console.WriteLine(@"Failed to add {0} due to error : 
                            {1} \n Ignoring it!", file, ex.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(@"Error in CreateArchive Service", ex.InnerException);
            }
            return Task.FromResult(filesCount);
        }
        public Task<int> ExtractToDirectory(string filepath, string directoryName)
        {
            int filesCount = 0;
            try
            {
                Log.Information("CompressionService: ExtractToDirectory service starts.");
                if (File.Exists(filepath))
                {
                    Log.Information("CompressionService: ExtractToDirectory" + filepath + "Alredy Exist.");
                    if (!Common.IsZipValid(filepath))
                    {
                        File.Delete(filepath); // If zip file is not valid, delete the file.
                        Log.Information(filepath + " is Invalid zip file.");
                        Log.Information("Delete invalid Zip " + filepath);
                    }
                    else
                    {
                        //Abort the operation - as valid zip is already in place.
                        Log.Information("Valid Zip is already in place " + filepath);
                        return Task.FromResult(filesCount); //Abort the operation.
                    }
                }
                try
                {
                    ZipFile.ExtractToDirectory(filepath, directoryName);
                }
                catch (Exception ex)
                {
                    Log.Fatal(@"Error in ExtractToDirectory Service", ex.InnerException);
                }
            }
            catch (IOException ex)
            {
                Log.Fatal(@"Failed to add {0} due to error:{1} \n Ignoring it!", filepath, ex.Message);
                Console.WriteLine(@"Failed to add {0} due to error : 
                            {1} \n Ignoring it!", filepath, ex.Message);
            }
            return Task.FromResult(filesCount);
        }

        public static bool Excluded(string file, IList<string> exceptions)
        {
            try
            {
                List<String> folderNames = (from folder in exceptions
                                            where folder.StartsWith(@"\")
                                                || folder.StartsWith(@"/")
                                            select folder).ToList<string>();
                if (!exceptions.Contains(Path.GetExtension(file)))
                {
                    foreach (string folderException in folderNames)
                    {
                        if (Path.GetDirectoryName(file).Contains(folderException))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(@"Error in Excluded Service", ex.InnerException);
            }
            return true;
        }
    }
}




