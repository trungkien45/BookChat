using BookChat.Data.Repository;
using BookChat.Models;
using SQLite;

namespace BookChat.Data.Providers.Sqlite.Repo
{
    public class SqliteBookmarkRepository : IBookmarkRepository
    {
        private readonly SQLiteAsyncConnection conn;

        public SqliteBookmarkRepository(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
        }

        public async Task<int> DeleteBookmarkAsync(Bookmark bookmark)
        {
            return await conn.ExecuteAsync("DELETE FROM Bookmark WHERE Id = ?", bookmark.Id);
        }

        public async Task<int> DeleteBookmarksByBookIdAsync(int bookId)
        {
            return await conn.ExecuteAsync("DELETE FROM Bookmark WHERE BookId = ?", bookId);
        }

        public async Task<Bookmark?> GetBookmarkByIdAsync(int id)
        {
            return (await conn.QueryAsync<Bookmark>("SELECT Id, BookId, PageNumber, Name FROM Bookmark WHERE Id = ?", id)).FirstOrDefault();
        }

        public async Task<List<Bookmark>> GetBookmarksByBookIdAsync(int bookId)
        {
            return await conn.QueryAsync<Bookmark>("SELECT Id, BookId, PageNumber, Name FROM Bookmark WHERE BookId = ? ORDER BY PageNumber", bookId);
        }

        public async Task<int> InsertBookmarkAsync(Bookmark bookmark)
        {
            var rows = await conn.ExecuteAsync(
                "INSERT OR IGNORE INTO Bookmark (BookId, PageNumber, Name) VALUES (?, ?, ?)",
                bookmark.BookId,
                bookmark.PageNumber,
                bookmark.Name);

            if (rows > 0)
                bookmark.Id = await conn.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");

            return rows;
        }

        public async Task<int> UpdateBookmarkAsync(Bookmark bookmark)
        {
            return await conn.ExecuteAsync(
                "UPDATE Bookmark SET BookId = ?, PageNumber = ?, Name = ? WHERE Id = ?",
                bookmark.BookId,
                bookmark.PageNumber,
                bookmark.Name,
                bookmark.Id);
        }
    }
}