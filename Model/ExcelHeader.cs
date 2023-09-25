namespace HemnetScraperService.Model
{
    public record ExcelHeader
    {
        public string Row1Column { get; set; } = string.Empty;
        public string Row2Column { get; set; } = string.Empty;
        public string Row3Column { get; set; } = string.Empty;
    }
}