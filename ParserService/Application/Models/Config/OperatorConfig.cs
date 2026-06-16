namespace ParserService.Application.Models.Config
{
    public class OperatorConfig
    {
        public string OperatorName { get; set; }
        public string BaseUrl { get; set; }
        public string ReferencesUrl { get; set; }
        public string DataUrl { get; set; }
        // Добавляем Priority, который сейчас будет по умолчанию 1
        public int Priority { get; set; } = 1;
    }
}
