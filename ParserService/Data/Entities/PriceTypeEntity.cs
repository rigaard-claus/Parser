namespace ParserService.Data.Entities
{
    public class PriceTypeEntity
    {
        public int Id { get; set; }

        public int DepartureCountryId { get; set; }
        public CountryEntity DepartureCountry { get; set; } = null!;

        public int DepartureRegionId { get; set; }
        public RegionEntity DepartureRegion { get; set; } = null!;

        public int ArrivalCountryId { get; set; }
        public CountryEntity ArrivalCountry { get; set; } = null!;

        public int ArrivalRegionId { get; set; }
        public RegionEntity ArrivalRegion { get; set; } = null!;

        public int HotelId { get; set; }
        public HotelEntity Hotel { get; set; } = null!;
    }
}
