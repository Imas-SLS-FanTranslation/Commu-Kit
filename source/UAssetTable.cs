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

        public void ReadCsv(string inFilename) => Messages = MessageList.ReadCsv(inFilename);

        public void ReadJson(string inFilename) => Messages = MessageList.ReadJson(inFilename);

        public void ReplaceText()
        {
            var csvLookup = messages.Records.ToDictionary((entry) => entry.Identifier);
            foreach (var entry in Data)
            {
                if (entry.Value[1] is StrPropertyData strData)
                {
                    strData.Value = new FString(csvLookup[entry.Name.ToString()].Target);
                }
            }
        }

        public void WriteUAssetToFile(string outputPath)
        {
            openFile.Write(outputPath);
        }

        public void WriteCsv(string outFilename) => Messages.WriteCsv(outFilename);

        public void WriteJson(string outFilename) => Messages.WriteJson(outFilename);

        private MessageList Messages
        {
            get
            {
                if (messages is null)
                {
                    messages = GetTrimmedData();
                }
                return messages;
            }
            set => messages = value;
        }

        private MessageList GetTrimmedData()
        {
            return new MessageList(Data.Select((entry) => new CSVClass(entry)));
        }

        private MessageList messages = null;

        private const UE4Version UNREAL_VERSION = UE4Version.VER_UE4_24;
        private readonly UAsset openFile;
        private readonly DataTable table;
    }
}