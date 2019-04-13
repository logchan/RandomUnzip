using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using CommandLine;

namespace RandomUnzip
{
    class Program
    {
        private static readonly Random _rng = new Random();

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;

            new Parser(settings =>
                {
                    settings.AutoHelp = true;
                    settings.CaseSensitive = false;
                    settings.HelpWriter = Console.Out;
                })
                .ParseArguments<Option>(args)
                .WithParsed(Run);
        }

        static void Run(Option options)
        {
            var encoding = Encoding.GetEncoding(options.Encoding ?? "UTF-8");
            Directory.CreateDirectory(options.OutputPath);

            using (var fs = File.OpenRead(options.InputFile))
            using (var archive = new ZipArchive(fs, ZipArchiveMode.Read, false, encoding))
            {
                var fileEntryIdx = (from idx in Enumerable.Range(0, archive.Entries.Count)
                    where !String.IsNullOrEmpty(archive.Entries[idx].Name)
                    select idx).ToList();
                var numEntries = fileEntryIdx.Count;

                if (options.Take <= 0)
                {
                    options.Take = numEntries;
                }
                options.Take = Math.Min(numEntries, options.Take);

                Console.WriteLine($"Taking {options.Take} files out of {numEntries} files...");
                foreach (var idx in GetRandomEntries(fileEntryIdx, options.Take))
                {
                    try
                    {
                        var entry = archive.Entries[idx];
                        if (String.IsNullOrEmpty(entry.Name))
                        {
                            continue;
                        }

                        Console.Write($"{entry.Name}");

                        var file = Path.Combine(options.OutputPath, entry.FullName);
                        var dir = new FileInfo(file).Directory.FullName;
                        Directory.CreateDirectory(dir);

                        using (var ofs = File.Open(file, FileMode.Create, FileAccess.Write))
                        using (var es = entry.Open())
                        {
                            es.CopyTo(ofs);
                        }
                        Console.WriteLine("\tok");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\tfailed: {ex.Message}");
                    }
                }
            }
        }

        static IEnumerable<int> GetRandomEntries(List<int> fileEntryIdx, int take)
        {
            for (var i = 0; i < take; ++i)
            {
                var j = _rng.Next(i, fileEntryIdx.Count);
                var t = fileEntryIdx[j];
                fileEntryIdx[j] = fileEntryIdx[i];
                fileEntryIdx[i] = t;

                yield return fileEntryIdx[i];
            }
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper disable once ClassNeverInstantiated.Local
        private class Option
        {
            [Option('i', "in", Required = true)]
            public string InputFile { get; set; }

            [Option('o', "out", Required = true)]
            public string OutputPath { get; set; }

            [Option('e', "encoding")]
            public string Encoding { get; set; }

            [Option('t', "take")]
            public int Take { get; set; }
        }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
    }
}
