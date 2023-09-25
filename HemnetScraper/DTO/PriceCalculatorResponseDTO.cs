using System.Text.Json.Serialization;

namespace HemnetScraperService.Scraper.DTO
{
    public class PriceCalculatorResponseDTO
    {
        [JsonPropertyName("data")]
        public PriceCalculatorDataRoot? Data { get; set; }
    }

    public class PriceCalculatorDataRoot
    {
        [JsonPropertyName("sellerMarketingProductPrices")]
        public SellerMarketingProductPrices? SellerMarketingProductPrices { get; set; }
    }

    public class PriceValue
    {
        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("__typename")]
        public string? TypeName { get; set; }
    }

    public class Price
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("price")]
        public PriceValue? PriceValue { get; set; }

        [JsonPropertyName("__typename")]
        public string? TypeName { get; set; }
    }

    public class SellerMarketingProductPrices
    {
        [JsonPropertyName("formattedValidThrough")]
        public string? FormattedValidThrough { get; set; }

        [JsonPropertyName("prices")]
        public List<Price>? Prices { get; set; }

        [JsonPropertyName("eligibleForPayLater")]
        public bool? EligibleForPayLater { get; set; }

        [JsonPropertyName("__typename")]
        public string? Typename { get; set; }
    }
}
