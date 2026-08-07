using BookChat.Data.Providers;
using BookChat.Models;

namespace BookChat.Data.Service
{
    public interface INoteService
    {
        Task<List<Note>> GetNoteInBook(int bookId);
    }
    public class NoteService : ServiceBase, INoteService
    {
        public NoteService(IDbSessionFactory factory) : base(factory)
        {
        }

        public async Task<List<Note>> GetNoteInBook(int bookId)
        {
            return await ExecuteAsync(db => db.NoteRepository.GetNotesByBookIdAsync(bookId));
        }
    }
}
