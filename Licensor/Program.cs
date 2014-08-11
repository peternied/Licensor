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

        private readonly static string[] _startsWithBlacklist = { "jquery", "knockout", "modernizr", "_references",  };

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
                .Where(file => !_startsWithBlacklist.Any(Path.GetFileName(file).StartsWith));

            var filesToApplyLicense = filesToLicense
                .Where(file => !ValidLicenseHeader(file, licensesByExtension[Path.GetExtension(file)]));

            filesToApplyLicense
                .AsParallel()
                .ForAll(file => ApplyLicenseHeader(file, licensesByExtension[Path.GetExtension(file)]));
        }

        private static void ApplyLicenseHeader(string file, string licenseFormat)
        {
            var license = string.Format(licenseFormat, DateTime.Now.Year);
            string fileContents = File.ReadAllText(file);
            using (TextWriter writer = new StreamWriter(File.OpenWrite(file)))
            {
                writer.WriteLine(license);
                writer.WriteLine();
                writer.Write(fileContents);
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

            var license = string.Format(licenseFormat, DateTime.Now.Year);
            return buffered.ToString().StartsWith(license);
        }
    }
}
