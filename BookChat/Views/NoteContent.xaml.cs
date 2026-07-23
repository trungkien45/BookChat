using BookChat.Data.Service;
using BookChat.Models;

namespace BookChat.Views;

public partial class NoteContent : ContentView
{
    private List<Note> notes;

    private INoteService _noteService;
    private int bookId;

    public NoteContent(INoteService noteService)
    {
        InitializeComponent();
        _noteService = noteService;
    }

    public int BookId
    {
        get => bookId;
        set
        {
            bookId = value;
            Task.Run(async () =>
            {
                Notes = await _noteService.GetNoteInBook(bookId);
            });
        }
    }
    public List<Note> Notes
    {
        get => notes;
        set
        {
            notes = value;
            OnPropertyChanged();
        }
    }
}