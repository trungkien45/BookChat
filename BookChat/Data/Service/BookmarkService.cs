using BookChat.Data.Providers;
using BookChat.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BookChat.Data.Service
{
    public interface IBookmarkService
    {
        Task<List<Bookmark>> GetBookmarkInBook(int bookId);
    }
    public class BookmarkService : ServiceBase, IBookmarkService
    {

        public BookmarkService(IDbSessionFactory factory) : base(factory)
        {
        }

        public async Task<List<Bookmark>> GetBookmarkInBook(int bookId)
        {
            return await ExecuteAsync(db => db.BookmarkRepository.GetBookmarksByBookIdAsync(bookId));
        }
    }

}
