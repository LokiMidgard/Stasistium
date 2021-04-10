using System.IO;

namespace Stasistium.Helper
{
    public static class Delete
    {
        public static void Readonly(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            string[]? files;
            try
            {
                files = Directory.GetFiles(directoryPath);

            }
            catch (System.Exception e)
            {
                System.Console.Error.WriteLine(e);
                files = System.Array.Empty<string>();
            }
            string[]? directories;
            try
            {
                directories = Directory.GetDirectories(directoryPath);

            }
            catch (System.Exception e)
            {
                System.Console.Error.WriteLine(e);
                directories = System.Array.Empty<string>();
            }
            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (var dir in directories)
            {
                Readonly(dir);
            }

            File.SetAttributes(directoryPath, FileAttributes.Normal);

            Directory.Delete(directoryPath, false);
        }

    }
}
