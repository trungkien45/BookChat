using BookChat.Data.Repository;
using BookChat.Models;
using SQLite;

namespace BookChat.Data.Providers.Sqlite.Repo
{
    public class SqliteBookRepository : IBookRepository
    {
        private SQLiteAsyncConnection conn;

        public SqliteBookRepository(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            return await conn.QueryAsync<Book>("SELECT Id, Path, ReadingPage FROM Book");
        }

        public async Task<Book> GetBookAsync(int id)
        {
            return (await conn.QueryAsync<Book>(
                "SELECT Id, Path, ReadingPage FROM Book WHERE Id = ? LIMIT 1",
                id)).FirstOrDefault()!;
        }

        public async Task<Book> GetBookByPathAsync(string path)
        {
            return (await conn.QueryAsync<Book>(
                "SELECT Id, Path, ReadingPage FROM Book WHERE Path = ? LIMIT 1",
                path)).FirstOrDefault()!;
        }

        public async Task<int> InsertBookAsync(Book book)
        {
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
            return await conn.ExecuteAsync(
                "UPDATE Book SET Path = ?, ReadingPage = ? WHERE Id = ?",
                book.Path,
                book.ReadingPage,
                book.Id);
        }

        public async Task<int> DeleteBookAsync(Book book)
        {
            return await conn.ExecuteAsync("DELETE FROM Book WHERE Id = ?", book.Id);
        }
    }
}
