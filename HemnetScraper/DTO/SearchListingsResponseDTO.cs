using System.Text.Json.Serialization;

namespace HemnetScraperService.Scraper.DTO
{
    public class SearchListingsResponseDTO
    {
        [JsonPropertyName("data")]
        public SearchListingsDataRoot? Data { get; set; }
    }

    public class SearchListingsDataRoot
    {
        [JsonPropertyName("searchForSaleListings")]
        public SearchForSaleListings? SearchForSaleListings { get; set; }
    }

    public class SearchForSaleListings
    {
        [JsonPropertyName("total")]
        public int? Total { get; set; }

        [JsonPropertyName("limit")]
        public int? Limit { get; set; }

        [JsonPropertyName("offset")]
        public int? Offset { get; set; }

        [JsonPropertyName("listings")]
        public List<Listing>? Listings { get; set; }
    }

    public class AskingPrice
    {
        [JsonPropertyName("amountInCents")]
        public string? AmountInCents { get; set; }

        [JsonPropertyName("formatted")]
        public string? Formatted { get; set; }
    }

    public class Listing : IEquatable<Listing?>
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("publishedAt")]
        public string? PublishedAt { get; set; }

        [JsonPropertyName("activePackage")]
        public string? ActivePackage { get; set; }

        [JsonPropertyName("askingPrice")]
        public AskingPrice? AskingPrice { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Listing);
        }

        public bool Equals(Listing? other)
        {
            return other is not null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(Listing? left, Listing? right)
        {
            return EqualityComparer<Listing>.Default.Equals(left, right);
        }

        public static bool operator !=(Listing? left, Listing? right)
        {
            return !(left == right);
        }
    }
}
