namespace ParserService.Data.Entities
{
    public class DirectionEntity
    {
        public int Id { get; set; }

        public int DepartureRegionId { get; set; }
        public RegionEntity DepartureRegion { get; set; } = null!;

        public int ArrivalRegionId { get; set; }
        public RegionEntity ArrivalRegion { get; set; } = null!;

        public bool IsActive { get; set; }
    }
}
