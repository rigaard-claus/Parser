namespace ParserService.Application
{
    public static class Extensions
    {
        public static class NatsSubjectBuilder
        {
            public static string GetSubject<THandler>()
            {
                var name = typeof(THandler).Name.Replace("Handler", "");

                return name.ToLower();
            }
        }
    }
}
