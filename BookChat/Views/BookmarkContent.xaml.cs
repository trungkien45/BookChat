using BookChat.Data.Service;
using BookChat.Models;
using System.ComponentModel;

namespace BookChat.Views;

public partial class BookmarkContent : ContentView, INotifyPropertyChanged
{
    private int bookId;
    private List<Bookmark> bookmarks;
    private readonly IBookmarkService bookmarkService;
    public BookmarkContent(IBookmarkService bookmarkService)
    {
        InitializeComponent();
        this.bookmarkService = bookmarkService;
    }
    public List<Bookmark> Bookmarks { get => bookmarks; set { bookmarks = value; OnPropertyChanged(); } }
    public int BookId
    {
        get => bookId; 
        set
        {
            bookId = value;
            Task.Run(async () =>
            {
                Bookmarks = await bookmarkService.GetBookmarkInBook(bookId);
            });
        }
    }

}