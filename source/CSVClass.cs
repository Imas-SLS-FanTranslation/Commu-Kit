using System;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

namespace Commu_Kit
{
    public class CSVClass
    {
        public CSVClass(string identifier, string characterName, string source, string target)
        {
            Identifier = identifier;
            CharacterName = characterName;
            Source = source;
            Target = target;
        }
        [Name("id")]
        public String Identifier { get; set; }
        [Name("character")]
        public String CharacterName{ get; set; }
        [Name("source")]
        public String Source{ get; set; }
        [Name("translatedstr")]
        public String Target{ get; set; }
    }

    public class CsvMap : ClassMap<CSVClass>
    {
        public CsvMap()
        {
            Map(m => m.Identifier).Name("id");
            Map(m => m.CharacterName).Name("character");
            Map(m => m.Source).Name("src");
            Map(m => m.Target).Name("translatedstr");



        }
    }
}