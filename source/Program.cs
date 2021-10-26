using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UAssetAPI;
using UAssetAPI.StructTypes;
using CommandLine;
using CsvHelper;
using CsvHelper.Configuration;
using UAssetAPI.PropertyTypes;


namespace Commu_Kit
{
    class Program
    {
        const UE4Version UNREAL_VERSION = UE4Version.VER_UE4_24;
       
        [Verb("export", HelpText = "Extracts commu text from .uasset files")]
        class ExportClass
        {
            [Value(0, HelpText = "Input file path", MetaName = "FilePath", Required = true)]
            public string FilePath { get; set; }
            [Option('o', HelpText = "output path")]
            public string OutputPath { get; set; }
        }
        [Verb("import", HelpText = "Patches .uasset files with translation from .csv files")]
        class ImportClass
        {
            [Value(0, HelpText = "Input .csv file path", MetaName = "CsvInputPath", Required = true)]
            public string CsvInputPath { get; set; }
            [Value(1, HelpText = "Input .uasset file path", MetaName = "UassetInputPath", Required = true)]
            public string UassetInputPath { get; set; }
            
            [Option('o', HelpText = "output path")]
            public string OutputPath { get; set; }
        }

        public static string GetStringByName(IEnumerable<CSVClass> ienum, string name)
        {
            return ienum.First(c => c.Identifier == name).Target;
        }
        private static int ExportFile(ExportClass opt)
        {
         
            string filePath = opt.FilePath;
            string outputFile = opt.OutputPath;
            if (String.IsNullOrEmpty(outputFile))
            {
                outputFile = Path.GetDirectoryName(filePath) + @"\" + Path.GetFileNameWithoutExtension(filePath) + ".csv";
                Console.WriteLine(outputFile);
            }

            UAsset openFile = new UAsset(filePath, UNREAL_VERSION);
            var DataTable = openFile.Exports[0] as DataTableExport;
            var Table = DataTable?.Table;
            var indexes = new[] {3, 4};
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                ShouldQuote = args => indexes.Contains(args.Row.Index)
            };
            using (var strWriter = new StreamWriter(outputFile))
            using (var csvExporter = new CsvWriter(strWriter, CultureInfo.InvariantCulture))
            {
                csvExporter.WriteHeader<CSVClass>();
                csvExporter.NextRecord();
                for (int i = 0; i < Table.Data.Count; i++)
                {
                    StructPropertyData Entry = Table.Data[i];
                    var Data = Entry.Value[1];
                    var SpeakerData = Entry.Value[0];
                    var csvRow = new CSVClass(Entry.Name.ToString(), SpeakerData.RawValue + "", Data.RawValue + "", "");
                    csvExporter.WriteRecord(csvRow);
                    csvExporter.NextRecord();
                  //  Console.WriteLine(
                      //  Entry.Name + "," + SpeakerData.RawValue + "," + "\"" + Data.RawValue + "\"" + ", ");
                }
            }
            Console.WriteLine("Written to:" + outputFile);
            
            return 0;
        }

        private static int ImportFile(ImportClass opt)
        {
           
            string filePath = opt.CsvInputPath;
            if (File.Exists(filePath))
            {
              //  Console.WriteLine(filePath);
            }
            else
            {
         //       Console.WriteLine("FileNotFound");
            }
            string outputFile = opt.OutputPath;
            if (String.IsNullOrEmpty(outputFile))
            {
                outputFile = Path.GetDirectoryName(filePath) + @"\" +  Path.GetFileNameWithoutExtension(filePath) + "-NEW.uasset";
            }
            UAsset openFile = new UAsset(opt.UassetInputPath, UNREAL_VERSION);
            var DataTable = openFile.Exports[0] as DataTableExport;
            var Table = DataTable?.Table;
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = false
            };
            using(var reader = new StreamReader(filePath, Encoding.UTF8))
            using (var csv = new CsvReader(reader, csvConfig))
            {
                var records = csv.GetRecords<CSVClass>();
                for (int i = 0; i < Table.Data.Count; i++)
                {
                    StructPropertyData Entry = Table.Data[i];
                    var Data = Entry.Value[1];
                    //Console.WriteLine(Entry.Name.ToString());
                    if (Data is StrPropertyData strData) strData.Value = new FString(GetStringByName(records,Entry.Name.ToString()));

                }
            }
            
            openFile.Write(outputFile);

            return 0;
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding= Encoding.Unicode;
            var parseArgs = Parser.Default.ParseArguments<ExportClass, ImportClass>(args).MapResult(
                (ExportClass opt) => ExportFile(opt),
                (ImportClass opt) => ImportFile(opt),
                errs => 1
            );
        }



    }
};
