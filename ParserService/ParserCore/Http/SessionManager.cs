using System.Collections.Concurrent;
using System.Net;

namespace ParserService.ParserCore.Http
{
    public static class SessionManager
    {
        private static readonly ConcurrentDictionary<string, OperatorSession> _sessions = new();

        public static void SaveSession(string operatorName, CookieContainer cookies)
            => _sessions[operatorName] = new OperatorSession { Cookies = cookies };

        public static CookieContainer? GetCookies(string operatorName)
        {
            if (_sessions.TryGetValue(operatorName, out var session) && !session.IsExpired)
                return session.Cookies;
            return null;
        }
    }
}
