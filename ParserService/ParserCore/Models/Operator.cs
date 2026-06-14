namespace ParserService.ParserCore.Models
{
    public class Operator
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long Priority { get; set; }
    }
}
