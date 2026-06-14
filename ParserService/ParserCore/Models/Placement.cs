namespace ParserService.ParserCore.Models
{
    public class Placement
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AdultsCount { get; set; }
        public int ChildrenCount { get; set; }
        public int OperatorId { get; set; }
    }
}
