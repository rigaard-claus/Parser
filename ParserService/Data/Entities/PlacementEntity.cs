namespace ParserService.Data.Entities
{
    public class PlacementEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int AdultsCount { get; set; }
        public int ChildrenCount { get; set; }

        public int OperatorId { get; set; }
        public OperatorEntity Operator { get; set; } = null!;
    }
}
