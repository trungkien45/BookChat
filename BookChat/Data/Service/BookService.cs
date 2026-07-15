using BookChat.Data.Providers;
using BookChat.Data.Repository;
using BookChat.Models;

namespace BookChat.Data.Service
{
    public interface IBookService
    {
        Task<List<Book>> GetBooksAsync();
        Task<Book> GetBookAsync(int id);
        Task<Book> GetBookByPathAsync(string path);
        Task<int> InsertBookAsync(Book book);
        Task<int> UpdateBookAsync(Book book);
        Task<int> DeleteBookAsync(Book book);
        Task SyncBooksAsync(List<string> currentPdfPaths);
    }
    public class BookService : ServiceBase, IBookService
    {

        public BookService(IDbSessionFactory dbSessionFactory): base(dbSessionFactory) 
        {
            
        }

        public async Task<List<Book>> GetBooksAsync()
        {

            return await ExecuteAsync(db => db.BookRepository.GetBooksAsync());
        
        }

        public async Task<Book> GetBookAsync(int id)
        {
            return await ExecuteAsync(db => db.BookRepository.GetBookAsync(id));
        }

        public async Task<Book> GetBookByPathAsync(string path)
        {
            return await ExecuteAsync(db => db.BookRepository.GetBookByPathAsync(path));
        }

        public async Task<int> InsertBookAsync(Book book)
        {
            return await ExecuteAsync(db => db.UnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var existingBook = await db.BookRepository.GetBookByPathAsync(book.Path);
                if (existingBook != null)
                {
                    return 0; // Book already exists, do not insert
                }
                return await db.BookRepository.InsertBookAsync(book);
            }));
        }

        public async Task<int> UpdateBookAsync(Book book)
        {
            return await ExecuteAsync(db => db.BookRepository.UpdateBookAsync(book));
        }

        public async Task<int> DeleteBookAsync(Book book)
        {
            return await ExecuteAsync(db => db.UnitOfWork.ExecuteInTransactionAsync(async () => 
            {
                return 
                    await db.BookmarkRepository.DeleteBookmarksByBookIdAsync(book.Id) 
                +
                    await db.BookRepository.DeleteBookAsync(book);
            }));
        }

        public async Task SyncBooksAsync(List<string> currentPdfPaths)
        {

            await ExecuteAsync(async db =>
            {
                var existingBooks = await GetBooksAsync();
                var currentPdfSet = new HashSet<string>(currentPdfPaths);
                var existingPaths = new HashSet<string>(existingBooks.Select(b => b.Path));

                foreach (var b in existingBooks)
                {
                    if (!currentPdfSet.Contains(b.Path))
                    {
                        await DeleteBookAsync(b);
                    }
                }
                foreach (var pdf in currentPdfPaths)
                {
                    if (!existingPaths.Contains(pdf))
                    {
                        await db.BookRepository.InsertBookAsync(new Book { Path = pdf });
                    }
                }
            });
        }
    }
}
