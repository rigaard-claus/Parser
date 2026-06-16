namespace ParserService.Application.Messaging
{
    public interface ISubscriptionRegistrar
    {
        Task RegisterAllAsync(INatsBus bus, IServiceProvider sp);
    }
}
