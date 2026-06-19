namespace ParserService.ParserCore.Models
{
    public class OperatorOptions
    {
        public int OperatorId { get; set; } = 0;
        public string OperatorName { get; set; }
        public int Priority { get; set; }
        public string BaseUrl { get; set; }
        public string HomePage { get; set; }
        public string DepartureReferenceUrl { get; set; }
        public string CountryReferenceUrl { get; set; }
        public string RegionReferenceUrl { get; set; }
        public string HotelReferenceUrl { get; set; }
        public string Referer { get; set; }
        public string DataUrl { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> RawHeaders { get; set; } = new();
    }
}
