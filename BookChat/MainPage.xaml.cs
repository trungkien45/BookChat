using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using BookChat.Resources;
using System.Linq;

namespace BookChat
{
    public partial class MainPage : ContentPage
    {
#if ANDROID
        // Current navigation state
        string currentPath = null; // for filesystem navigation
        string rootPath = null;
#else
        // Current navigation state
        string currentPath = null; // for filesystem navigation
        string rootPath = null;
#endif
#if ANDROID
        string androidTreeUriStr = null;
        string androidCurrentDocId = null;
        System.Collections.Generic.Stack<string> androidDocStack = new System.Collections.Generic.Stack<string>();
#endif
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

            var storedPath = Microsoft.Maui.Storage.Preferences.Get("LibPath", string.Empty);
            libLabel.Text = string.IsNullOrEmpty(storedPath) ? AppResources.NoLibPathConfigured : $"{AppResources.LibPathLabel}: {storedPath}";

            if (string.IsNullOrEmpty(storedPath))
            {
                filesStack.Children.Add(new Label { Text = AppResources.NoLibPathConfigured, TextColor = Colors.Gray });
                return;
            }

#if ANDROID
            if (storedPath.StartsWith("content://"))
            {
                // Initialize android navigation state if root changed
                if (androidTreeUriStr == null || androidTreeUriStr != storedPath)
                {
                    androidTreeUriStr = storedPath;
                    androidCurrentDocId = null;
                    androidDocStack.Clear();
                }

                try
                {
                    var uri = Android.Net.Uri.Parse(androidTreeUriStr);
                    if (androidCurrentDocId == null)
                        androidCurrentDocId = Android.Provider.DocumentsContract.GetTreeDocumentId(uri);

                    var items = await EnumerateAndroidContentUriAsync(androidTreeUriStr, androidCurrentDocId);
                    if (items.Count == 0)
                    {
                        filesStack.Children.Add(new Label { Text = AppResources.FolderReadError, TextColor = Colors.Gray });
                    }
                    else
                    {
                        // Show parent .. when not at tree root
                        var rootDoc = Android.Provider.DocumentsContract.GetTreeDocumentId(uri);
                        if (androidCurrentDocId != rootDoc || androidDocStack.Count > 0)
                        {
                            filesStack.Children.Add(CreateItemView("..", true, () =>
                            {
                                if (androidDocStack.Count > 0)
                                {
                                    androidCurrentDocId = androidDocStack.Pop();
                                }
                                else
                                {
                                    androidCurrentDocId = rootDoc;
                                }
                                _ = LoadLibPathAsync();
                            }));
                        }

                        foreach (var it in items)
                        {
                            filesStack.Children.Add(CreateItemView(it.Name, it.IsDirectory, () =>
                            {
                                if (it.IsDirectory)
                                {
                                    androidDocStack.Push(androidCurrentDocId);
                                    androidCurrentDocId = it.DocumentId;
                                    _ = LoadLibPathAsync();
                                }
                                else
                                {
                                    // TODO: open file or handle selection
                                }
                            }));
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
                // Filesystem navigation
                if (!storedPath.StartsWith("content://"))
                {
                    if (rootPath == null || rootPath != storedPath)
                    {
                        rootPath = storedPath;
                        currentPath = rootPath;
                    }

                    if (System.IO.Directory.Exists(currentPath))
                    {
                        // Show directories and only PDF files
                        var dirs = System.IO.Directory.GetDirectories(currentPath);
                        var files = System.IO.Directory.GetFiles(currentPath)
                            .Where(f => string.Equals(System.IO.Path.GetExtension(f), ".pdf", StringComparison.OrdinalIgnoreCase))
                            .ToArray();

                        if (dirs.Length == 0 && files.Length == 0)
                        {
                            filesStack.Children.Add(new Label { Text = AppResources.FolderEmpty, TextColor = Colors.Gray });
                            return;
                        }

                        // add parent link if not at root
                        if (!string.IsNullOrEmpty(rootPath) && !string.Equals(currentPath?.TrimEnd(System.IO.Path.DirectorySeparatorChar), rootPath?.TrimEnd(System.IO.Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))
                        {
                            filesStack.Children.Add(CreateItemView("..", true, () =>
                            {
                                var parent = System.IO.Directory.GetParent(currentPath);
                                if (parent != null)
                                {
                                    currentPath = parent.FullName;
                                    _ = LoadLibPathAsync();
                                }
                            }));
                        }

                        foreach (var d in dirs)
                        {
                            var full = d;
                            filesStack.Children.Add(CreateItemView(System.IO.Path.GetFileName(d), true, () =>
                            {
                                currentPath = full;
                                _ = LoadLibPathAsync();
                            }));
                        }
                        foreach (var f in files)
                        {
                            var full = f;
                            filesStack.Children.Add(CreateItemView(System.IO.Path.GetFileName(f), false, () =>
                            {
                                // TODO: open file or handle selection
                            }));
                        }
                    }
                    else
                    {
                        filesStack.Children.Add(new Label { Text = AppResources.PathNotFound, TextColor = Colors.Red });
                    }
                }
            }
            catch (Exception ex)
            {
                filesStack.Children.Add(new Label { Text = string.Format(AppResources.ErrorReadingPath, ex.Message), TextColor = Colors.Red });
            }
        }

#if ANDROID
        private class AndroidEntry
        {
            public string Name { get; set; }
            public string DocumentId { get; set; }
            public bool IsDirectory { get; set; }
        }

        private System.Threading.Tasks.Task<System.Collections.Generic.List<AndroidEntry>> EnumerateAndroidContentUriAsync(string uriStr, string parentDocId)
        {
            return System.Threading.Tasks.Task.Run(() =>
            {
                var list = new System.Collections.Generic.List<AndroidEntry>();
                try
                {
                    var uri = Android.Net.Uri.Parse(uriStr);
                    var resolver = Android.App.Application.Context.ContentResolver;

                    var childrenUri = Android.Provider.DocumentsContract.BuildChildDocumentsUriUsingTree(uri, parentDocId);

                    string[] projection = new[] { Android.Provider.DocumentsContract.Document.ColumnDocumentId, Android.Provider.DocumentsContract.Document.ColumnDisplayName, Android.Provider.DocumentsContract.Document.ColumnMimeType };
                    using (var cursor = resolver.Query(childrenUri, projection, null, null, null))
                    {
                        if (cursor != null)
                        {
                            int idIndex = cursor.GetColumnIndex(Android.Provider.DocumentsContract.Document.ColumnDocumentId);
                            int nameIndex = cursor.GetColumnIndex(Android.Provider.DocumentsContract.Document.ColumnDisplayName);
                            int mimeIndex = cursor.GetColumnIndex(Android.Provider.DocumentsContract.Document.ColumnMimeType);
                            while (cursor.MoveToNext())
                            {
                                var docId = cursor.GetString(idIndex);
                                var name = cursor.GetString(nameIndex);
                                var mime = cursor.GetString(mimeIndex);
                                if (!string.IsNullOrEmpty(name))
                                {
                                    if (mime == "vnd.android.document/directory")
                                    {
                                        list.Add(new AndroidEntry { Name = name, DocumentId = docId, IsDirectory = true });
                                    }
                                    else if (string.Equals(mime, "application/pdf", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                                    {
                                        list.Add(new AndroidEntry { Name = name, DocumentId = docId, IsDirectory = false });
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
                return list;
            });
        }
#endif

        // Create a small row showing an icon (emoji) and the name. Using emoji avoids adding image assets.
        private View CreateItemView(string name, bool isFolder, Action onTapped = null)
        {
            var icon = isFolder ? "📁" : "📄";
            var iconLabel = new Label
            {
                Text = icon,
                FontSize = 16,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
                TextColor = isFolder ? Colors.Black : Colors.Red // PDFs show red icon
            };

            var nameLabel = new Label
            {
                Text = name,
                VerticalTextAlignment = TextAlignment.Center,
                LineBreakMode = LineBreakMode.TailTruncation
            };

            var row = new HorizontalStackLayout
            {
                Spacing = 4,
                Children = { iconLabel, nameLabel }
            };

            if (onTapped != null)
            {
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) => onTapped();
                row.GestureRecognizers.Add(tap);
            }

            return row;
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
