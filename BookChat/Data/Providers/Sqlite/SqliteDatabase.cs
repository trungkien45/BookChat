using SQLite;

namespace BookChat.Data.Providers
{
    public class SqliteDatabase
    {
        private SQLiteAsyncConnection? _connection;

        public async Task<SQLiteAsyncConnection> GetConnectionAsync()
        {
            // If the connection is already established, return it. Otherwise, create a new connection and initialize the database.
            if (_connection != null)
                return _connection;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "Db.db");
            _connection = new SQLiteAsyncConnection(dbPath);
            await _connection.ExecuteAsync("""
                CREATE TABLE IF NOT EXISTS Book (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Path TEXT NOT NULL UNIQUE,
                    ReadingPage INTEGER NOT NULL DEFAULT 0
                )
                """);

            return _connection;
        }
    }
}
