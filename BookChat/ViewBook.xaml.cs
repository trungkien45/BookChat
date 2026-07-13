using BookChat.Resources;
using BookChat.StorageService;

namespace BookChat;

[QueryProperty(nameof(File), "File")]
public partial class ViewBook : ContentPage
{
    private string base64String = string.Empty;
    private StorageItem _file;

    public StorageItem File
    {
        get => _file;
        set
        {
            _file = value;
#if ANDROID
            LoadPdf();
#elif WINDOWS
            var filePath = _file?.Id;
            if (!string.IsNullOrEmpty(filePath))
            {
                var fileUri = new Uri(filePath);
                PdfViewer.Source = fileUri;
            }
#endif
        }
    }
#if ANDROID
    private void LoadPdf()
    {
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
        }).Wait();
    }
#endif
    public ViewBook()
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
        PdfViewer.Source = "ms-appx-web:///Resources/Raw/pdfjs/web/viewer.html";
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
            string jsScript = $@"
                    window.postMessage({{
                        type: 'LOAD_BYTE_ARRAY',
                        data: '{base64String}'
                    }}, '*');";

            // Execute javaScript necessary to load the PDF data into the viewer
            await PdfViewer.EvaluateJavaScriptAsync(jsScript);
        }
    }
    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
    }
}