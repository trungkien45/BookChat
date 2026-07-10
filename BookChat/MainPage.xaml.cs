using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using BookChat.Resources;

namespace BookChat
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadLibPathAsync();
        }

        private async System.Threading.Tasks.Task LoadLibPathAsync()
        {
            var filesStack = this.FindByName<StackLayout>("FilesStack");
            var libLabel = this.FindByName<Label>("LibPathLabel");

            if (filesStack == null || libLabel == null)
            {
                // If names aren't found in XAML, abort safely.
                return;
            }

            filesStack.Children.Clear();

            var path = Microsoft.Maui.Storage.Preferences.Get("LibPath", string.Empty);
            libLabel.Text = string.IsNullOrEmpty(path) ? AppResources.NoLibPathConfigured : $"{AppResources.LibPathLabel}: {path}";

            if (string.IsNullOrEmpty(path))
            {
                filesStack.Children.Add(new Label { Text = AppResources.NoLibPathConfigured, TextColor = Colors.Gray });
                return;
            }

#if ANDROID
            if (path.StartsWith("content://"))
            {
                try
                {
                    var items = await EnumerateAndroidContentUriAsync(path);
                    if (items.Count == 0)
                    {
                        filesStack.Children.Add(new Label { Text = AppResources.FolderReadError, TextColor = Colors.Gray });
                    }
                    else
                    {
                        foreach (var it in items)
                        {
                            filesStack.Children.Add(new Label { Text = it, LineBreakMode = LineBreakMode.TailTruncation });
                        }
                    }
                }
                catch (Exception ex)
                {
                    filesStack.Children.Add(new Label { Text = string.Format(AppResources.ErrorReadingContentUri, ex.Message), TextColor = Colors.Red });
                }
                return;
            }
#endif

            try
            {
                if (System.IO.Directory.Exists(path))
                {
                    var dirs = System.IO.Directory.GetDirectories(path);
                    var files = System.IO.Directory.GetFiles(path);

                    if (dirs.Length == 0 && files.Length == 0)
                    {
                        filesStack.Children.Add(new Label { Text = AppResources.FolderEmpty, TextColor = Colors.Gray });
                        return;
                    }

                    foreach (var d in dirs)
                    {
                        filesStack.Children.Add(CreateItemView(System.IO.Path.GetFileName(d), true));
                    }
                    foreach (var f in files)
                    {
                        filesStack.Children.Add(CreateItemView(System.IO.Path.GetFileName(f), false));
                    }
                }
                else
                {
                    filesStack.Children.Add(new Label { Text = AppResources.PathNotFound, TextColor = Colors.Red });
                }
            }
            catch (Exception ex)
            {
                filesStack.Children.Add(new Label { Text = string.Format(AppResources.ErrorReadingPath, ex.Message), TextColor = Colors.Red });
            }
        }

#if ANDROID
        private System.Threading.Tasks.Task<System.Collections.Generic.List<string>> EnumerateAndroidContentUriAsync(string uriStr)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                var list = new System.Collections.Generic.List<string>();
                try
                {
                    var uri = Android.Net.Uri.Parse(uriStr);
                    var resolver = Android.App.Application.Context.ContentResolver;

                    // Build children URI
                    var treeDocId = Android.Provider.DocumentsContract.GetTreeDocumentId(uri);
                    var childrenUri = Android.Provider.DocumentsContract.BuildChildDocumentsUriUsingTree(uri, treeDocId);

                    string[] projection = new[] { Android.Provider.DocumentsContract.Document.ColumnDisplayName, Android.Provider.DocumentsContract.Document.ColumnMimeType };
                    using (var cursor = resolver.Query(childrenUri, projection, null, null, null))
                    {
                        if (cursor != null)
                        {
                            int nameIndex = cursor.GetColumnIndex(Android.Provider.DocumentsContract.Document.ColumnDisplayName);
                            int mimeIndex = cursor.GetColumnIndex(Android.Provider.DocumentsContract.Document.ColumnMimeType);
                            while (cursor.MoveToNext())
                            {
                                var name = cursor.GetString(nameIndex);
                                var mime = cursor.GetString(mimeIndex);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    if (mime == "vnd.android.document/directory")
                                        list.Add(name + "/");
                                    else
                                        list.Add(name);
                                }
                            }
                        }
                    }
                }
                catch { }
                return list;
            });
        }
#if NET6_0_OR_GREATER
        // Create a small row showing an icon (emoji) and the name. Using emoji avoids adding image assets.
        private View CreateItemView(string name, bool isFolder)
        {
            var icon = isFolder ? "📁" : "📄";
            var iconLabel = new Label
            {
                Text = icon,
                FontSize = 16,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var nameLabel = new Label
            {
                Text = name,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };

            return new HorizontalStackLayout
            {
                Spacing = 4,
                Children = { iconLabel, nameLabel }
            };
        }
#else
        private View CreateItemView(string name, bool isFolder)
        {
            // Fallback: simple text with slash for folder
            var text = isFolder ? name + "/" : name;
            return new Label { Text = text };
        }
#endif
#endif

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
