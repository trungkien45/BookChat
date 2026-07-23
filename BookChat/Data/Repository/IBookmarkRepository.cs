using BookChat.Models;

namespace BookChat.Data.Repository
{
    public interface IBookmarkRepository
    {
        Task<int> DeleteBookmarksByBookIdAsync(int book);
        Task<int> InsertBookmarkAsync(Bookmark bookmark);
        Task<int> UpdateBookmarkAsync(Bookmark bookmark);
        Task<List<Bookmark>> GetBookmarksByBookIdAsync(int book);
        Task<Bookmark?> GetBookmarkByIdAsync(int id);
        Task<int> DeleteBookmarkAsync(Bookmark bookmark);
    }
}