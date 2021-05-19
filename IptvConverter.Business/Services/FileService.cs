using System.IO;

namespace IptvConverter.Business.Services
{
    public class FileService
    {
        private static void ensureUploadFolderExists(string uploadFolderPath)
        {
            if (!Directory.Exists(uploadFolderPath))
            {
                Directory.CreateDirectory(uploadFolderPath);
            }
        }

        public static string GetEpgFolderPath(bool isDeveopment, string contentRootPath)
        {
            var folder = isDeveopment
                ? Path.Combine(contentRootPath, "bin", "Debug", "net5.0", "sources")
                : Path.Combine(contentRootPath, "sources");

            ensureUploadFolderExists(folder);
            return folder;
        }


    }
}
