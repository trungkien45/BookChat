using BookChat.Data.Service;
using BookChat.Models;
using BookChat.Resources;
using BookChat.StorageService;
using BookChat.Views;
using System.Diagnostics;
using System.Globalization;

namespace BookChat;

[QueryProperty(nameof(File), "File")]
public partial class ViewBook : ContentPage
{
    private string base64String = string.Empty;
    private StorageItem _file;
    string language = Preferences.Get(Const.appLanguagePreferenceKey, CultureInfo.CurrentUICulture.Name);

    public StorageItem File
    {
        get => _file;
        set
        {
            _file = value;
            LoadPdf();
        }
    }
    private Book? book = null;
    private readonly IBookService bookService;
    private void LoadPdf()
    {
#if ANDROID
        var uri = Android.Net.Uri.Parse(_file.Id);
        if (uri == null)
        {
            Task.Run(async () => await DisplayAlertAsync(AppResources.TitleError, AppResources.OpenPDFError, AppResources.Ok)).Wait();
            return;
        }
        using var stream = Android.App.Application.Context
            .ContentResolver!
            .OpenInputStream(uri);
        if (stream == null)
        {
            Task.Run(async () => await DisplayAlertAsync(AppResources.TitleError, AppResources.OpenPDFError, AppResources.Ok)).Wait();
            return;
        }
        Task.Run(async () =>
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var pdfBytes = ms.ToArray();
            LoadPdfFromByteArray(PdfViewer, pdfBytes);
            book = await bookService.GetBookByPathAsync(_file.Id);
        }).Wait();
#elif WINDOWS
        Task.Run(async () =>
        {
            using var stream = new FileStream(_file.Id, FileMode.Open);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var pdfBytes = ms.ToArray();
            LoadPdfFromByteArray(PdfViewer, pdfBytes);
            book = await bookService.GetBookByPathAsync(_file.Id);
        }).Wait();
#endif
    }
    LibraryContent libraryContent;
    NoteContent noteContent;
    BookmarkContent bookmarkContent;
    ChatContent chatContent;
    public ViewBook(IBookService bookService, LibraryContent libraryContent, NoteContent noteContent, BookmarkContent bookmarkContent, ChatContent chatContent)
    {
        _file = new StorageItem();
        InitializeComponent();
#if ANDROID
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("AllowPdfJsInternalScripts", (handler, view) =>
        {
            handler.PlatformView.Settings.AllowFileAccess = true;
            handler.PlatformView.Settings.AllowFileAccessFromFileURLs = true;
            handler.PlatformView.Settings.AllowUniversalAccessFromFileURLs = true;
            handler.PlatformView.Settings.JavaScriptEnabled = true;
            handler.PlatformView.Settings.DomStorageEnabled = true; //Very important for the PDF.js UI
        });
#endif
        this.bookService = bookService;
        this.libraryContent = libraryContent;
        this.noteContent = noteContent;
        this.bookmarkContent = bookmarkContent;
        this.chatContent = chatContent;

    }
    public void LoadPdfFromByteArray(WebView myWebView, byte[] pdfBytes)
    {
        // Convert the byte array into a clean Base64 string
        base64String = Convert.ToBase64String(pdfBytes).Replace("\r", "").Replace("\n", "");
#if ANDROID 
        PdfViewer.Source = "file:///android_asset/pdfjs/web/viewer.html";
#elif IOS
        PdfViewer.Source = "pdfjs/web/viewer.html"; // iOS automatically maps Resources/Raw to the app root
#elif WINDOWS
        PdfViewer.Source = "pdfjs/web/viewer.html";
#endif

        // Wait for the WebView to finish loading the PDF.js UI frame, then push the in-memory data
        PdfViewer.Navigated -= OnNavigated;
        PdfViewer.Navigated += OnNavigated;
    }
    async void OnNavigated(object? s, WebNavigatedEventArgs e)
    {
        if (e.Result == WebNavigationResult.Success)
        {
            // Execute a script to send the binary data to the viewer via the secure postMessage function
            await PdfViewer.EvaluateJavaScriptAsync($"window.loadPdf('{base64String}')");
        }
    }
    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        var langpreflix = language.Substring(0, Const.VNlangPrefix.Length);

        _isPinned = Preferences.Default.Get(Const.sidebarPinnedPreferenceKey, false);
        _savedWidth = Preferences.Default.Get(Const.sidebarWidthPreferenceKey, 250.0);

        var color = _isPinned ? "light" : "dark";

        imgLib.Source = $"{langpreflix}_library_{color}.png";
        imgNote.Source = $"{langpreflix}_note_{color}.png";
        imgBookmark.Source = $"{langpreflix}_bookmark_{color}.png";
        if (_savedWidth < MinSidebarWidth) _savedWidth = 250.0;

        UpdatePinButtonVisual();
        if (_isPinned)
            ChooseTabSidebar(currentTab);

        SetupChatPanel();

