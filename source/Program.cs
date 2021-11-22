using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Commu_Kit
{
    internal class Program
    {
        [Verb("export", HelpText = "Extracts commu text from .uasset files")]
        private class ExportClass
        {
            [Value(0, HelpText = "Input file path", MetaName = "FilePath", Required = true)]
            public string FilePath { get; set; }

            [Option('o', HelpText = "output path")]
            public string OutputPath { get; set; }
        }

        [Verb("import", HelpText = "Patches .uasset files with translation from .csv files")]
        private class ImportClass
        {
            [Value(0, HelpText = "Input .csv file path", MetaName = "CsvInputPath", Required = true)]
            public string CsvInputPath { get; set; }

            [Value(1, HelpText = "Input .uasset file path", MetaName = "UassetInputPath", Required = true)]
            public string UassetInputPath { get; set; }

            [Option('o', HelpText = "output path")]
            public string OutputPath { get; set; }
        }

        private static void ExportFile(ExportClass opt)
        {
            string filePath = opt.FilePath;
            string outputFile = opt.OutputPath;
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = $"{Path.GetDirectoryName(filePath)}\\{Path.GetFileNameWithoutExtension(filePath)}.csv";
                Console.WriteLine(outputFile);
            }
            var table = new UAssetTable(filePath);
            using (var csvExporter = new CsvWriter(new StreamWriter(outputFile), CultureInfo.InvariantCulture))
            {
                csvExporter.Context.RegisterClassMap(new CsvClassMap(useNotes: false));
                csvExporter.WriteRecords(table.GetTrimmedData());
            }
            Console.WriteLine("Written to:" + outputFile);
        }

        private static void ImportFile(ImportClass opt)
        {
            string filePath = opt.CsvInputPath;
            string outputFile = opt.OutputPath;
            if (string.IsNullOrEmpty(outputFile))
            {
                outputFile = $"{Path.GetDirectoryName(filePath)}\\{Path.GetFileNameWithoutExtension(filePath)}-NEW.uasset";
            }
            var table = new UAssetTable(opt.UassetInputPath);
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                MissingFieldFound = null
            };
            using (var csv = new CsvReader(new StreamReader(filePath, Encoding.UTF8), csvConfig))
            {
                csv.Context.RegisterClassMap(new CsvClassMap(useNotes: true));
                table.ApplyTrimmedData(csv.GetRecords<CSVClass>());
            }
            table.WriteUAssetToFile(outputFile);
        }

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Parser.Default.ParseArguments<ExportClass, ImportClass>(args)
                .WithParsed<ExportClass>(ExportFile)
                .WithParsed<ImportClass>(ImportFile);
        }
    }
};