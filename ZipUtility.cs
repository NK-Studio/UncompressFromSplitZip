using System.IO;
using Unity.SharpZipLib.Zip;

namespace NKStudio.SharpZipLib.Utils
{
    public static class ZipUtility
    {
        /// <summary>
        /// Extract the split zip file.
        /// </summary>
        /// <param name="archivePath">The path to the zip file</param>
        /// <param name="outFolder">The output folder</param>
        public static void UncompressFromSplitZip(string archivePath,
            // string password,
            string outFolder)
        {
            // Create temporary folder
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempPath);

            var tempFile = Path.Combine(tempPath, "Temp.zip");

            try
            {
                // Merge all split files into one temporary file
                using (FileStream outputStream = File.Create(tempFile))
                {
                    var filePath = Path.GetDirectoryName(archivePath);
                    var fileName = Path.GetFileNameWithoutExtension(archivePath);
                    string[] files = Directory.GetFiles(filePath, $"{fileName}.z*");

                    foreach (string file in files)
                    {
                        using (FileStream inputStream = File.OpenRead(file))
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }
                }

                // Ensure the output directory exists
                if (!Directory.Exists(outFolder))
                {
                    Directory.CreateDirectory(outFolder);
                }

                // Extract the merged zip file
                using (ZipInputStream zipInputStream = new ZipInputStream(File.OpenRead(tempFile)))
                {
                    // TODO: Why!!! password is not working
                    // if (!string.IsNullOrEmpty(password))
                    // {
                    //     zipInputStream.Password = password; 
                    // }

                    ZipEntry entry;
                    while ((entry = zipInputStream.GetNextEntry()) != null)
                    {
                        string entryFilePath = Path.Combine(outFolder, entry.Name);

                        // Create directory if needed
                        if (entry.IsDirectory)
                        {
                            Directory.CreateDirectory(entryFilePath);
                        }
                        else
                        {
                            string directoryName = Path.GetDirectoryName(entryFilePath);
                            if (!string.IsNullOrEmpty(directoryName))
                            {
                                Directory.CreateDirectory(directoryName);
                            }

                            // Extract file
                            using (var fileStream = new FileStream(entryFilePath, FileMode.Create, FileAccess.Write))
                            {
                                byte[] buffer = new byte[4096];
                                int size;
                                while ((size = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    fileStream.Write(buffer, 0, size);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                // Remove temporary folder
                if (Directory.Exists(tempPath)) 
                    Directory.Delete(tempPath, true);
            }
        }
    }
}