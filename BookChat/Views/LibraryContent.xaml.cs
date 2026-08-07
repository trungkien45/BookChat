using BookChat.StorageService;
using BookChat.StorageService.Inteface;
using System.Collections.ObjectModel;

namespace BookChat.Views;

public partial class LibraryContent : ContentView
{
    private readonly ObservableCollection<StorageItem> storageItems = [];
    private StorageItem storageItem = null!;
    private int requestId;
    public StorageItem StorageItem
    {
        get => storageItem;
        set
        {
            if (storageItem == value)
                return;

            storageItem = value;
            OnPropertyChanged();

            _ = LoadFilesAsync(storageItem);
        }
    }

    private async Task LoadFilesAsync(StorageItem storageItem)
    {
        var currentRequest = ++requestId;

        try
        {
            var storedPath = Preferences.Get(Const.libPathPreferenceKey, string.Empty);
            var parent = await storageService.GetParentFolder(storageItem, storedPath);
            if (parent == null)
                return;
            var result = await storageService.GetPdfFiles(parent);

            if (currentRequest != requestId)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                StorageItems.ReplaceWith(result);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    public ObservableCollection<StorageItem> StorageItems => storageItems;

    private readonly IStorageService storageService;

    public LibraryContent(IStorageService storageService)
    {
        this.storageService = storageService;
        InitializeComponent();
    }

    public event EventHandler<StorageItem>? FileSelected;

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is StorageItem selectedItem)
        {
            FileSelected?.Invoke(this, selectedItem);
            ((CollectionView)sender).SelectedItem = null;
        }
    }
}