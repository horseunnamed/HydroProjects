using System.IO;

namespace Core
{
    public class Dir
    {
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
