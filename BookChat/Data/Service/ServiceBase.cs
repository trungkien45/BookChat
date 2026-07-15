using BookChat.Data.Providers;

namespace BookChat.Data.Service
{
    public abstract class ServiceBase
    {
        private readonly IDbSessionFactory _factory;

        protected ServiceBase(IDbSessionFactory factory)
        {
            _factory = factory;
        }

        protected async Task<TResult> ExecuteAsync<TResult>(
            Func<IDbSession, Task<TResult>> action)
        {
            await using IDbSession db = await _factory.CreateAsync();
            return await action(db);
        }
        protected async Task ExecuteAsync(
            Func<IDbSession, Task> action)
        {
            await using IDbSession db = await _factory.CreateAsync();
            await action(db);
        }
    }
}
