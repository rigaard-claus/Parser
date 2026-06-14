namespace ParserService.ParserCore.Models
{
    public class Price
    {
        public long Id { get; set; }
        public int PriceTypeId { get; set; }
        public int PlacementId { get; set; }
        public int CurrencyId { get; set; }
        public DateTime Date { get; set; }
        public int Nights { get; set; }
        public decimal PriceValue { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
