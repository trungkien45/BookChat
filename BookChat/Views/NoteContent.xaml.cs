using BookChat.Data.Service;
using BookChat.Models;
using BookChat.Resources;
using System.Collections.ObjectModel;

namespace BookChat.Views;

public partial class NoteContent : ContentView
{
    private readonly INoteService noteService;

    private int bookId;
    private int requestId;
    public event EventHandler<Note>? NoteSelected;
    public Func<Task<int>>? GetCurrentPageFunc { get; set; }

    public NoteContent(INoteService NoteService)
    {
        InitializeComponent();

        this.noteService = NoteService;
        BindingContext = this;
    }

    private readonly ObservableCollection<Note> notes = [];

    public ObservableCollection<Note> Notes => notes;

    public int BookId
    {
        get => bookId;
        set
        {
            if (bookId == value)
                return;

            bookId = value;
            OnPropertyChanged();

            _ = LoadNotesAsync(bookId);
        }
    }

    private async Task LoadNotesAsync(int id)
    {
        var currentRequest = ++requestId;

        try
        {
            var result = await noteService.GetNoteInBook(id);

            if (currentRequest != requestId)
                return;

            Notes.ReplaceWith(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
    #region Helper Methods
    private static Note? GetNoteFromSender(object? sender)
    {
        return (sender as Element)?.BindingContext as Note;
    }

    private async Task<int> GetCurrentPageAsync()
    {
        if (GetCurrentPageFunc != null)
        {
            try
            {
                return await GetCurrentPageFunc();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        Element? parent = Parent;
        while (parent != null)
        {
            if (parent is ViewBook viewBook)
            {
                try
                {
                    var (current, _) = await viewBook.GetPdfPageInfoAsync();
                    return current;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
            parent = parent.Parent;
        }

        return 1;
    }
    private async Task JumpToNotePageAsync(int pageNumber)
    {
        Element? parent = Parent;
        while (parent != null)
        {
            if (parent is ViewBook viewBook)
            {
                await viewBook.GoToPageAsync(pageNumber);
                break;
            }
            parent = parent.Parent;
        }
    }
    #endregion

    #region Event Handlers
    private async void OnAddNoteClicked(object? sender, EventArgs e)
    {
        if (bookId <= 0)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        int currentPage = await GetCurrentPageAsync();
        if (currentPage <= 0) currentPage = 1;

        string defaultTitle = string.Format(AppResources.PageFormat, currentPage);

        string result = await page.DisplayPromptAsync(
            AppResources.NoteListTitle,
            string.Format(AppResources.PageFormat, currentPage),
            AppResources.Ok,
            AppResources.Cancel,
            initialValue: defaultTitle);

        if (string.IsNullOrWhiteSpace(result))
            return;

        var newNote = new Note
        {
            BookId = bookId,
            PageNumber = currentPage,
            Content = result.Trim()
        };

        try
        {
            await noteService.AddNoteAsync(newNote);
            await LoadNotesAsync(bookId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void OnEditNoteClicked(object? sender, EventArgs e)
    {
        var note = GetNoteFromSender(sender);
        if (note == null) return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        string result = await page.DisplayPromptAsync(
            AppResources.EditNote,
            string.Format(AppResources.PageFormat, note.PageNumber),
            AppResources.Ok,
            AppResources.Cancel,
            initialValue: note.Content);

        if (string.IsNullOrWhiteSpace(result) || result.Trim() == note.Content)
            return;

        note.Content = result.Trim();

        try
        {
            await noteService.UpdateNoteAsync(note);
            await LoadNotesAsync(bookId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void OnDeleteNoteClicked(object? sender, EventArgs e)
    {
        var note = GetNoteFromSender(sender);
        if (note == null) return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        bool confirm = await page.DisplayAlertAsync(
            AppResources.Confirm,
            string.Format(AppResources.ConfirmDeleteMessage, note.Content),
            AppResources.Yes,
            AppResources.No);

        if (!confirm) return;

        try
        {
            await noteService.DeleteNoteAsync(note);
            await LoadNotesAsync(bookId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void OnNoteOptionsClicked(object? sender, TappedEventArgs e)
    {
        if (sender is not VisualElement el || el.BindingContext is not Note Note)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        var action = await page.DisplayActionSheetAsync(
            Note.Content,
            AppResources.Cancel,
            null,
            AppResources.EditNote,
            AppResources.DeleteNote);

        if (action == AppResources.EditNote)
            OnEditNoteClicked(el, EventArgs.Empty);
        else if (action == AppResources.DeleteNote)
            OnDeleteNoteClicked(el, EventArgs.Empty);
    }

    private async void OnNoteItemTapped(object? sender, TappedEventArgs e)
    {
        var note = GetNoteFromSender(sender);
        if (note == null) return;

        NoteSelected?.Invoke(this, note);

        if (NoteSelected == null)
        {
            await JumpToNotePageAsync(note.PageNumber);
        }
    }

    #endregion
}