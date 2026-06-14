namespace ParserService.ParserCore.Models
{
    public class Direction
    {
        public int Id { get; set; }
        public int DepartureRegionId { get; set; }
        public int ArrivalRegionId { get; set; }
        public bool IsActive { get; set; }
    }
}
