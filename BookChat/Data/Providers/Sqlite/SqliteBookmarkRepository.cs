using BookChat.Data.Repository;
using SQLite;

namespace BookChat.Data.Providers.Sqlite
{
    public class SqliteBookmarkRepository : IBookmarkRepository
    {
        private SQLiteAsyncConnection conn;

        public SqliteBookmarkRepository(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
        }

        public Task<int> DeleteBookmarksByBookIdAsync(int id)
        {
            //TODO: Implement this method to delete bookmarks by book ID
            throw new NotImplementedException();
        }
    }
}