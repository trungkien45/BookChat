using Microsoft.Maui.Controls;
using BookChat.Resources;
using System.Globalization;
using Microsoft.Maui.Storage;
using System.IO;

namespace BookChat
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();

            // Load saved lib path
            var saved = Preferences.Get("LibPath", string.Empty);
            if (!string.IsNullOrEmpty(saved))
                LibPathEntry.Text = saved;

            // Load saved language
            var lang = Preferences.Get("AppLanguage", CultureInfo.CurrentUICulture.Name);
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
                        await DisplayAlert("Info", AppResources.PickFolderFailed, "OK");
                    }
                    return;
                }
#endif

                // Cross-platform fallback: pick a file and use its directory
                var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Pick any file inside the folder you want to use" });
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
                        await DisplayAlert("Info", AppResources.PickFolderFailed, "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Folder pick failed: {ex.Message}", "OK");
            }
        }

        private void OnSaveLibPathClicked(object sender, EventArgs e)
        {
            var path = LibPathEntry.Text ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
            {
                DisplayAlert("Error", "Lib Path is empty. Please enter or pick a folder.", "OK");
                return;
            }

            Preferences.Set("LibPath", path);
            DisplayAlert(AppResources.SaveLibPathButton, AppResources.SavedLibPathMessage, "OK");
        }

        private async void OnApplyLanguageClicked(object sender, EventArgs e)
        {
            if (LanguagePicker.SelectedIndex < 0)
            {
                await DisplayAlert("Info", AppResources.PleaseSelectLanguage, "OK");
                return;
            }

            var culture = LanguagePicker.Items[LanguagePicker.SelectedIndex];
            try
            {
                var ci = new CultureInfo(culture);
                CultureInfo.DefaultThreadCurrentCulture = ci;
                CultureInfo.DefaultThreadCurrentUICulture = ci;

                Preferences.Set("AppLanguage", culture);
                // Update UI strings immediately
                UpdateUIText();

                await DisplayAlert("Language", AppResources.LanguageApplied, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not apply language: {ex.Message}", "OK");
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
