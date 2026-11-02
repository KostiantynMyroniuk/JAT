namespace JAT.API.Common.Abstractions
{
    public interface IHandler<TRequest, TResponse>
    {
        Task<TResponse> HandleAsync(TRequest request, CancellationToken ct = default);
    }
}
