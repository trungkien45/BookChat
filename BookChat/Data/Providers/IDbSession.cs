using BookChat.Data.Providers.Sqlite;
using BookChat.Data.Repository;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

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
