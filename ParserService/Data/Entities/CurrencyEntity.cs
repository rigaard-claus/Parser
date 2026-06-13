namespace ParserService.Data.Entities
{
    public class CurrencyEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int OperatorId { get; set; }
        public OperatorEntity Operator { get; set; } = null!;
    }
}
