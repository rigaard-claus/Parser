namespace ParserService.Application.Messaging.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NatsSubjectAttribute(string subject) : Attribute
    {
        public string Subject { get; } = subject;
    }
}
