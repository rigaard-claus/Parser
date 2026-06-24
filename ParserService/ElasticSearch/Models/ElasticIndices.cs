namespace ParserService.ElasticSearch.Models
{
    public static class ElasticIndices
    {
        public enum IndexData
        {
            Prices,
            Hotels,
            Regions,
            Tours,
            Countries
        }

        public static string GetName(this IndexData index) => index.ToString().ToLower();
    }
}
