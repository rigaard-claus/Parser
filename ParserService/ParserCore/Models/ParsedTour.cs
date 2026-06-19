namespace ParserService.ParserCore.Models
{
    public class ParsedTour
    {
        public string OperatorName { get; set; } = string.Empty;
        public long OperatorId { get; set; } = 0;
        public string DepartureCountry { get; set; } = string.Empty;
        public string DepartureCountryFrontendId { get; set; } = string.Empty;
        public string DepartureRegion { get; set; } = string.Empty;
        public string DepartureRegionFrontendId { get; set; } = string.Empty;
        public string ArrivalCountry { get; set; } = string.Empty;
        public string ArrivalCountryFrontendId { get; set; } = string.Empty;
        public string ArrivalTour { get; set; } = string.Empty;
        public string ArrivalTourFrontendId { get; set; } = string.Empty;
        public string ArrivalRegion { get; set; } = string.Empty;
        public string ArrivalRegionFrontendId { get; set; } = string.Empty;
        public string HotelName { get; set; } = string.Empty;
        public string HotelFrontendId { get; set; } = string.Empty;
        public string PlacementName { get; set; } = "2AD";
        public string CurrencyCode { get; set; } = "EUR";
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public int Nights { get; set; }
    }
}
