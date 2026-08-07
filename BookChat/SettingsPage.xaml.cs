using BookChat.Resources;
using System.Globalization;
using BookChat.StorageService;
using BookChat.StorageService.Inteface;
using BookChat.Data.Service;
namespace BookChat
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            // Load saved lib path
            var saved = Preferences.Get(Const.libPathPreferenceKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
                LibPathEntry.Text = saved;

            // Load saved language
            var lang = Preferences.Get(Const.appLanguagePreferenceKey, CultureInfo.CurrentUICulture.Name);
            var index = LanguagePicker.Items.IndexOf(lang);
            if (index >= 0)
                LanguagePicker.SelectedIndex = index;

            // Update UI texts to current culture
            UpdateUIText();
        }

        private async void OnBrowseClicked(object sender, EventArgs e)
        {
            try
            {
#if ANDROID
                // Use native Android folder picker (Storage Access Framework)
                if (MainActivity.Instance != null)
                {
                    var uriStr = await MainActivity.Instance.PickFolderAsync();
                    if (!string.IsNullOrEmpty(uriStr))
                    {
                        // Store the persisted URI string. On Android use URI to access files.
                        LibPathEntry.Text = uriStr;
                    }
                    else
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.PickFolderFailed, AppResources.Ok);
                    }
                    return;
                }
#endif

                // Cross-platform fallback: pick a file and use its directory
                var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = AppResources.FolderPickTitle });
                if (result != null)
                {
                    var full = result.FullPath;
                    if (!string.IsNullOrEmpty(full))
                    {
                        var dir = Path.GetDirectoryName(full);
                        LibPathEntry.Text = dir;
                    }
                    else
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.PickFolderFailed, AppResources.Ok);
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(AppResources.Error, string.Format(AppResources.FolderPickFailedMessage, ex.Message), AppResources.Ok);
            }
        }

        private async void OnSaveLibPathClicked(object sender, EventArgs e)
        {
            var path = LibPathEntry.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                await DisplayAlertAsync(AppResources.Error, AppResources.LibPathEmptyMessage, AppResources.Ok);
                return;
            }

            Preferences.Set(Const.libPathPreferenceKey, path);

            try
            {
                var storageService = Application.Current?.Handler?.MauiContext?.Services.GetService<IStorageService>();
                var bookService = Application.Current?.Handler?.MauiContext?.Services.GetService<IBookService>();

                if (storageService != null && bookService != null)
                {
                    var rootItem = await storageService.GetRootFolder(path) ?? new StorageItem 
                    { 
                        Id = path, 
                        DocumentId = "", 
                        DisplayName = System.IO.Path.GetFileName(path),
                        IsDirectory = true 
                    };

                    var allPdfItems = await storageService.GetPdfFilesAndFolders(rootItem, true);
                    var pdfFiles = allPdfItems.Select(x => x.Id).ToList();

                    await bookService.SyncBooksAsync(pdfFiles);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(AppResources.Error, $"Database sync failed: {ex.Message}", AppResources.Ok);
            }

            await DisplayAlertAsync(AppResources.SaveLibPathButton, AppResources.SavedLibPathMessage, AppResources.Ok);
        }


        private async void OnApplyLanguageClicked(object sender, EventArgs e)
        {
            if (LanguagePicker.SelectedIndex < 0)
            {
                await DisplayAlertAsync(AppResources.Info, AppResources.PleaseSelectLanguage, AppResources.Ok);
                return;
            }

            var culture = LanguagePicker.Items[LanguagePicker.SelectedIndex];
            try
            {
                var ci = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentCulture = ci;
                CultureInfo.DefaultThreadCurrentUICulture = ci;

                Preferences.Set(Const.appLanguagePreferenceKey, culture);
                // Update UI strings immediately
                //UpdateUIText();

                await DisplayAlertAsync(AppResources.Language, AppResources.LanguageApplied, AppResources.Ok);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync(AppResources.Error, string.Format(AppResources.ApplyLanguageError, ex.Message), AppResources.Ok);
            }
        }

        private void UpdateUIText()
        {
            try
            {
                // Update labels and buttons that are defined in XAML with x:Name
                var titleLabel = this.FindByName<Label>("SettingsTitleLabel");
                var libLabel = this.FindByName<Label>("LibPathLabel");
                var browseBtn = this.FindByName<Button>("BrowseButton");
                var saveBtn = this.FindByName<Button>("SaveLibPathButton");
                var appLangLabel = this.FindByName<Label>("AppLanguageLabel");
                var applyBtn = this.FindByName<Button>("ApplyLanguageButton");
                var configureLabel = this.FindByName<Label>("ConfigureInfoLabel");

                if (titleLabel != null) titleLabel.Text = AppResources.SettingsTitle;
                if (libLabel != null) libLabel.Text = AppResources.LibPathLabel;
                if (LibPathEntry != null) LibPathEntry.Placeholder = AppResources.LibPathPlaceholder;
                if (browseBtn != null) browseBtn.Text = AppResources.BrowseButton;
                if (saveBtn != null) saveBtn.Text = AppResources.SaveLibPathButton;
                if (appLangLabel != null) appLangLabel.Text = AppResources.AppLanguageLabel;
                if (LanguagePicker != null) LanguagePicker.Title = AppResources.SelectLanguageTitle;
                if (applyBtn != null) applyBtn.Text = AppResources.ApplyLanguageButton;
                if (configureLabel != null) configureLabel.Text = AppResources.ConfigureInfo;
            }
            catch { }
        }
    }
}
