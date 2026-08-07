using BookChat.Data.Service;
using BookChat.Models;
using BookChat.Resources;
using System.Collections.ObjectModel;

namespace BookChat.Views;

public partial class BookmarkContent : ContentView
{
    private readonly IBookmarkService bookmarkService;

    private int bookId;
    private int requestId;

    public event EventHandler<Bookmark>? BookmarkSelected;
    public Func<Task<int>>? GetCurrentPageFunc { get; set; }

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

    #region Helper Methods

    private static Bookmark? GetBookmarkFromSender(object? sender)
    {
        return (sender as Element)?.BindingContext as Bookmark;
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

    private async Task JumpToBookmarkPageAsync(int pageNumber)
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

    private async void OnAddBookmarkClicked(object? sender, EventArgs e)
    {
        if (bookId <= 0)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        int currentPage = await GetCurrentPageAsync();
        if (currentPage <= 0) currentPage = 1;

        string defaultTitle = string.Format(AppResources.PageFormat, currentPage);

        string result = await page.DisplayPromptAsync(
            AppResources.BookmarkListTitle,
            string.Format(AppResources.PageFormat, currentPage),
            AppResources.Ok,
            AppResources.Cancel,
            initialValue: defaultTitle);

        if (string.IsNullOrWhiteSpace(result))
            return;

        var newBookmark = new Bookmark
        {
            BookId = bookId,
            PageNumber = currentPage,
            Name = result.Trim()
        };

        try
        {
            await bookmarkService.AddBookmarkAsync(newBookmark);
            await LoadBookmarksAsync(bookId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void OnEditBookmarkClicked(object? sender, EventArgs e)
    {
        var bookmark = GetBookmarkFromSender(sender);
        if (bookmark == null) return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        string result = await page.DisplayPromptAsync(
            AppResources.EditBookmark,
            string.Format(AppResources.PageFormat, bookmark.PageNumber),
            AppResources.Ok,
            AppResources.Cancel,
            initialValue: bookmark.Name);

        if (string.IsNullOrWhiteSpace(result) || result.Trim() == bookmark.Name)
            return;

        bookmark.Name = result.Trim();

        try
        {
            await bookmarkService.UpdateBookmarkAsync(bookmark);
            await LoadBookmarksAsync(bookId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void OnDeleteBookmarkClicked(object? sender, EventArgs e)
    {
        var bookmark = GetBookmarkFromSender(sender);
        if (bookmark == null) return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        bool confirm = await page.DisplayAlertAsync(
            AppResources.Confirm,
            string.Format(AppResources.ConfirmDeleteMessage, bookmark.Name),
            AppResources.Yes,
            AppResources.No);

        if (!confirm) return;

        try
        {
            await bookmarkService.DeleteBookmarkAsync(bookmark);
            await LoadBookmarksAsync(bookId);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private async void OnBookmarkOptionsClicked(object? sender, TappedEventArgs e)
    {
        if (sender is not VisualElement el || el.BindingContext is not Bookmark bookmark)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        var action = await page.DisplayActionSheetAsync(
            bookmark.Name,
            AppResources.Cancel,
            null,
            AppResources.EditBookmark,
            AppResources.DeleteBookmark);

        if (action == AppResources.EditBookmark)
            OnEditBookmarkClicked(el, EventArgs.Empty);
        else if (action == AppResources.DeleteBookmark)
            OnDeleteBookmarkClicked(el, EventArgs.Empty);
    }

    private async void OnBookmarkItemTapped(object? sender, TappedEventArgs e)
    {
        var bookmark = GetBookmarkFromSender(sender);
        if (bookmark == null) return;

        BookmarkSelected?.Invoke(this, bookmark);

        if (BookmarkSelected == null)
        {
            await JumpToBookmarkPageAsync(bookmark.PageNumber);
        }
    }

    #endregion
}
