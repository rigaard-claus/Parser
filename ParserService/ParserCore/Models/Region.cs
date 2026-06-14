namespace ParserService.ParserCore.Models
{
    public class Region
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FrontendId { get; set; } = string.Empty;
        public int TourId { get; set; }
    }
}
