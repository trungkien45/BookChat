using BookChat.Data.Service;
using BookChat.Models;
using BookChat.StorageService;
using BookChat.StorageService.Inteface;
using System.Collections.ObjectModel;
using System.Net;

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
            var parent = await stogareService.GetParentFolder(storageItem, storedPath);
            if (parent == null)
                return;
            var result = await stogareService.GetPdfFiles(parent);

            if (currentRequest != requestId)
                return;

            StorageItems.ReplaceWith(result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    public ObservableCollection<StorageItem> StorageItems => storageItems;

    private readonly IStogareService stogareService;

    public LibraryContent(IStogareService stogareService)
    {
        this.stogareService = stogareService;
        InitializeComponent();
    }

}