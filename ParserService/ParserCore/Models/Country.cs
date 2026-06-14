namespace ParserService.ParserCore.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FrontendId { get; set; } = string.Empty;
        public int OperatorId { get; set; }
    }
}
