namespace ParserService.ParserCore.Models
{
    public class PriceType
    {
        public int Id { get; set; }
        public int DepartureCountryId { get; set; }
        public int DepartureRegionId { get; set; }
        public int ArrivalCountryId { get; set; }
        public int ArrivalRegionId { get; set; }
        public int HotelId { get; set; }
    }
}
