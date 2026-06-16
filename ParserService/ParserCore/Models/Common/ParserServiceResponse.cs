namespace ParserService.ParserCore.Models.Common
{
    public abstract record ParserServiceResponse
    {
        public bool Success { get; set; } = true;
        public string? Error { get; set; }
    }
}
