using BookChat.Data.Repository;
using BookChat.Models;
using SQLite;

namespace BookChat.Data.Providers.Sqlite.Repo
{
    public class SqliteBookmarkRepository : IBookmarkRepository
    {
        private SQLiteAsyncConnection conn;

        public SqliteBookmarkRepository(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
        }

        public Task<int> DeleteBookmarkAsync(Bookmark bookmark)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteBookmarksByBookIdAsync(int id)
        {
            //TODO: Implement this method to delete bookmarks by book ID
            throw new NotImplementedException();
        }

        public Task<Bookmark> GetBookmarkByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Bookmark>> GetBookmarksByBookIdAsync(int bookId)
        {
            throw new NotImplementedException();
        }

        public Task<int> InsertBookmarkAsync(Bookmark bookmark)
        {
            throw new NotImplementedException();
        }

        public Task<int> UpdateBookmarkAsync(Bookmark bookmark)
        {
            throw new NotImplementedException();
        }
    }
}