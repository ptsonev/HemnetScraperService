using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HemnetScraperService.HemnetScraperModel
{
    public record HemnetLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? PK_ID { get; set; }
        public string? LocationId { get; set; }
        public string? LocationName { get; set; }
    }
}
