using SQLite;

namespace BookChat.Data.Providers
{
    public class SqliteUnitOfWork : IUnitOfWork
    {
        // SemaphoreSlim ensures that only one transaction uses the shared SQLiteAsyncConnection at a time, preventing conflicts caused by concurrent transactions on the same connection.
        // if multiple transactions are attempted simultaneously, they will be queued and executed one at a time, ensuring data integrity and preventing potential deadlocks or race conditions.
        // if Orther Provider strongly recommend to use a separate connection for each transaction, but in this case, we are using a single shared connection with a lock to manage concurrent access.
        // if use Other Provider, you may need to adjust the implementation to create a new connection for each transaction instead of using a shared connection with a lock.
        // In Orther Provider, if support multiple concurrent transactions, you may not need to use a lock, but you should ensure that each transaction uses its own connection to avoid conflicts.
        // In that case, can remove the SemaphoreSlim and use a separate connection for each transaction, but it may have performance implications due to the overhead of creating and disposing connections.
        // In that case, you can safely remove the line below.
        private readonly SemaphoreSlim _transactionLock = new(1, 1);
        private SQLiteAsyncConnection conn;

        public SqliteUnitOfWork(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
        }

        public async Task ExecuteInTransactionAsync(Func<Task> action)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                await action();
                return true;
            });
        }

        public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await _transactionLock.WaitAsync();
            var conn = this.conn;

            try
            {
                await conn.ExecuteAsync("BEGIN IMMEDIATE TRANSACTION");
                var result = await action();
                await conn.ExecuteAsync("COMMIT");
                return result;
            }
            catch
            {
                await conn.ExecuteAsync("ROLLBACK");
                throw;
            }
            finally
            {
                _transactionLock.Release();
            }
        }
    }
}
