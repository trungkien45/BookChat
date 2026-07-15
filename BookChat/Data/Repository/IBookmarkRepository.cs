using BookChat.Models;

namespace BookChat.Data.Repository
{
    public interface IBookmarkRepository
    {
        Task<int> DeleteBookmarksByBookIdAsync(int id);
        public Task<int> InsertBookmarkAsync(Bookmark bookmark);
        public Task<int> UpdateBookmarkAsync(Bookmark bookmark);
        public Task<List<Bookmark>> GetBookmarksByBookIdAsync(int bookId);
        public Task<Bookmark> GetBookmarkByIdAsync(int id);
        public Task<int> DeleteBookmarkAsync(Bookmark bookmark);
    }
}