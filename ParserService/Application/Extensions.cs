namespace ParserService.Application
{
    public static class Extensions
    {
        public static class NatsSubjectBuilder
        {
            public static string GetSubject<T>()
            {
                var type = typeof(T);
                string fullName = type.FullName ?? type.Name;
                return fullName.Replace(".", "_").ToLower();
            }
        }
    }
}
