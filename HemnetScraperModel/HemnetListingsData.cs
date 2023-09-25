using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HemnetScraperService.HemnetScraperModel
{
    public record HemnetListingsData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? PK_ID { get; set; }
        public int? NewListingsCount { get; set; }
        public int? TotalListingsCount { get; set; }
        public double? AverageAskingPrice { get; set; }
        public Dictionary<string, int>? PackageCounter { get; set; }
    }
}
