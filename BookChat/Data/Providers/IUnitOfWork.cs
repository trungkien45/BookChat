namespace BookChat.Data.Providers
{
    public interface IUnitOfWork
    {
        Task ExecuteInTransactionAsync(Func<Task> action);
        Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
    }
}
