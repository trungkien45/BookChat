using BookChat.Data.Providers;
using BookChat.Models;

namespace BookChat.Data.Service
{
    public interface INoteService
    {
        Task<int> AddNoteAsync(Note newNote);
        Task<int> DeleteNoteAsync(Note bookmark);
        Task<List<Note>> GetNoteInBook(int bookId);
        Task<int> UpdateNoteAsync(Note note);
    }
    public class NoteService : ServiceBase, INoteService
    {
        public NoteService(IDbSessionFactory factory) : base(factory)
        {
        }

        public async Task<int> AddNoteAsync(Note newNote)
        {
            return await ExecuteAsync(db => db.NoteRepository.InsertNoteAsync(newNote));
        }

        public async Task<int> DeleteNoteAsync(Note note)
        {
            return await ExecuteAsync(db => db.NoteRepository.DeleteNoteAsync(note));
        }

        public async Task<List<Note>> GetNoteInBook(int bookId)
        {
            return await ExecuteAsync(db => db.NoteRepository.GetNotesByBookIdAsync(bookId));
        }

        public async Task<int> UpdateNoteAsync(Note note)
        {
            return await ExecuteAsync(db => db.NoteRepository.UpdateNoteAsync(note));
        }
    }
}
