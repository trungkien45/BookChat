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

    #region Event Handlers

    private void OnAddBookmarkClicked(object? sender, EventArgs e)
    {
        // TODO: Thân hàm xử lý thêm bookmark
    }

    private void OnEditBookmarkClicked(object? sender, EventArgs e)
    {
        // TODO: Thân hàm xử lý sửa bookmark
    }

    private void OnDeleteBookmarkClicked(object? sender, EventArgs e)
    {
        // TODO: Thân hàm xử lý xóa bookmark
    }

    private async void OnBookmarkOptionsClicked(object? sender, TappedEventArgs e)
    {
        if (sender is not VisualElement el || el.BindingContext is not Bookmark bookmark)
            return;

        var page = Application.Current?.Windows.FirstOrDefault()?.Page;
        if (page == null) return;

        var action = await page.DisplayActionSheetAsync(
            bookmark.Name,
            BookChat.Resources.AppResources.Cancel,
            null,
            BookChat.Resources.AppResources.EditBookmark,
            BookChat.Resources.AppResources.DeleteBookmark);

        if (action == BookChat.Resources.AppResources.EditBookmark)
            OnEditBookmarkClicked(el, EventArgs.Empty);
        else if (action == BookChat.Resources.AppResources.DeleteBookmark)
            OnDeleteBookmarkClicked(el, EventArgs.Empty);
    }

    private void OnBookmarkItemTapped(object? sender, TappedEventArgs e)
    {
        // TODO: Thân hàm xử lý chọn bookmark
    }

    #endregion
}