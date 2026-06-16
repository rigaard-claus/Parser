namespace ParserService.Application.Handlers
{
    public interface INatsHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request);
    }
}
