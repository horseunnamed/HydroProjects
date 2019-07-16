using CommandLine;

namespace EfficiencyDiffChecker
{
    class Program
    {
        private class Options
        {
            [Option("floodmap1", Required = true)]
            public string Floodmap1Path { get; set; }

            [Option("floodmap2", Required = true)]
            public string Floodmap2Path { get; set; }

            [Option("output", Required = true)]
            public string OutputDir { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(o =>
            {
                
            });
        }
    }
}
