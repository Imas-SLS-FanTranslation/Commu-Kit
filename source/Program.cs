using CommandLine;
using System;
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

        [Verb("import", HelpText = "Patches .uasset files with translation from .csv or .json files")]
        private class ImportClass
        {
            [Value(0, HelpText = "Input file path (.csv or .json)", MetaName = "CsvInputPath", Required = true)]
            public string CsvInputPath { get; set; }

            [Value(1, HelpText = "Input .uasset file path", MetaName = "UassetInputPath", Required = true)]
            public string UassetInputPath { get; set; }

            [Option('o', HelpText = "output path")]
            public string OutputPath { get; set; }
        }

        [Verb("convert", HelpText = "Converts between .csv and .json files")]
        private class ConvertClass
        {
            [Value(0, HelpText = "Input file path", Required = true)]
            public string InputPath { get; set; }

            [Value(1, HelpText = "Output file path", Required = true)]
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
            if (outputFile.EndsWith(".csv"))
            {
                table.WriteCsv(outputFile);
            }
            else if (outputFile.EndsWith(".json"))
            {
                table.WriteJson(outputFile);
            }
            else
            {
                throw new ArgumentException("Unrecognised file extension. Please specify a .csv or .json file name.");
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
            if (filePath.EndsWith(".csv"))
            {
                table.ReadCsv(filePath);
            }
            else if (filePath.EndsWith(".json"))
            {
                table.ReadJson(filePath);
            }
            else
            {
                throw new ArgumentException("Unrecognised file extension. Please specify a .csv or .json file.");
            }
            table.ReplaceText();
            table.WriteUAssetToFile(outputFile);
        }

        private static void ConvertFile(ConvertClass opt)
        {
            string inPath = opt.InputPath;
            string outPath = opt.OutputPath;
            if (inPath.EndsWith(".csv") && outPath.EndsWith(".json"))
            {
                MessageList.ReadCsv(inPath).WriteJson(outPath);
            }
            else if (inPath.EndsWith(".json") && outPath.EndsWith(".csv"))
            {
                MessageList.ReadJson(inPath).WriteCsv(outPath);
            }
            else
            {
                throw new ArgumentException("Incorrect file extensions. Expected a .csv file and a .json file.");
            }
        }

        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Parser.Default.ParseArguments<ExportClass, ImportClass, ConvertClass>(args)
                .WithParsed<ExportClass>(ExportFile)
                .WithParsed<ImportClass>(ImportFile)
                .WithParsed<ConvertClass>(ConvertFile);
        }
    }
};