namespace BookChat.Data.Providers.Sqlite
{
    public class SqliteDbSessionFactory : IDbSessionFactory
    {
        private readonly SqliteDatabase _database;

        public SqliteDbSessionFactory(SqliteDatabase database)
        {
            _database = database;
        }

        public async Task<IDbSession> CreateAsync()
        {
            var conn = await _database.GetConnectionAsync();

            return new SqliteDbSession(conn);
        }
    }
}
