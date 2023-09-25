using System.Text.Json.Serialization;

namespace HemnetScraperService.Scraper.DTO
{
    public class GraphQueryDTO
    {
        [JsonPropertyName("operationName")]
        public string? OperationName { get; set; }

        [JsonPropertyName("variables")]
        public Variables? Variables { get; set; }

        [JsonPropertyName("query")]
        public string? Query { get; set; }
    }

    public class Variables
    {
        [JsonPropertyName("searchInput")]
        public SearchInput? SearchInput { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("offset")]
        public int? Offset { get; set; }

        [JsonPropertyName("sort")]
        public string? Sort { get; set; }

        [JsonPropertyName("withBrokerTips")]
        public bool? WithBrokerTips { get; set; }

        [JsonPropertyName("withTopListing")]
        public bool? WithTopListing { get; set; }

        [JsonPropertyName("askingPrice")]
        public int? AskingPrice { get; set; }

        [JsonPropertyName("locationId")]
        public string? LocationId { get; set; }

        [JsonPropertyName("productCodes")]
        public string[]? ProductCodes { get; set; }
    }

    public class SearchInput
    {
        [JsonPropertyName("balcony")]
        public string? Balcony { get; set; }

        [JsonPropertyName("biddingStarted")]
        public string? BiddingStarted { get; set; }

        [JsonPropertyName("elevator")]
        public string? Elevator { get; set; }

        [JsonPropertyName("foreclosure")]
        public string? Foreclosure { get; set; }

        [JsonPropertyName("liveStream")]
        public string? LiveStream { get; set; }

        [JsonPropertyName("locationIds")]
        public List<string>? LocationIds { get; set; }

        [JsonPropertyName("newConstruction")]
        public string? NewConstruction { get; set; }

        [JsonPropertyName("owned")]
        public string? Owned { get; set; }

        [JsonPropertyName("publishedSince")]
        public string? PublishedSince { get; set; }
    }
}
