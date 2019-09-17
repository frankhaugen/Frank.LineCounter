using ConsoleTableExt;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using Console = Colorful.Console;

namespace Frank.LineCounter
{
    public static class Program
    {
        private static DirectoryInfo _currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        private static List<FileInfo> _files = new List<FileInfo>();
        private static Stopwatch _stopwatch = new Stopwatch();

        public static void Main(string[] paths)
        {
            _stopwatch.Start();
            Console.WriteAscii("Frank's Line-Counter", ColorTranslator.FromHtml("#8AFFEF"));
            if (paths.Any() && !string.IsNullOrWhiteSpace(paths[0]))
            {
                try
                {
                    _currentDirectory = new DirectoryInfo(paths[0]);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            _files = _currentDirectory.EnumerateFiles("*.cs", SearchOption.AllDirectories).Where(f => f.DirectoryName != null && !f.DirectoryName.StartsWith('.') || f.DirectoryName.Contains("Migrations")).ToList();

            var fileData = GetFileData();
            if (fileData.Any())
            {
                Console.Out.WriteLine($"Number of C# -files:\t{fileData.Count}");
                Console.Out.WriteLine($"Total number of text-lines:\t{fileData.Select(fd => fd.TextLineCount).Sum()}");
                Console.Out.WriteLine($"Total number of code-lines:\t{fileData.Select(fd => fd.CodeLineCount).Sum()}");
                Console.Out.WriteLine($"Average number of text-lines:\t{fileData.Select(fd => fd.TextLineCount).Sum() / fileData.Count}");
                Console.Out.WriteLine($"Average number of code-lines:\t{fileData.Select(fd => fd.CodeLineCount).Sum() / fileData.Count}");
                Console.Out.WriteLine($"Total number of file breaking the 150-rule:\t{fileData.Count(data => data.TextLineCount > 150)}");
                Console.Out.WriteLine($"Total number of file breaking the 50-rule:\t{fileData.Count(data => data.CodeLineCount > 50)}");
                Console.Out.WriteLine($"Total number of file breaking both rules:\t{fileData.Count(data => data.CodeLineCount > 50 && data.TextLineCount > 150)}");

                ConsoleTableBuilder
                    .From(fileData.Where(fd => fd.CodeLineCount > 50 || fd.TextLineCount > 150).ToList())
                    .WithFormat(ConsoleTableBuilderFormat.Minimal)
                    .ExportAndWriteLine();
            }
            _stopwatch.Stop();

            Console.WriteLine($"Time used: {_stopwatch.ElapsedMilliseconds} Milliseconds");

            Console.WriteLine("Finished...");
            Console.ReadLine();
        }

        private static List<FileData> GetFileData()
        {
            return _files.Select(fi => new FileData()
            {
                Name = fi.Name,
                FullName = fi.FullName,
                TextLineCount = File.ReadLines(fi.FullName).Count(),
                CodeLineCount = File.ReadAllText(fi.FullName).Count(cha => cha == char.Parse(";"))
            }).ToList();
        }
    }

    internal class FileData
    {
        public string Name { get; set; }
        public string FullName { get; set; }
        public int TextLineCount { get; set; }
        public int CodeLineCount { get; set; }
    }
}
