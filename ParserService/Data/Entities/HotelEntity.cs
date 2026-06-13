namespace ParserService.Data.Entities
{
    public class HotelEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string FrontendId { get; set; } = null!;
        public int RegionId { get; set; }
        public RegionEntity Region { get; set; } = null!;
    }
}
