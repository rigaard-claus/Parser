namespace ParserService.Data.Entities
{
    public class RegionEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string FrontendId { get; set; } = null!;

        public int TourId { get; set; }
        public TourEntity Tour { get; set; } = null!;

        public ICollection<HotelEntity> Hotels { get; set; } = new List<HotelEntity>();
    }
}
