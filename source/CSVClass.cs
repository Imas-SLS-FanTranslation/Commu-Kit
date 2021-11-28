using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Newtonsoft.Json;
using System.ComponentModel;
using UAssetAPI.StructTypes;

namespace Commu_Kit
{
    public class CSVClass
    {
        public CSVClass() // default constructor needed for CSV reading
        {
        }

        public CSVClass(StructPropertyData entry)
        {
            Identifier = entry.Name.ToString();
            CharacterName = entry.Value[0].RawValue?.ToString() ?? "";
            Source = entry.Value[1].RawValue?.ToString() ?? "";
            Target = "";
        }

        [Name("id")]
        public string Identifier { get; set; }

        [Name("character")]
        public string CharacterName { get; set; }

        [Name("source")]
        public string Source { get; set; }

        [Name("translatedstr")]
        public string Target { get; set; }

        [Name("translatornotes")]
        [Optional]
        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Notes { get; set; } = "";
    }
    public class LineBreakConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            return text.Replace("\r\n", "\n");
        }
    }

    public sealed class CsvClassMap : ClassMap<CSVClass>
    {
        public CsvClassMap(bool useNotes)
        {
            AutoMap(System.Globalization.CultureInfo.InvariantCulture);
            Map(m => m.Source).TypeConverter<LineBreakConverter>();
            Map(m => m.Target).TypeConverter<LineBreakConverter>();
            Map(m => m.Notes).TypeConverter<LineBreakConverter>();
            if (!useNotes)
            {
                Map(m => m.Notes).Ignore();
            }
        }
    }
}