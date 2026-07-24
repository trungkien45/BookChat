using BookChat.Data.Service;
using BookChat.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BookChat.Views;

public partial class BookmarkContent : ContentView
{
    private readonly IBookmarkService bookmarkService;

    private int bookId;
    private int requestId;

    public BookmarkContent(IBookmarkService bookmarkService)
    {
        InitializeComponent();

        this.bookmarkService = bookmarkService;
        BindingContext = this;
    }

    private readonly ObservableCollection<Bookmark> bookmarks = [];

    public ObservableCollection<Bookmark> Bookmarks => bookmarks;

    public int BookId
    {
        get => bookId;
        set
        {
            if (bookId == value)
                return;

            bookId = value;
            OnPropertyChanged();

            _ = LoadBookmarksAsync(bookId);
        }
    }

    private async Task LoadBookmarksAsync(int id)
    {
        var currentRequest = ++requestId;

        try
        {
            var result = await bookmarkService.GetBookmarkInBook(id);

            if (currentRequest != requestId)
                return;

            Bookmarks.ReplaceWith(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}