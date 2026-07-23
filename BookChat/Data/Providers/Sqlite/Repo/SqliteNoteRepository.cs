using BookChat.Data.Repository;
using BookChat.Models;
using SQLite;

namespace BookChat.Data.Providers.Sqlite.Repo
{
    public class SqliteNoteRepository : INoteRepository
    {
        private readonly SQLiteAsyncConnection conn;

        public SqliteNoteRepository(SQLiteAsyncConnection conn)
        {
            this.conn = conn;
        }

        public async Task<int> DeleteNoteAsync(Note note)
        {
            return await conn.ExecuteAsync("DELETE FROM Note WHERE Id = ?", note.Id);
        }

        public async Task<int> DeleteNotesByBookIdAsync(int bookId)
        {
            return await conn.ExecuteAsync("DELETE FROM Note WHERE BookId = ?", bookId);
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return (await conn.QueryAsync<Note>("SELECT Id, BookId, PageNumber, Content FROM Note WHERE Id = ?", id)).FirstOrDefault();
        }

        public async Task<List<Note>> GetNotesByBookIdAsync(int bookId)
        {
            return await conn.QueryAsync<Note>("SELECT Id, BookId, PageNumber, Content FROM Note WHERE BookId = ?", bookId);
        }

        public async Task<int> InsertNoteAsync(Note note)
        {
            var rows = await conn.ExecuteAsync(
                "INSERT OR IGNORE INTO Note (BookId, PageNumber, Content) VALUES (?, ?, ?)",
                note.BookId,
                note.PageNumber,
                note.Content);

            if (rows > 0)
                note.Id = await conn.ExecuteScalarAsync<int>("SELECT last_insert_rowid()");

            return rows;
        }

        public async Task<int> UpdateNoteAsync(Note note)
        {
            return await conn.ExecuteAsync(
                "UPDATE Note SET BookId = ?, PageNumber = ?, Content = ? WHERE Id = ?",
                note.BookId,
                note.PageNumber,
                note.Content,
                note.Id);
        }
    }
}
