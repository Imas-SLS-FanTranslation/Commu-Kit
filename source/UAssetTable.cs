using System.Collections.Generic;
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

        public IEnumerable<CSVClass> GetTrimmedData()
        {
            return Data.Select((entry) => new CSVClass(entry));
        }

        public void ApplyTrimmedData(IEnumerable<CSVClass> trimmedData)
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

        public void WriteUAssetToFile(string outputPath)
        {
            openFile.Write(outputPath);
        }

        private const UE4Version UNREAL_VERSION = UE4Version.VER_UE4_24;
        private readonly UAsset openFile;
        private readonly DataTable table;
    }
}