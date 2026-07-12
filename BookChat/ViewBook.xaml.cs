
namespace BookChat;

public partial class ViewBook : ContentPage
{
	public ViewBook()
	{
		InitializeComponent();
#if ANDROID
        Microsoft.Maui.Handlers.WebViewHandler.Mapper.AppendToMapping("pdfviewer", (handler, View) =>
        {
            handler.PlatformView.Settings.AllowFileAccess = true;
            handler.PlatformView.Settings.AllowFileAccessFromFileURLs = true;
            handler.PlatformView.Settings.AllowUniversalAccessFromFileURLs = true;
        });
#endif
    }

    private async void ContentPage_Loaded(object sender, EventArgs e)
    {
        //PdfViewer.Source = @"C:\Users\KIEN\Documents\Book\1818-21-bai-hoc-cho-the-ky-21-thuviensach.vn.pdf";
    }
}