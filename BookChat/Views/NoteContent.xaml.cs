using BookChat.Data.Service;
using BookChat.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BookChat.Views;

public partial class NoteContent : ContentView
{
    private readonly INoteService NoteService;

    private int bookId;
    private int requestId;

    public NoteContent(INoteService NoteService)
    {
        InitializeComponent();

        this.NoteService = NoteService;
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
            var result = await NoteService.GetNoteInBook(id);

            if (currentRequest != requestId)
                return;

            Notes.ReplaceWith(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    #region Event Handlers

    private void OnAddNoteClicked(object? sender, EventArgs e)
    {
        // TODO: Thân hàm xử lý thêm Note
    }

    private void OnEditNoteClicked(object? sender, EventArgs e)
    {
        // TODO: Thân hàm xử lý sửa Note
    }

    private void OnDeleteNoteClicked(object? sender, EventArgs e)
    {
        // TODO: Thân hàm xử lý xóa Note
    }

    private async void OnNoteOptionsClicked(object? sender, TappedEventArgs e)
    {
        if (sender is not VisualElement el || el.BindingContext is not Note Note)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        var action = await page.DisplayActionSheetAsync(
            Note.Content,
            BookChat.Resources.AppResources.Cancel,
            null,
            BookChat.Resources.AppResources.EditNote,
            BookChat.Resources.AppResources.DeleteNote);

        if (action == BookChat.Resources.AppResources.EditNote)
            OnEditNoteClicked(el, EventArgs.Empty);
        else if (action == BookChat.Resources.AppResources.DeleteNote)
            OnDeleteNoteClicked(el, EventArgs.Empty);
    }

    private void OnNoteItemTapped(object? sender, TappedEventArgs e)
    {
        // TODO: Thân hàm xử lý chọn Note
    }

    #endregion
}