using CsvHelper;
using CsvHelper.Configuration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UAssetAPI;
using UAssetAPI.PropertyTypes;
using UAssetAPI.StructTypes;

namespace Commu_Kit
{
    internal class UAssetTable
    {
        public List<StructPropertyData> Data => table.Data;

        public UAssetTable(string uAssetPath)
        {
            openFile = new UAsset(uAssetPath, UNREAL_VERSION);
            table = (openFile.Exports[0] as DataTableExport)?.Table;
            if (table is null)
            {
                throw new InvalidDataException("Could not access data table. Please check you also have the corresponding .uexp file.");
            }
        }

        public void ReadCsv(string inFilename)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                MissingFieldFound = null
            };
            using var csv = new CsvReader(new StreamReader(inFilename, System.Text.Encoding.UTF8), csvConfig);
            csv.Context.RegisterClassMap(new CsvClassMap(useNotes: true));
            ApplyTrimmedData(csv.GetRecords<CSVClass>());
        }

        public void WriteUAssetToFile(string outputPath)
        {
            openFile.Write(outputPath);
        }

        public void WriteCsv(string outFilename)
        {
            using var csvExporter = new CsvWriter(new StreamWriter(outFilename), CultureInfo.InvariantCulture);
            IEnumerable<CSVClass> records = GetTrimmedData();
            csvExporter.Context.RegisterClassMap(new CsvClassMap(useNotes: false));
            csvExporter.WriteRecords(records);
        }

        private IEnumerable<CSVClass> GetTrimmedData()
        {
            return Data.Select((entry) => new CSVClass(entry));
        }

        private void ApplyTrimmedData(IEnumerable<CSVClass> trimmedData)
        {
            var csvLookup = trimmedData.ToDictionary((entry) => entry.Identifier);
            foreach (var entry in Data)
            {
                if (entry.Value[1] is StrPropertyData strData)
                {
                    strData.Value = new FString(csvLookup[entry.Name.ToString()].Target.Replace("\r\n", "\n"));
                }
            }
        }

        private const UE4Version UNREAL_VERSION = UE4Version.VER_UE4_24;
        private readonly UAsset openFile;
        private readonly DataTable table;
    }
}