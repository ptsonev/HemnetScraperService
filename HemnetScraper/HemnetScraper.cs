using HemnetScraperService.HemnetScraperModel;
using HemnetScraperService.Scraper.DTO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HemnetScraperService.Scraper
{
    public class HemnetScraper
    {
        private readonly ILogger<HemnetScraperBackgroundService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IDbContextFactory<HemnetDbContext> _dbContextFactory;
        private readonly IConfiguration _configuration;
        private static readonly TimeZoneInfo _swedishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");

        public HemnetScraper(ILogger<HemnetScraperBackgroundService> logger, IHttpClientFactory httpClientFactory, IDbContextFactory<HemnetDbContext> dbContextFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;

            _jsonSerializerOptions = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns TimeSpan for the next update</returns>
        public async Task<TimeSpan> Scrape()
        {
            _logger.LogInformation("Starting the scraping loop.");

            _logger.LogInformation("Creating the Db Context");
            using var hemnetDatabaseContext = await _dbContextFactory.CreateDbContextAsync();
            var lastCompletedDate = hemnetDatabaseContext.ScraperTask.Max(p => p.MondayDateTime);

            var currentServerTime = GetCurrentServerTime();
            var (startDate, endDate) = CalculateWeekRange(currentServerTime);

            _logger.LogInformation("Server Time {ServerTime} | Local Time {LocalTime} | Start Date {StartDate} | Last Completed Date {LastCompletedDate}", currentServerTime, DateTime.Now, startDate, lastCompletedDate);

            if (!lastCompletedDate.HasValue || startDate.Date > lastCompletedDate)
            {
                var counties = _configuration.GetSection("AppSettings:Counties").Get<List<HemnetLocation>>();
                var cities = _configuration.GetSection("AppSettings:Cities").Get<List<HemnetLocation>>();
                var prices = _configuration.GetSection("AppSettings:Prices").Get<int?[]>();

                var hemnetPriceLocations = (from city in cities
                                            from price in prices!
                                            select new HemnetPriceCalculatorLocation()
                                            {
                                                LocationId = city.LocationId,
                                                LocationName = city.LocationName,
                                                HomeStartingPrice = price,
                                            }).ToList();

                _logger.LogInformation("Scraping price calculator: START");
                foreach (var hemnetPriceLocation in hemnetPriceLocations!)
                {
                    await CalculatePrice(hemnetPriceLocation);
                    await Task.Delay(1000, CancellationToken.None);
                }
                _logger.LogInformation("Scraping price calculator: END");

                _logger.LogInformation("Scraping listings: START");
                HashSet<Listing> listingsSet = new();
                foreach (var county in counties!)
                {
                    var currentResults = await ScrapeListings(county, startDate, endDate);
                    listingsSet.UnionWith(currentResults);
                }
                _logger.LogInformation("Scraping listings: END");

                _logger.LogInformation("Scraping total listings: START");
                int? totalListings = await GetTotalListings();
                _logger.LogInformation("Scraping total listings: END");

                HemnetScraperTask currentScraperTask = new()
                {
                    ScrapingDate = DateTime.Now,
                    MondayDateTime = startDate,
                    PriceCalculatorLocations = hemnetPriceLocations,
                    ListingData = new()
                    {
                        TotalListingsCount = totalListings,
                        NewListingsCount = listingsSet.Count,
                        PackageCounter = listingsSet.Where(p => !string.IsNullOrWhiteSpace(p.ActivePackage))
                                                    .GroupBy(p => p.ActivePackage!)
                                                    .ToDictionary(k => k.Key, v => v.Count()),
                        AverageAskingPrice = listingsSet.Select(p =>
                        {
                            if (double.TryParse(p.AskingPrice?.AmountInCents, out double parsedPrice))
                                return parsedPrice / 100.0;
                            return default(double?);

                        }).Average()
                    }
                };

                _logger.LogInformation("Saving the changes to the database");
                hemnetDatabaseContext.ScraperTask.Add(currentScraperTask);
                hemnetDatabaseContext.SaveChanges();
                
                currentServerTime = GetCurrentServerTime();
            }

            TimeSpan nextUpdateTimeSpan = endDate.AddDays(7) - currentServerTime;
            nextUpdateTimeSpan = nextUpdateTimeSpan <= TimeSpan.Zero ? TimeSpan.Zero : nextUpdateTimeSpan;
            nextUpdateTimeSpan += TimeSpan.FromSeconds(5);

            var nextUpdateLocalTime = DateTime.Now.Add(nextUpdateTimeSpan);
            var nextUpdateServerTime = currentServerTime.Add(nextUpdateTimeSpan);

            _logger.LogInformation("Next update will be at Local Time: {localTime} | Server Time: {serverTime}", nextUpdateLocalTime, nextUpdateServerTime);

            return nextUpdateTimeSpan;
        }

        public async Task<List<Listing>> ScrapeListings(HemnetLocation countyLocation, DateTime startDate, DateTime endDate)
        {
            List<Listing> resultListings = new();
            var httpClient = _httpClientFactory.CreateClient("AndroidHemnetClient");
            for (int offset = 0; offset <= 2450; offset += 50)
            {
                GraphQueryDTO countyPaginationQuery = new()
                {
                    Variables = new()
                    {
                        WithBrokerTips = false,
                        WithTopListing = false,
                        Sort = HemnetScraperConstants.SORT_NEWEST,
                        Limit = 50,
                        Offset = offset,
                        SearchInput = new()
                        {
                            Balcony = HemnetScraperConstants.INCLUDE,
                            BiddingStarted = HemnetScraperConstants.INCLUDE,
                            Elevator = HemnetScraperConstants.INCLUDE,
                            Foreclosure = HemnetScraperConstants.INCLUDE,
                            LiveStream = HemnetScraperConstants.INCLUDE,
                            NewConstruction = HemnetScraperConstants.INCLUDE,
                            Owned = HemnetScraperConstants.INCLUDE,
                            PublishedSince = "2w",
                            LocationIds = new() { countyLocation.LocationId! },
                        },
                    },
                    OperationName = HemnetScraperConstants.SEARCH_OPERATION,
                    Query = HemnetScraperConstants.SEARCH_QUERY
                };

                var response = await httpClient.PostAsJsonAsync(HemnetScraperConstants.GRAPH_URL, countyPaginationQuery, _jsonSerializerOptions);
                response.EnsureSuccessStatusCode();

                var result = (await response.Content.ReadFromJsonAsync<SearchListingsResponseDTO>())?.Data?.SearchForSaleListings;
                if (result is null)
                    throw new InvalidOperationException();

                foreach (var listing in result.Listings!)
                {
                    var publishedAtTimestamp = (long)double.Parse(listing.PublishedAt!);
                    var publishedAtDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(publishedAtTimestamp);
                    var publishedAtDateTime = TimeZoneInfo.ConvertTimeFromUtc(publishedAtDateTimeOffset.UtcDateTime, _swedishTimeZone);

                    if (publishedAtDateTime < startDate)
                        return resultListings;

                    if (publishedAtDateTime >= startDate && publishedAtDateTime < endDate)
                        resultListings.Add(listing);
                }

                if (result?.Offset >= result?.Total)
                    return resultListings;
            }

            return resultListings;
        }

        public async Task<int?> GetTotalListings()
        {
            var httpClient = _httpClientFactory.CreateClient("AndroidHemnetClient");
            GraphQueryDTO totalListingsQuery = new()
            {
                Variables = new()
                {
                    WithBrokerTips = false,
                    WithTopListing = false,
                    Sort = HemnetScraperConstants.SORT_NEWEST,
                    Limit = 50,
                    Offset = 0,
                    SearchInput = new()
                    {
                        Balcony = HemnetScraperConstants.INCLUDE,
                        BiddingStarted = HemnetScraperConstants.INCLUDE,
                        Elevator = HemnetScraperConstants.INCLUDE,
                        Foreclosure = HemnetScraperConstants.INCLUDE,
                        LiveStream = HemnetScraperConstants.INCLUDE,
                        NewConstruction = HemnetScraperConstants.INCLUDE,
                        Owned = HemnetScraperConstants.INCLUDE,
                        PublishedSince = "all",
                    },
                },
                OperationName = HemnetScraperConstants.TOTAL_LISTINGS_OPERATION,
                Query = HemnetScraperConstants.TOTAL_LISTINGS_QUERY
            };

            var response = await httpClient.PostAsJsonAsync(HemnetScraperConstants.GRAPH_URL, totalListingsQuery, _jsonSerializerOptions);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<SearchListingsResponseDTO>())?.Data?.SearchForSaleListings?.Total;
        }

        public async Task CalculatePrice(HemnetPriceCalculatorLocation inputLocation)
        {
            var packages = _configuration.GetSection("AppSettings:Packages").Get<string[]>();
            var httpClient = _httpClientFactory.CreateClient("DesktopHemnetClient");
            GraphQueryDTO calculatorQuery = new()
            {
                Variables = new()
                {
                    LocationId = inputLocation.LocationId,
                    AskingPrice = inputLocation.HomeStartingPrice,
                    ProductCodes = packages,
                },
                OperationName = HemnetScraperConstants.CALCULATOR_OPERATION,
                Query = HemnetScraperConstants.CALCULATOR_QUERY
            };

            var response = await httpClient.PostAsJsonAsync(HemnetScraperConstants.GRAPH_URL, calculatorQuery, _jsonSerializerOptions);
            response.EnsureSuccessStatusCode();

            var result = (await response.Content.ReadFromJsonAsync<PriceCalculatorResponseDTO>())?.Data?.SellerMarketingProductPrices?.Prices;
            if (result is null)
                throw new InvalidOperationException();

            int? basicPrice = result.FirstOrDefault(p => p.Code == "BASIC")?.PriceValue?.Amount;
            foreach (var package in result)
            {
                if (string.IsNullOrWhiteSpace(package.Code))
                    continue;

                int? currentValue = package?.PriceValue?.Amount;
                if (package!.Code != "BASIC" && basicPrice.HasValue)
                    currentValue += basicPrice;

                inputLocation.PackagePrices![package.Code] = currentValue;
            }
        }

        public static DateTime GetCurrentServerTime()
        {
            var httpClient = new HttpClient();
            HttpRequestMessage headRequest = new(HttpMethod.Head, "https://www.hemnet.se/");
            var response = httpClient.Send(headRequest);

            var serverDateTimeOffset = response.Headers.Date;
            serverDateTimeOffset ??= DateTimeOffset.UtcNow;

            var utcServerDateTime = serverDateTimeOffset.Value.UtcDateTime;
            var swedishTime = TimeZoneInfo.ConvertTimeFromUtc(utcServerDateTime, _swedishTimeZone);

            return swedishTime;
        }

        public static (DateTime, DateTime) CalculateWeekRange(DateTime currentTime)
        {
            int daysUntilMonday = ((int)currentTime.DayOfWeek - 1 + 7) % 7;
            var lastMondayDateTime = currentTime.AddDays(-daysUntilMonday - 7).Date;

            return (lastMondayDateTime, lastMondayDateTime.AddDays(7));
        }
    }
}
