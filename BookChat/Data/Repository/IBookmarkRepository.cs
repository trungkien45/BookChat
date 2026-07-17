using BookChat.Models;

namespace BookChat.Data.Repository
{
    public interface IBookmarkRepository
    {
        Task<int> DeleteBookmarksByBookIdAsync(Book book);
        public Task<int> InsertBookmarkAsync(Bookmark bookmark);
        public Task<int> UpdateBookmarkAsync(Bookmark bookmark);
        public Task<List<Bookmark>> GetBookmarksByBookIdAsync(Book book);
        public Task<Bookmark?> GetBookmarkByIdAsync(int id);
        public Task<int> DeleteBookmarkAsync(Bookmark bookmark);
    }
}