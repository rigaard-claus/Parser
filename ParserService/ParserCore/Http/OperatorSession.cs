using System.Net;

namespace ParserService.ParserCore.Http
{
    public class OperatorSession
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public CookieContainer Cookies { get; set; } = new();
        public bool IsExpired => (DateTime.UtcNow - CreatedAt).TotalMinutes > 30;
    }
}