#if WINDOWS
        if (ResizeHandle.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement winView)
        {
            var protectedCursorProp = typeof(Microsoft.UI.Xaml.UIElement).GetProperty("ProtectedCursor", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (protectedCursorProp != null)
            {
                winView.PointerEntered += (s, e) =>
                {
                    var cursor = Microsoft.UI.Input.InputSystemCursor.Create(Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast);
                    protectedCursorProp.SetValue(winView, cursor);
                };
                winView.PointerExited += (s, e) =>
                {
                    protectedCursorProp.SetValue(winView, null);
                };
            }
        }
#endif
    }

    private string currentTab = "library";
    private bool _isPinned = false;
    private double _savedWidth = 250;
    private const double MinSidebarWidth = 10;
    private double _panStartWidth = 250;
    private CancellationTokenSource? _autoHideCts;

    private void OnPinClicked(object? sender, EventArgs e)
    {
        _isPinned = !_isPinned;
        Preferences.Default.Set(Const.sidebarPinnedPreferenceKey, _isPinned);
        UpdatePinButtonVisual();

        if (!_isPinned)
        {
            ScheduleAutoHide();
        }
        else
        {
            CancelAutoHide();
        }
    }

    private void UpdatePinButtonVisual()
    {
        if (_isPinned)
        {
            btnPin.Text = "📌";
            btnPin.TextColor = Color.FromArgb("#4EC9B0");
            ToolTipProperties.SetText(btnPin, AppResources.UnpinSidebar);
            ShowSidebar();
        }
        else
        {
            btnPin.Text = "📍";
            btnPin.TextColor = Color.FromArgb("#858585");
            ToolTipProperties.SetText(btnPin, AppResources.PinSidebar);
            HideSidebar();
        }
    }

    private void OnResizePanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartWidth = SidebarColumn.Width.Value > 0 ? SidebarColumn.Width.Value : _savedWidth;
                break;
            case GestureStatus.Running:
                double newWidth = _panStartWidth + e.TotalX;
                if (newWidth < MinSidebarWidth)
                    newWidth = MinSidebarWidth;

                _savedWidth = newWidth;
                SidebarColumn.Width = new GridLength(newWidth);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                if (SidebarColumn.Width.Value >= MinSidebarWidth)
                {
                    _savedWidth = SidebarColumn.Width.Value;
                    Preferences.Default.Set(Const.sidebarWidthPreferenceKey, _savedWidth);
                }
                break;
        }
    }

    private void OnSidebarPointerEntered(object? sender, PointerEventArgs e)
    {
        CancelAutoHide();
    }

    private void OnSidebarPointerExited(object? sender, PointerEventArgs e)
    {
        if (!_isPinned && xSideBarPanel.IsVisible)
        {
            ScheduleAutoHide();
        }
    }

    private void ScheduleAutoHide()
    {
        CancelAutoHide();
        _autoHideCts = new CancellationTokenSource();
        var token = _autoHideCts.Token;

        Task.Delay(1000, token).ContinueWith(t =>
        {
            if (!t.IsCanceled)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!_isPinned && xSideBarPanel.IsVisible)
                    {
                        HideSidebar();
                    }
                });
            }
        });
    }

    private void CancelAutoHide()
    {
        _autoHideCts?.Cancel();
        _autoHideCts?.Dispose();
        _autoHideCts = null;
    }

    private void HideSidebar()
    {
        xSideBarPanel.IsVisible = false;
        SidebarColumn.Width = 0;
        var langpreflix = language.Substring(0, Const.VNlangPrefix.Length);

        switch (currentTab)
        {
            case "library":
                imgLib.Source = $"{langpreflix}_library_dark.png";
                break;
            case "note":
                imgNote.Source = $"{langpreflix}_note_dark.png";
                break;
            case "bookmark":
                imgBookmark.Source = $"{langpreflix}_bookmark_dark.png";
                break;
            default:
                break;
        }
    }
    private void ShowSidebar()
    {
        xSideBarPanel.IsVisible = true;
        SidebarColumn.Width = Math.Max(MinSidebarWidth, _savedWidth);
        var langpreflix = language.Substring(0, Const.VNlangPrefix.Length);

        switch (currentTab)
        {
            case "library":
                imgLib.Source = $"{langpreflix}_library_light.png";
                imgNote.Source = $"{langpreflix}_note_dark.png";
                imgBookmark.Source = $"{langpreflix}_bookmark_dark.png";
                break;
            case "note":
                imgNote.Source = $"{langpreflix}_note_light.png";
                imgLib.Source = $"{langpreflix}_library_dark.png";
                imgBookmark.Source = $"{langpreflix}_bookmark_dark.png";
                if (book != null)
                {
                    noteContent.BookId = book.Id;
                }
                break;
            case "bookmark":
                imgNote.Source = $"{langpreflix}_note_dark.png";
                imgLib.Source = $"{langpreflix}_library_dark.png";
                imgBookmark.Source = $"{langpreflix}_bookmark_light.png";
                if (book != null)
                {
                    bookmarkContent.BookId = book.Id;
                }
                break;
            default:
                break;
        }

    }

    private void OnTabTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter == null) return;
        string targetTab = (e.Parameter ?? string.Empty).ToString()!;

        // Nhấn lại tab cũ -> Ẩn sidebar
        if (currentTab == targetTab && xSideBarPanel.IsVisible)
        {
            HideSidebar();
            return;
        }

        currentTab = targetTab;

        ChooseTabSidebar(targetTab);

        ShowSidebar();
    }
    private void ChooseTabSidebar(string targetTab)
    {
        switch (targetTab)
        {
            case "library":
                lbSidebarTitle.Text = AppResources.Library;
                SideBarContent.Content = libraryContent;
                break;
            case "note":
                lbSidebarTitle.Text = AppResources.Notes;
                SideBarContent.Content = noteContent;
                break;
            case "bookmark":
                lbSidebarTitle.Text = AppResources.Bookmarks;
                SideBarContent.Content = bookmarkContent;
                break;
        }
    }

    // ─── Chat Panel ────────────────────────────────────────────────────────────

    private bool _isChatPinned = false;
    private double _savedChatWidth = 320;
    private double _savedChatHeight = 240;
    private const double MinChatSize = 10;
    private double _chatPanStartValue = 320;
    private bool _isLandscape = true;
    private bool _isResizingChat = false;
    private CancellationTokenSource? _chatAutoHideCts;

    private void OnMainSizeChanged(object sender, EventArgs e)
    {
        bool landscape = this.Width > this.Height;
        if (landscape == _isLandscape) return;
        _isLandscape = landscape;
        ApplyOrientation();
    }

    private void ApplyOrientation()
    {

        bool isChatVisible = xChatPanel.IsVisible;
        double chatSize = _isLandscape ? _savedChatWidth : _savedChatHeight;
        if (_isLandscape)
        {
            // Landscape: Col0=DocPanel(*) | Col1=ChatActivityBar(45) | Col2=ResizeHandle(20) | Col3=ChatPanel
            xMain.RowDefinitions.Clear();
            xMain.ColumnDefinitions.Clear();
            xMain.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));        // 0: DocPanel
            xMain.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(45)));     // 1: ChatActivityBar
            xMain.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(isChatVisible ? 20 : 0))); // 2: ResizeHandle
            xMain.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(isChatVisible ? chatSize : 0))); // 3: ChatPanel

            Grid.SetRow(xDocumentPanel, 0); Grid.SetColumn(xDocumentPanel, 0); Grid.SetRowSpan(xDocumentPanel, 1); Grid.SetColumnSpan(xDocumentPanel, 1);
            Grid.SetRow(ChatActivityBar, 0); Grid.SetColumn(ChatActivityBar, 1);
            Grid.SetRow(ChatResizeHandle, 0); Grid.SetColumn(ChatResizeHandle, 2);
            Grid.SetRow(xChatPanel, 0); Grid.SetColumn(xChatPanel, 3);

            // Activity bar: vertical text (rotated)
            ChatActivityBar.WidthRequest = 45;
            ChatActivityBar.HeightRequest = -1;
            ChatActivityBar.VerticalOptions = LayoutOptions.Start;
            ChatActivityBar.HorizontalOptions = LayoutOptions.Center;

            grdChat.HeightRequest = 130;
            grdChat.WidthRequest = 45;
            imgChat.HeightRequest = 130;
            imgChat.WidthRequest = 45;
            // Resize handle grip: vertical bar ↔
            ChatResizeHandle.WidthRequest = 20;
            ChatResizeHandle.HeightRequest = -1;
            ChatResizeGrip.WidthRequest = 20;
            ChatResizeGrip.HeightRequest = 48;
            lbChatResizeArrow.Text = "↔";
        }
        else
        {
            // Portrait: Row0=DocPanel(*) | Row1=ChatActivityBar(45) | Row2=ResizeHandle(20) | Row3=ChatPanel
            xMain.ColumnDefinitions.Clear();
            xMain.RowDefinitions.Clear();
            xMain.RowDefinitions.Add(new RowDefinition(GridLength.Star));                                  // 0: DocPanel
            xMain.RowDefinitions.Add(new RowDefinition(new GridLength(45)));                               // 1: ChatActivityBar
            xMain.RowDefinitions.Add(new RowDefinition(new GridLength(isChatVisible ? 20 : 0)));             // 2: ResizeHandle
            xMain.RowDefinitions.Add(new RowDefinition(new GridLength(isChatVisible ? chatSize : 0)));       // 3: ChatPanel

            Grid.SetColumn(xDocumentPanel, 0); Grid.SetRow(xDocumentPanel, 0); Grid.SetRowSpan(xDocumentPanel, 1); Grid.SetColumnSpan(xDocumentPanel, 1);
            Grid.SetColumn(ChatActivityBar, 0); Grid.SetRow(ChatActivityBar, 1);
            Grid.SetColumn(ChatResizeHandle, 0); Grid.SetRow(ChatResizeHandle, 2);
            Grid.SetColumn(xChatPanel, 0); Grid.SetRow(xChatPanel, 3);

            // Activity bar: horizontal text (no rotation)
            ChatActivityBar.HeightRequest = 45;
            ChatActivityBar.WidthRequest = -1;
            ChatActivityBar.VerticalOptions = LayoutOptions.Center;
            ChatActivityBar.HorizontalOptions = LayoutOptions.Start;

            grdChat.HeightRequest = 45;
            grdChat.WidthRequest = 130;
            imgChat.HeightRequest = 45;
            imgChat.WidthRequest = 130;
            // Resize handle grip: horizontal bar ↕
            ChatResizeHandle.HeightRequest = 20;
            ChatResizeHandle.WidthRequest = -1;
            ChatResizeGrip.HeightRequest = 20;
            ChatResizeGrip.WidthRequest = 48;
            lbChatResizeArrow.Text = "↕";
        }

        ChatResizeHandle.IsVisible = isChatVisible;
        UpdateChatLabel(isChatVisible);
