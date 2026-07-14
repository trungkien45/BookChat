using BookChat.Models;

namespace BookChat.Data.Providers
{
    public class SqliteBookRepository : IBookRepository
    {
        private readonly SqliteDatabase _database;

        public SqliteBookRepository(SqliteDatabase database)
        {
            _database = database;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            var conn = await _database.GetConnectionAsync();
            return await conn.QueryAsync<Book>("SELECT Id, Path, ReadingPage FROM Book");
        }

        public async Task<Book> GetBookAsync(int id)
        {
            var conn = await _database.GetConnectionAsync();
            return (await conn.QueryAsync<Book>(
                "SELECT Id, Path, ReadingPage FROM Book WHERE Id = ? LIMIT 1",
                id)).FirstOrDefault()!;
        }

        public async Task<Book> GetBookByPathAsync(string path)
        {
            var conn = await _database.GetConnectionAsync();
            return (await conn.QueryAsync<Book>(
                "SELECT Id, Path, ReadingPage FROM Book WHERE Path = ? LIMIT 1",
                path)).FirstOrDefault()!;
        }

        public async Task<int> InsertBookAsync(Book book)
        {
            var conn = await _database.GetConnectionAsync();
            var rows = await conn.ExecuteAsync(
                "INSERT OR IGNORE INTO Book (Path, ReadingPage) VALUES (?, ?)",
                book.Path,
                book.ReadingPage);

            if (rows > 0)
                book.Id = await conn.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");

            return rows;
        }

        public async Task<int> UpdateBookAsync(Book book)
        {
            var conn = await _database.GetConnectionAsync();
            return await conn.ExecuteAsync(
                "UPDATE Book SET Path = ?, ReadingPage = ? WHERE Id = ?",
                book.Path,
                book.ReadingPage,
                book.Id);
        }

        public async Task<int> DeleteBookAsync(Book book)
        {
            var conn = await _database.GetConnectionAsync();
            return await conn.ExecuteAsync("DELETE FROM Book WHERE Id = ?", book.Id);
        }
    }
}
