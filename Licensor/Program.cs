namespace Licensor
{
    using System;
    using System.IO;
    using System.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ApplicationException("Did not see an argument for the directory to apply licenses onto");
            }

            Directory.EnumerateFiles(args.First(), "*", SearchOption.AllDirectories)
                .GroupBy(Path.GetExtension)
                .ToList()
                .ForEach(k => Console.WriteLine(k.Key, k));
        }
    }
}
