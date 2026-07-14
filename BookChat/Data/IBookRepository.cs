using BookChat.Models;

namespace BookChat.Data
{
    public interface IBookRepository
    {
        Task<List<Book>> GetBooksAsync();
        Task<Book> GetBookAsync(int id);
        Task<Book> GetBookByPathAsync(string path);
        Task<int> InsertBookAsync(Book book);
        Task<int> UpdateBookAsync(Book book);
        Task<int> DeleteBookAsync(Book book);
    }
}
