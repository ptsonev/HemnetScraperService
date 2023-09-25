using HemnetScraperService.HemnetScraperModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HemnetScraperService.Helpers
{
    public static class ConfigHelper
    {
        public static string GenerateJSON()
        {
            List<HemnetLocation> inputLocations = new()
            {
                new HemnetLocation { LocationName = "Stockholm", LocationId = "18031" },
                new HemnetLocation { LocationName = "Göteborg", LocationId = "17920" },
                new HemnetLocation { LocationName = "Malmö", LocationId = "17989" },
                new HemnetLocation { LocationName = "Uppsala", LocationId = "17800" },
                new HemnetLocation { LocationName = "Upplands Väsby", LocationId = "17798" },
                new HemnetLocation { LocationName = "Västerås", LocationId = "17821" },
                new HemnetLocation { LocationName = "Örebro", LocationId = "17838" },
                new HemnetLocation { LocationName = "Linköping", LocationId = "17847" },
                new HemnetLocation { LocationName = "Helsingborg", LocationId = "17932" },
                new HemnetLocation { LocationName = "Jönköping", LocationId = "17952" },
            };

            //List<HemnetLocation> inputLocations = new()
            //{
            //    new HemnetLocation { LocationName = "Stockholm", LocationId = "17744" },
            //    new HemnetLocation { LocationName = "Uppsala", LocationId = "17745" },
            //    new HemnetLocation { LocationName = "Södermanland", LocationId = "17746" },
            //    new HemnetLocation { LocationName = "Östergötland", LocationId = "17747" },
            //    new HemnetLocation { LocationName = "Jönköping", LocationId = "17748" },
            //    new HemnetLocation { LocationName = "Kronoberg", LocationId = "17749" },
            //    new HemnetLocation { LocationName = "Kalmar", LocationId = "17750" },
            //    new HemnetLocation { LocationName = "Gotland", LocationId = "17751" },
            //    new HemnetLocation { LocationName = "Blekinge", LocationId = "17752" },
            //    new HemnetLocation { LocationName = "Skåne", LocationId = "17753" },
            //    new HemnetLocation { LocationName = "Halland", LocationId = "17754" },
            //    new HemnetLocation { LocationName = "Västra Götaland", LocationId = "17755" },
            //    new HemnetLocation { LocationName = "Värmland", LocationId = "17756" },
            //    new HemnetLocation { LocationName = "Örebro", LocationId = "17757" },
            //    new HemnetLocation { LocationName = "Västmanland", LocationId = "17758" },
            //    new HemnetLocation { LocationName = "Dalarna", LocationId = "17759" },
            //    new HemnetLocation { LocationName = "Gävleborg", LocationId = "17760" },
            //    new HemnetLocation { LocationName = "Västernorrland", LocationId = "17761" },
            //    new HemnetLocation { LocationName = "Jämtland", LocationId = "17762" },
            //    new HemnetLocation { LocationName = "Västerbotten", LocationId = "17763" },
            //    new HemnetLocation { LocationName = "Norrbotten", LocationId = "17764" },
            //};

            JsonSerializerOptions options = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
            };

            return JsonSerializer.Serialize(inputLocations, options);
        }
    }
}
