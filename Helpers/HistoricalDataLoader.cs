using CsvHelper;
using CsvHelper.Configuration;
using HemnetScraperService.HemnetScraperModel;
using System.Globalization;
using System.Text;

namespace HemnetScraperService.Helpers
{
    public static class HistoricalDataLoader
    {
        public static void Load(HemnetDbContext dbContext)
        {
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
            { };

            using var streamReader = new StreamReader("historical_data.csv", Encoding.UTF8);
            using CsvReader csvReader = new(streamReader, csvConfig);
            var historicalData = csvReader.GetRecords<HistoricalCsvLine>().Where(p => p.TotalListings.HasValue).ToList();
            foreach (var groupedByDate in historicalData.GroupBy(p => p.Date))
            {
                var currentTask = dbContext.ScraperTask.FirstOrDefault(p => p.MondayDateTime == groupedByDate.Key);
                if (currentTask == null)
                {
                    currentTask = new()
                    {
                        ScrapingDate = DateTime.Now,
                        MondayDateTime = groupedByDate.Key!.Value.Date,

                        ListingData = new()
                        {
                            AverageAskingPrice = null,
                            TotalListingsCount = groupedByDate.First().TotalListings,
                            NewListingsCount = groupedByDate.Last().NewListingsCount,
                        },
                        PriceCalculatorLocations = new()
                    };
                    dbContext.ScraperTask.Add(currentTask);
                }
                foreach (var packageGroup in groupedByDate.GroupBy(p => new { p.Location, p.HomeStartingPrice }))
                {
                    if (!currentTask.PriceCalculatorLocations!.Any(p => p.LocationName == packageGroup.Key.Location && p.HomeStartingPrice == packageGroup.Key.HomeStartingPrice))
                    {
                        HemnetPriceCalculatorLocation currentLocation = new()
                        {
                            HomeStartingPrice = packageGroup.Key.HomeStartingPrice,
                            LocationName = packageGroup.Key.Location,
                            PackagePrices = packageGroup.ToDictionary(k => k.Package!, v => v.PackagePrice)
                        };
                        currentTask.PriceCalculatorLocations!.Add(currentLocation);
                    }
                }
            }
            dbContext.SaveChanges();
        }
    }
}
