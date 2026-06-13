namespace ParserService.Data.Entities
{
    public class CountryEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string FrontendId { get; set; } = null!;
        public int OperatorId { get; set; }
        public OperatorEntity Operator { get; set; } = null!;
        public ICollection<TourEntity> Tours { get; set; } = new List<TourEntity>();
    }
}
