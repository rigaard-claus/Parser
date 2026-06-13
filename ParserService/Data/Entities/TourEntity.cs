namespace ParserService.Data.Entities
{
    public class TourEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string FrontendId { get; set; } = null!;

        public int CountryId { get; set; }
        public CountryEntity Country { get; set; } = null!;

        public ICollection<RegionEntity> Regions { get; set; } = new List<RegionEntity>();
    }
}
