namespace ParserService.Application
{
    public static class Extensions
    {
        public static string GetSubject<T>()
        {
            // Берем только имя класса, без длинного пути (Fullname)
            // Это избавит нас от проблем с точками в именах пространств
            string name = typeof(T).Name;

            // Убираем "Handler", если есть, чтобы топики были короткими
            if (name.EndsWith("Handler"))
                name = name.Substring(0, name.Length - "Handler".Length);

            // Возвращаем всегда простой формат: "handler.имя"
            return $"handler.{name.ToLower()}";
        }
    }
}
