using BookChat.Models;

namespace BookChat.Data.Repository
{
    public interface IBookmarkRepository
    {
        Task<int> DeleteBookmarksByBookIdAsync(Book book);
        Task<int> InsertBookmarkAsync(Bookmark bookmark);
        Task<int> UpdateBookmarkAsync(Bookmark bookmark);
        Task<List<Bookmark>> GetBookmarksByBookIdAsync(Book book);
        Task<Bookmark?> GetBookmarkByIdAsync(int id);
        Task<int> DeleteBookmarkAsync(Bookmark bookmark);
    }
}