#if WINDOWS
        SetChatCursorForHandle();
#endif
    }

    private void UpdateChatLabel(bool isChatVisible)
    {
        var langPrefix = language.Substring(0, Const.VNlangPrefix.Length);
        var imgdirection = _isLandscape ? "vertical" : "horizontal";
        var color = isChatVisible ? "light" : "dark";
        imgChat.Source = $"{langPrefix}_chat_{imgdirection}_{color}.png";
    }

    private void SetupChatPanel()
    {
        _isChatPinned = Preferences.Default.Get(Const.chatPinnedPreferenceKey, false);
        _savedChatWidth = Preferences.Default.Get(Const.chatWidthPreferenceKey, 320.0);
        _savedChatHeight = Preferences.Default.Get(Const.chatHeightPreferenceKey, 240.0);
        if (_savedChatWidth < MinChatSize) _savedChatWidth = 320;
        if (_savedChatHeight < MinChatSize) _savedChatHeight = 240;

        lbChatTitle.Text = AppResources.ChatPanel;
        //lbChatActivityTab.Text = AppResources.ChatPanel;

        _isLandscape = Width > Height;
        ApplyOrientation();
        UpdateChatPinVisual();
    }
    private void UpdateChatPinVisual()
    {
        if (_isChatPinned)
        {
            btnChatPin.Text = "📌";
            btnChatPin.TextColor = Color.FromArgb("#4EC9B0");
            ToolTipProperties.SetText(btnChatPin, AppResources.UnpinChat);
            ShowChatPanel();
        }
        else
        {
            btnChatPin.Text = "📍";
            btnChatPin.TextColor = Color.FromArgb("#858585");
            ToolTipProperties.SetText(btnChatPin, AppResources.PinChat);
            HideChatPanel();
        }
    }

    private void OnChatPinClicked(object? sender, EventArgs e)
    {
        _isChatPinned = !_isChatPinned;
        Preferences.Default.Set(Const.chatPinnedPreferenceKey, _isChatPinned);
        UpdateChatPinVisual();
    }

    private void OnChatActivityTapped(object? sender, TappedEventArgs e)
    {
        if (xChatPanel.IsVisible)
        {
            HideChatPanel();
        }
        else
        {
            ShowChatPanel();
        }
        UpdateChatLabel(xChatPanel.IsVisible);
    }

    private void ShowChatPanel()
    {
        xChatPanel.IsVisible = true;
        ChatResizeHandle.IsVisible = true;
        double size = _isLandscape
            ? Math.Max(MinChatSize, _savedChatWidth)
            : Math.Max(MinChatSize, _savedChatHeight);

        if (_isLandscape && xMain.ColumnDefinitions.Count >= 4)
        {
            xMain.ColumnDefinitions[2] = new ColumnDefinition(new GridLength(20));
            xMain.ColumnDefinitions[3] = new ColumnDefinition(new GridLength(size));
        }
        else if (!_isLandscape && xMain.RowDefinitions.Count >= 4)
        {
            xMain.RowDefinitions[2] = new RowDefinition(new GridLength(20));
            xMain.RowDefinitions[3] = new RowDefinition(new GridLength(size));
        }
        UpdateChatLabel(xChatPanel.IsVisible);
        ChatPanelContent.Content = chatContent;
    }

    private void HideChatPanel()
    {
        xChatPanel.IsVisible = false;
        ChatResizeHandle.IsVisible = false;

        if (_isLandscape && xMain.ColumnDefinitions.Count >= 4)
        {
            xMain.ColumnDefinitions[2] = new ColumnDefinition(new GridLength(0));
            xMain.ColumnDefinitions[3] = new ColumnDefinition(new GridLength(0));
        }
        else if (!_isLandscape && xMain.RowDefinitions.Count >= 4)
        {
            xMain.RowDefinitions[2] = new RowDefinition(new GridLength(0));
            xMain.RowDefinitions[3] = new RowDefinition(new GridLength(0));
        }
        UpdateChatLabel(xChatPanel.IsVisible);
    }

    private void OnChatResizePanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _isResizingChat = true;
                _chatAutoHideCts?.Cancel();
                _chatAutoHideCts?.Dispose();
                _chatAutoHideCts = null;
                _chatPanStartValue = _isLandscape ? _savedChatWidth : _savedChatHeight;
                break;

            case GestureStatus.Running:
                // Landscape: handle is left of ChatPanel, drag left grows chat
                // Portrait: handle is above ChatPanel, drag up grows chat
                double delta = _isLandscape ? -e.TotalX : -e.TotalY;
                double newSize = Math.Max(MinChatSize, _chatPanStartValue + delta);

                if (_isLandscape && xMain.ColumnDefinitions.Count >= 4)
                {
                    _savedChatWidth = newSize;
                    xMain.ColumnDefinitions[3] = new ColumnDefinition(new GridLength(newSize)); // index 3 = xChatPanel
                }
                else if (!_isLandscape && xMain.RowDefinitions.Count >= 4)
                {
                    _savedChatHeight = newSize;
                    xMain.RowDefinitions[3] = new RowDefinition(new GridLength(newSize)); // index 3 = xChatPanel
                }
                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                _isResizingChat = false;
                if (_isLandscape)
                    Preferences.Default.Set(Const.chatWidthPreferenceKey, _savedChatWidth);
                else
                    Preferences.Default.Set(Const.chatHeightPreferenceKey, _savedChatHeight);
                break;
        }
    }

    private void OnChatPointerEntered(object? sender, PointerEventArgs e)
    {
        _chatAutoHideCts?.Cancel();
        _chatAutoHideCts?.Dispose();
        _chatAutoHideCts = null;
    }

    private void OnChatPointerExited(object? sender, PointerEventArgs e)
    {

        if (!_isChatPinned && !_isResizingChat && xChatPanel.IsVisible)
        {
            _chatAutoHideCts = new CancellationTokenSource();
            var token = _chatAutoHideCts.Token;
            Task.Delay(1000, token).ContinueWith(t =>
            {
                if (!t.IsCanceled)
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (!_isChatPinned && !_isResizingChat) HideChatPanel();
                    });
            });
        }
    }

#if WINDOWS
    private void SetChatCursorForHandle()
    {
        if (ChatResizeHandle.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement winView)
        {
            var prop = typeof(Microsoft.UI.Xaml.UIElement).GetProperty("ProtectedCursor",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            if (prop != null)
            {
                var shape = _isLandscape
                    ? Microsoft.UI.Input.InputSystemCursorShape.SizeWestEast
                    : Microsoft.UI.Input.InputSystemCursorShape.SizeNorthSouth;

                winView.PointerEntered += (s, e) => prop.SetValue(winView, Microsoft.UI.Input.InputSystemCursor.Create(shape));
                winView.PointerExited += (s, e) => prop.SetValue(winView, null);
            }
        }
    }
#endif
}