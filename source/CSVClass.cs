using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
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
        public string Notes { get; set; }
    }

    public sealed class CsvClassMap : ClassMap<CSVClass>
    {
        public CsvClassMap(bool useNotes)
        {
            AutoMap(System.Globalization.CultureInfo.InvariantCulture);
            if (!useNotes)
            {
                Map(m => m.Notes).Ignore();
            }
        }
    }
}