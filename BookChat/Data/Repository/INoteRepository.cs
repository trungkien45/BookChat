using BookChat.Models;

namespace BookChat.Data.Repository
{
    public interface INoteRepository
    {
        Task<int> DeleteNotesByBookIdAsync(int bookId);
        Task<int> InsertNoteAsync(Note note);
        Task<int> UpdateNoteAsync(Note note);
        Task<List<Note>> GetNotesByBookIdAsync(int bookId);
        Task<Note?> GetNoteByIdAsync(int id);
        Task<int> DeleteNoteAsync(Note note);
    }
}
