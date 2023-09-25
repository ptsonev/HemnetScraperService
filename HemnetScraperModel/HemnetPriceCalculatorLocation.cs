namespace HemnetScraperService.HemnetScraperModel
{
    public record HemnetPriceCalculatorLocation : HemnetLocation
    {
        public int? HomeStartingPrice { get; set; }
        public Dictionary<string, int?>? PackagePrices { get; set; } = new();
    }
}
