using BookChat.Models;

namespace BookChat.Data
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

    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BookService(IBookRepository bookRepository, IUnitOfWork unitOfWork)
        {
            _bookRepository = bookRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Book>> GetBooksAsync()
        {
            return await _bookRepository.GetBooksAsync();
        }

        public async Task<Book> GetBookAsync(int id)
        {
            return await _bookRepository.GetBookAsync(id);
        }

        public async Task<Book> GetBookByPathAsync(string path)
        {
            return await _bookRepository.GetBookByPathAsync(path);
        }

        public async Task<int> InsertBookAsync(Book book)
        {
            return await _bookRepository.InsertBookAsync(book);
        }

        public async Task<int> UpdateBookAsync(Book book)
        {
            return await _bookRepository.UpdateBookAsync(book);
        }

        public async Task<int> DeleteBookAsync(Book book)
        {
            return await _unitOfWork.ExecuteInTransactionAsync(async () => 
            {
                return await _bookRepository.DeleteBookAsync(book);
            });
        }

        public async Task SyncBooksAsync(List<string> currentPdfPaths)
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
                    await _bookRepository.InsertBookAsync(new Book { Path = pdf });
                }
            }
        }
    }
}
