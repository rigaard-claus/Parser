namespace ParserService.Data.Entities
{
    public class OperatorEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public long Priority { get; set; }
        public ICollection<CountryEntity> Countries { get; set; } = new List<CountryEntity>();
    }
}
