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
    }
}
