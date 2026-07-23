using BookChat.Data.Providers.Sqlite.Repo;
using BookChat.Data.Repository;
using SQLite;

namespace BookChat.Data.Providers.Sqlite
{
    public sealed class SqliteDbSession : IDbSession
    {
        private readonly SQLiteAsyncConnection _conn;

        public SqliteDbSession(SQLiteAsyncConnection conn)
        {
            _conn = conn;
            BookRepository = new SqliteBookRepository(_conn);
            BookmarkRepository = new SqliteBookmarkRepository(_conn);
            NoteRepository = new SqliteNoteRepository(_conn);
            UnitOfWork = new SqliteUnitOfWork(_conn);
        }

        public IBookRepository BookRepository { get; }

        public IBookmarkRepository BookmarkRepository { get; }

        public IUnitOfWork UnitOfWork { get; }

        public INoteRepository NoteRepository { get; }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
