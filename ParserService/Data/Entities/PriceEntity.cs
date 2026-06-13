namespace ParserService.Data.Entities
{
    public class PriceEntity
    {
        public long Id { get; set; }

        public int PriceTypeId { get; set; }
        public PriceTypeEntity PriceType { get; set; } = null!;

        public int PlacementId { get; set; }
        public PlacementEntity Placement { get; set; } = null!;

        public int CurrencyId { get; set; }
        public CurrencyEntity Currency { get; set; } = null!;

        public DateTime Date { get; set; }
        public int Nights { get; set; }
        public decimal Price { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
