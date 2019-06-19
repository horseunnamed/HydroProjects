using System;
using System.IO;

namespace Core
{
    public class Dir
    {
        public static string Data()
        {
            var solutionPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent?.Parent?.FullName;
            return Path.Combine(solutionPath ?? throw new InvalidOperationException(), "data");
        }

        public static string Data(string filePath)
        {
            return Path.Combine(Data(), filePath);
        }

        public static void RequireDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                var dirInfo = new DirectoryInfo(path);

                foreach (FileInfo file in dirInfo.GetFiles())
                {
                    file.Delete(); 
                }

                foreach (DirectoryInfo dir in dirInfo.GetDirectories())
                {
                    dir.Delete(true); 
                }
            }
            else
            {
                Directory.CreateDirectory(path);
            }
        }

    }
}
