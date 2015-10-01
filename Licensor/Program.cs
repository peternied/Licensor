namespace Licensor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Licensor.Properties;

    internal class Program
    {
        private static readonly IDictionary<string, string> licensesByExtension = new Dictionary<string, string>
        {
            { ".cs", Resources.CLicenseFormat },
            { ".java", Resources.CLicenseFormat },
            { ".js", Resources.JSLicenseFormat },
        };

        private readonly static string[] _startsWithBlacklist = { "jquery", "knockout", "modernizr", "_references", "AssemblyInfo" };

        private static readonly string[] _folderBlacklist = { "Generated" };

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ApplicationException("Did not see an argument for the directory to apply licenses onto");
            }

            var filesToLicense = Directory.EnumerateFiles(args.First(), "*", SearchOption.AllDirectories)
                .GroupBy(Path.GetExtension)
                .Where(group => licensesByExtension.Keys.Contains(group.Key))
                .SelectMany(group => group)
                .Where(file => !_startsWithBlacklist.Any(Path.GetFileName(file).StartsWith))
                .Where(file => !IsUnderRestrictedFolder(file));

            var filesToApplyLicense = filesToLicense
                .Where(file => !ValidLicenseHeader(file, licensesByExtension[Path.GetExtension(file)]));

            filesToApplyLicense
                .AsParallel()
                .ForAll(file => ApplyLicenseHeader(file, licensesByExtension[Path.GetExtension(file)]));
        }

        private static void ApplyLicenseHeader(string file, string licenseFormat)
        {
            var license = string.Format(licenseFormat, DateTime.Now.Year);
            string extension = Path.GetExtension(file);
            string firstValidLine;
            switch (extension)
            {
                case ".java":
                    firstValidLine = "package";
                    break;
                case ".cs":
                    firstValidLine = "namespace";
                    break;
                default:
                    firstValidLine = string.Empty;
                    break;
            }

            string[] fileContents = File.ReadAllLines(file);
            StringBuilder licenseFreeFile = new StringBuilder();
            fileContents
                .SkipWhile(line => !line.StartsWith(firstValidLine))
                .ToList().ForEach(line => licenseFreeFile.AppendLine(line));

            File.Delete(file);
            using (TextWriter writer = new StreamWriter(File.OpenWrite(file)))
            {
                writer.WriteLine(license);
                writer.WriteLine();
                writer.Write(licenseFreeFile.ToString());
            }
        }

        private static bool ValidLicenseHeader(string file, string licenseFormat)
        {
            const int linesToBuffer = 25;
            StringBuilder buffered = new StringBuilder();

            using (StreamReader reader = new StreamReader(File.OpenRead(file)))
            {
                for (int i = 0; !reader.EndOfStream && i != linesToBuffer; i++)
                {
                    buffered.AppendLine(reader.ReadLine());
                }
            }

            if (buffered.ToString().Contains("auto-generated"))
            {
                return true;
            }

            var license = string.Format(licenseFormat, DateTime.Now.Year);
            return buffered.ToString().StartsWith(license);
        }

        public static bool IsUnderRestrictedFolder(string path)
        {
            var elements = path.Split(new[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var pathElement in elements)
            {
                foreach (var blackListedFolder in _folderBlacklist)
                {
                    if (pathElement.Equals(blackListedFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
