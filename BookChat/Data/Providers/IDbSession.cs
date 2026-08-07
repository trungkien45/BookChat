using BookChat.Data.Repository;

namespace BookChat.Data.Providers
{
    public interface IDbSession : IAsyncDisposable
    {
        IBookRepository BookRepository { get; }
        IBookmarkRepository BookmarkRepository { get; }
        INoteRepository NoteRepository { get; }
        IUnitOfWork UnitOfWork { get; }
    }
}
