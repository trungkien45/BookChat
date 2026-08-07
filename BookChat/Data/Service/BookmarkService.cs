using BookChat.Data.Providers;
using BookChat.Models;

namespace BookChat.Data.Service
{
    public interface IBookmarkService
    {
        Task<List<Bookmark>> GetBookmarkInBook(int bookId);
        Task<int> AddBookmarkAsync(Bookmark bookmark);
        Task<int> UpdateBookmarkAsync(Bookmark bookmark);
        Task<int> DeleteBookmarkAsync(Bookmark bookmark);
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

        public async Task<int> AddBookmarkAsync(Bookmark newBookmark)
        {
            return await ExecuteAsync(db => db.BookmarkRepository.InsertBookmarkAsync(newBookmark));
        }

        public async Task<int> UpdateBookmarkAsync(Bookmark bookmark)
        {
            return await ExecuteAsync(db => db.BookmarkRepository.UpdateBookmarkAsync(bookmark));
        }

        public async Task<int> DeleteBookmarkAsync(Bookmark bookmark)
        {
            return await ExecuteAsync(db => db.BookmarkRepository.DeleteBookmarkAsync(bookmark));
        }
    }

}

