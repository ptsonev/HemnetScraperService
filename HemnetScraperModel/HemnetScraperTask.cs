using System.ComponentModel.DataAnnotations;

namespace HemnetScraperService.HemnetScraperModel
{
    public record HemnetScraperTask
    {
        [Key]
        public DateTime? MondayDateTime { get; set; }
        public DateTime? ScrapingDate { get; set; }
        public HemnetListingsData? ListingData { get; set; }
        public List<HemnetPriceCalculatorLocation>? PriceCalculatorLocations { get; set; }
    }
}
