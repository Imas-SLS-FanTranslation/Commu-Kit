using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UAssetAPI;
using UAssetAPI.PropertyTypes;

namespace Commu_Kit
{
    internal class Program
    {
        private const UE4Version UNREAL_VERSION = UE4Version.VER_UE4_24;

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

            UAsset openFile = new UAsset(filePath, UNREAL_VERSION);
            var Table = (openFile.Exports[0] as DataTableExport)?.Table;
            if (Table is null)
            {
                throw new InvalidDataException("Could not access data table. Please check you also have the corresponding .uexp file.");
            }
            var quoteIndexes = new int[] { 3, 4 };
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                ShouldQuote = args => quoteIndexes.Contains(args.Row.Index)
            };
            using (var csvExporter = new CsvWriter(new StreamWriter(outputFile), CultureInfo.InvariantCulture))
            {
                var records = Table.Data.Select((entry) => new CSVClass(entry));
                csvExporter.WriteRecords(records);
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
            UAsset openFile = new UAsset(opt.UassetInputPath, UNREAL_VERSION);
            var Table = (openFile.Exports[0] as DataTableExport)?.Table;
            if (Table is null)
            {
                throw new InvalidDataException("Could not access data table. Please check you also have the corresponding .uexp file.");
            }
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                MissingFieldFound = null
            };
            using (var csv = new CsvReader(new StreamReader(filePath, Encoding.UTF8), csvConfig))
            {
                var csvLookup = csv.GetRecords<CSVClass>().ToDictionary((entry) => entry.Identifier);
                foreach (var entry in Table.Data)
                {
                    if (entry.Value[1] is StrPropertyData strData)
                    {
                        strData.Value = new FString(csvLookup[entry.Name.ToString()].Target.Replace("\r\n", "\n"));
                    }
                }
            }
            openFile.Write(outputFile);
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