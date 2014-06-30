namespace Licensor
{
    using System;
    using System.IO;
    using System.Linq;

    class Program
    {
        private readonly static string[] _supportedExtensions = { ".cs", ".java", ".js"};

        private readonly static string[] _startsWithBlacklist = { "jquery" };

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ApplicationException("Did not see an argument for the directory to apply licenses onto");
            }

            var filesToLicense = Directory.EnumerateFiles(args.First(), "*", SearchOption.AllDirectories)
                .GroupBy(Path.GetExtension)
                .Where(group => _supportedExtensions.Contains(group.Key))
                .SelectMany(group => group)
                .Where(file => !_startsWithBlacklist.Any(Path.GetFileName(file).StartsWith));

        }
    }
}
