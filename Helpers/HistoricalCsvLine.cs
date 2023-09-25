namespace HemnetScraperService.Helpers
{
    public record HistoricalCsvLine
    {
        //Location,HomeStartingPrice,Package,Date,PackagePrice,TotalListings,NewListingsCount
        public string? Location { get; set; }
        public int? HomeStartingPrice { get; set; }
        public string? Package { get; set; }
        public DateTime? Date { get; set; }
        public int? PackagePrice { get; set; }
        public int? TotalListings { get; set; }
        public int? NewListingsCount { get; set; }
    }
}
