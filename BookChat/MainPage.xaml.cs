#if ANDROID
using Android.Provider;
#endif
using BookChat.Resources;

namespace BookChat
{
    public partial class MainPage : ContentPage
    {
        string currentPath = null;
        string rootPath = null;

#if ANDROID
        string androidTreeUriStr = null;
        string androidCurrentDocId = null;
        System.Collections.Generic.Stack<string> androidDocStack = new();
        System.Collections.Generic.Stack<string> androidNameStack = new();
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
        private void ShowNoLibrary()
        {
            FilesStack.Children.Add(new Label
            {
                Text = AppResources.NoLibPathConfigured,
                TextColor = Colors.Gray
            });
        }
        private async Task LoadLibPathAsync()
        {
            FilesStack.Children.Clear();

            var storedPath = Preferences.Get("LibPath", "");

            UpdateLibLabel(storedPath);

            if (string.IsNullOrEmpty(storedPath))
            {
                ShowNoLibrary();
                return;
            }

#if ANDROID
            if (storedPath.StartsWith("content://"))
            {
                await LoadAndroidLibraryAsync(storedPath);
                return;
            }
#endif

            LoadFileSystemLibrary(storedPath);
        }
        private void LoadFileSystemLibrary(string storedPath)
        {
            InitializeNavigation(storedPath);

            if (!Directory.Exists(currentPath))
            {
                ShowMessage(AppResources.PathNotFound, Colors.Red);
                return;
            }

            UpdateBreadcrumb();

            AddParentItem();

            var dirCount = AddDirectories();
            var pdfCount = AddPdfFiles();

            ShowEmptyIfNeeded(dirCount, pdfCount);
        }
        private void ShowEmptyIfNeeded(int dirCount, int fileCount)
        {
            if (dirCount == 0 && fileCount == 0)
            {
                ShowMessage(AppResources.FolderEmpty, Colors.Gray);
            }
        }
        private void UpdateBreadcrumb()
        {
            if (string.IsNullOrEmpty(rootPath) || string.IsNullOrEmpty(currentPath))
            {
                BreadcrumbLabel.Text = string.Empty;
                return;
            }

            var relative = Path.GetRelativePath(rootPath, currentPath);

            BreadcrumbLabel.Text = relative == "."
                ? Path.GetFileName(rootPath)
                : $"{Path.GetFileName(rootPath)} / {relative.Replace(Path.DirectorySeparatorChar, '/')}";
        }
        private void AddParentItem()
        {
            if (currentPath == rootPath)
                return;

            FilesStack.Children.Add(CreateItemView("..", true, () =>
            {
                var parent = Directory.GetParent(currentPath);

                if (parent == null)
                    return;

                currentPath = parent.FullName;
                _ = LoadLibPathAsync();
            }));
        }
        private int AddDirectories()
        {
            var count = 0;
            foreach (var dir in Directory.GetDirectories(currentPath))
            {
                FilesStack.Children.Add(
                    CreateItemView(Path.GetFileName(dir), true, () =>
                    {
                        currentPath = dir;
                        _ = LoadLibPathAsync();
                    }));
                count++;
            }
            return count;
        }
        private void UpdateLibLabel(string storedPath)
        {
            LibPathLabel.Text = string.IsNullOrEmpty(storedPath)
                ? AppResources.NoLibPathConfigured
                : $"{AppResources.LibPathLabel}: {storedPath}";
        }
        private int AddPdfFiles()
        {
            var count = 0;
            foreach (var file in Directory.GetFiles(currentPath, "*.pdf"))
            {
                FilesStack.Children.Add(
                    CreateItemView(Path.GetFileName(file), false));
                count++;
            }
            return count;
        }
        private void InitializeNavigation(string storedPath)
        {
            if (rootPath == storedPath)
                return;

            rootPath = storedPath;
            currentPath = storedPath;
        }
#if ANDROID
        private class AndroidEntry
        {
            public string Name { get; set; }
            public string DocumentId { get; set; }
            public bool IsDirectory { get; set; }
        }
        private void UpdateAndroidBreadcrumb(string storedPath)
        {
            BreadcrumbLabel.Text = androidNameStack.Count > 0
                ? $"{storedPath} / {string.Join("/", androidNameStack.Reverse())}"
                : storedPath;
        }
        private async Task LoadAndroidLibraryAsync(string storedPath)
        {
            UpdateAndroidBreadcrumb(storedPath);

            InitializeAndroidTree(storedPath);

            try
            {
                var uri = Android.Net.Uri.Parse(androidTreeUriStr);

                if (androidCurrentDocId == null)
                {
                    androidCurrentDocId =
                        Android.Provider.DocumentsContract.GetTreeDocumentId(uri);
                }

                var items = await EnumerateAndroidContentUriAsync(
                    androidTreeUriStr,
                    androidCurrentDocId);

                if (items.Count == 0)
                {
                    ShowMessage(AppResources.FolderReadError, Colors.Gray);
                    return;
                }

                AddAndroidParent(uri);

                foreach (var item in items)
                {
                    AddAndroidItem(item);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(
                    string.Format(AppResources.ErrorReadingContentUri, ex.Message),
                    Colors.Red);
            }
        }

        private void AddAndroidItem(AndroidEntry item)
        {
            FilesStack.Children.Add(CreateItemView(item.Name, item.IsDirectory, () =>
            {
                if (item.IsDirectory)
                {
                    androidDocStack.Push(androidCurrentDocId);
                    androidNameStack.Push(item.Name);
                    androidCurrentDocId = item.DocumentId;
                    _ = LoadLibPathAsync();
                }
            }));
        }

        private void AddAndroidParent(Android.Net.Uri uri)
        {
            var rootDoc = DocumentsContract.GetTreeDocumentId(uri);

            if (androidCurrentDocId == rootDoc && androidDocStack.Count == 0)
                return;

            FilesStack.Children.Add(CreateItemView("..", true, NavigateAndroidUp));
        }
        private void NavigateAndroidUp()
        {
            var rootDoc = DocumentsContract.GetTreeDocumentId(
                Android.Net.Uri.Parse(androidTreeUriStr));

            androidCurrentDocId = androidDocStack.Count > 0
                ? androidDocStack.Pop()
                : rootDoc;

            if (androidNameStack.Count > 0)
                androidNameStack.Pop();

            _ = LoadLibPathAsync();
        }
        private void InitializeAndroidTree(string storedPath)
        {
            if (androidTreeUriStr == storedPath)
                return;

            androidTreeUriStr = storedPath;
            androidCurrentDocId = null;
            androidDocStack.Clear();
            androidNameStack.Clear();
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

        private View CreateItemView(string name, bool isFolder, Action onTapped = null)
        {
            var icon = isFolder ? "📁" : "📙";
            var iconLabel = new Label
            {
                Text = icon,
                FontSize = 16,
                VerticalTextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0),
                //TextColor = isFolder ? Colors.Black : Colors.Red // Đã bỏ TextColor vì emoji sẽ tự hiển thị màu gốc của nó
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
        private void ShowMessage(string text, Color color = default)
        {
            FilesStack.Children.Add(new Label
            {
                Text = text,
                TextColor = color == default ? Colors.Gray : color
            });
        }
        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
