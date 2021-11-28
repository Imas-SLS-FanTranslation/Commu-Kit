using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Commu_Kit
{
    internal class MessageList
    {
        public MessageList(IEnumerable<CSVClass> records)
        {
            Records = records.ToList();
        }

        public static MessageList ReadCsv(string inFilename)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                MissingFieldFound = null
            };
            using var csv = new CsvReader(new StreamReader(inFilename, System.Text.Encoding.UTF8), csvConfig);
            csv.Context.RegisterClassMap(new CsvClassMap(useNotes: true));
            return new MessageList(csv.GetRecords<CSVClass>());
        }

        public static MessageList ReadJson(string inFilename)
        {
            using var reader = new JsonTextReader(File.OpenText(inFilename));
            var serializer = new JsonSerializer();
            return new MessageList(serializer.Deserialize<IEnumerable<CSVClass>>(reader));
        }

        public void WriteCsv(string outFilename)
        {
            using var csvExporter = new CsvWriter(new StreamWriter(outFilename), CultureInfo.InvariantCulture);
            csvExporter.Context.RegisterClassMap(new CsvClassMap(
                useNotes: Records.Any(record => !string.IsNullOrWhiteSpace(record.Notes))
                ));
            csvExporter.WriteRecords(Records);
        }

        public void WriteJson(string outFilename)
        {
            using var writer = new JsonTextWriter(File.CreateText(outFilename));
            writer.Formatting = Formatting.Indented;
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, Records);
        }

        public IEnumerable<CSVClass> Records { get; }
    }
}