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
        // Secondary split view state
        string secondaryCurrentPath = null;
        string secondaryRootPath = null;
        bool isSecondaryViewVisible = false;

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

        private void CloseSecondaryView()
        {
            try
            {
                SecondaryScroll.IsVisible = false;
                SecondaryFilesStack.Children.Clear();
                SecondaryBreadcrumbLabel.Text = string.Empty;
                secondaryCurrentPath = null;
                secondaryRootPath = null;
                isSecondaryViewVisible = false;
                // Restore MainGrid to single column
                MainGrid.ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Star }
                };
                MainGrid.RowDefinitions = new RowDefinitionCollection();
                Grid.SetColumn(PrimaryScroll, 0);
                Grid.SetRow(PrimaryScroll, 0);
            }
            catch { }
        }

        private void ShowSecondaryForPath(string path)
        {
            try
            {
                secondaryRootPath = rootPath;
                secondaryCurrentPath = path;

                SecondaryFilesStack.Children.Clear();

                if (!Directory.Exists(secondaryCurrentPath))
                {
                    SecondaryFilesStack.Children.Add(new Label { Text = AppResources.PathNotFound, TextColor = Colors.Red });
                    SecondaryScroll.IsVisible = true;
                    // Ensure split layout applies immediately
                    ApplySplitLayout(MainGrid.Width, MainGrid.Height);
                    return;
                }

                // Make secondary visible
                SecondaryScroll.IsVisible = true;

                // Ensure split layout applies immediately
                ApplySplitLayout(MainGrid.Width, MainGrid.Height);

                UpdateSecondaryBreadcrumb();

                AddSecondaryParentItem();

                var dirCount = AddSecondaryDirectories();
                var fileCount = AddSecondaryPdfFiles();

                if (dirCount == 0 && fileCount == 0)
                {
                    SecondaryFilesStack.Children.Add(new Label { Text = AppResources.FolderEmpty, TextColor = Colors.Gray });
                }

                AddSecondaryFolderSpacer();
                isSecondaryViewVisible = true;
            }
            catch (Exception ex)
            {
                SecondaryFilesStack.Children.Add(new Label { Text = ex.Message, TextColor = Colors.Red });
            }
        }

        private void UpdateSecondaryBreadcrumb()
        {
            if (string.IsNullOrEmpty(secondaryRootPath) || string.IsNullOrEmpty(secondaryCurrentPath))
            {
                SecondaryBreadcrumbLabel.Text = string.Empty;
                return;
            }

            var relative = Path.GetRelativePath(secondaryRootPath, secondaryCurrentPath);

            SecondaryBreadcrumbLabel.Text = relative == Const.currentFolderDot
                ? Path.GetFileName(secondaryRootPath)
                : $"{Path.GetFileName(secondaryRootPath)} / {relative.Replace(Path.DirectorySeparatorChar, Const.breadcrumbSeparatorChar)}";
        }

        private void AddSecondaryParentItem()
        {
            if (secondaryCurrentPath == secondaryRootPath)
                return;

            SecondaryFilesStack.Children.Add(CreateItemView(Const.parentFolderDots, true, () =>
            {
                var parent = Directory.GetParent(secondaryCurrentPath);

                if (parent == null)
                    return;

                secondaryCurrentPath = parent.FullName;
                _ = LoadSecondaryPathAsync();
            }, null, enableMenu: false));
        }

        private int AddSecondaryDirectories()
        {
            var count = 0;
            foreach (var dir in Directory.GetDirectories(secondaryCurrentPath))
            {
                SecondaryFilesStack.Children.Add(
                    CreateItemView(Path.GetFileName(dir), true, () =>
                    {
                        secondaryCurrentPath = dir;
                        _ = LoadSecondaryPathAsync();
                    }, dir));
                count++;
            }
            return count;
        }

        private int AddSecondaryPdfFiles()
        {
            var count = 0;
            foreach (var file in Directory.GetFiles(secondaryCurrentPath, Const.pdfFilePattern))
            {
                SecondaryFilesStack.Children.Add(
                    CreateItemView(Path.GetFileName(file), false, null, file));
                count++;
            }
            return count;
        }

        private void AddSecondaryFolderSpacer()
        {
            try
            {
                var spacerGrid = new Grid
                {
                    HeightRequest = 44,
                    BackgroundColor = Colors.Transparent,
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star }
                    }
                };

                var dots = new Label
                {
                    Text = Const.currentFolderDotMenus,
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    BackgroundColor = Colors.Transparent
                };

                spacerGrid.Children.Add(dots);

                var overlay = new Button
                {
                    Text = string.Empty,
                    BackgroundColor = Colors.Black.MultiplyAlpha(0.01f),
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Fill,
                    HeightRequest = 44,
                    BorderWidth = 0,
                    Padding = new Thickness(0),
                    InputTransparent = false,
                    IsEnabled = true
                };

                overlay.Clicked += async (s, e) =>
                {
                    var displayName = string.IsNullOrEmpty(secondaryCurrentPath) ? string.Empty : Path.GetFileName(secondaryCurrentPath);
                    await ShowContextMenuAsync(displayName, true, secondaryCurrentPath, null, true, true);
                };

                spacerGrid.Children.Add(overlay);

                SecondaryFilesStack.Children.Add(spacerGrid);
            }
            catch { }
        }

        private Task LoadSecondaryPathAsync()
        {
            return Task.Run(async () =>
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    SecondaryFilesStack.Children.Clear();
                });

                if (string.IsNullOrEmpty(secondaryCurrentPath))
                    return;

                try
                {
                    if (!Directory.Exists(secondaryCurrentPath))
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            SecondaryFilesStack.Children.Add(new Label { Text = AppResources.PathNotFound, TextColor = Colors.Red });
                        });
                        return;
                    }

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        UpdateSecondaryBreadcrumb();
                        AddSecondaryParentItem();
                        var dirCount = AddSecondaryDirectories();
                        var pdfCount = AddSecondaryPdfFiles();
                        if (dirCount == 0 && pdfCount == 0)
                        {
                            SecondaryFilesStack.Children.Add(new Label { Text = AppResources.FolderEmpty, TextColor = Colors.Gray });
                        }
                        AddSecondaryFolderSpacer();
                    });
                }
                catch { }
            });
        }

        // Open a new MainPage preloaded with the given path (used for "Open In New View")
        public MainPage(string startPath)
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(startPath))
                return;

#if ANDROID
            if (startPath.StartsWith(Const.androidContentUriPrefix))
            {
                _ = LoadAndroidLibraryAsync(startPath);
                return;
            }
#endif

            LoadFileSystemLibrary(startPath);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _ = LoadLibPathAsync();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            ApplySplitLayout(width, height);
        }

        // Ensure MainGrid uses split layout according to available size.
        private void ApplySplitLayout(double width, double height)
        {
            try
            {
                if (SecondaryScroll == null || !SecondaryScroll.IsVisible)
                    return;

                if (width <= 0 || height <= 0)
                {
                    width = this.Width;
                    height = this.Height;
                }

                if (width > height)
                {
                    // side-by-side equal columns
                    MainGrid.RowDefinitions = new RowDefinitionCollection();
                    MainGrid.ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = GridLength.Star }
                    };
                    Grid.SetRow(PrimaryScroll, 0);
                    Grid.SetColumn(PrimaryScroll, 0);
                    Grid.SetRow(SecondaryScroll, 0);
                    Grid.SetColumn(SecondaryScroll, 1);
                }
                else
                {
                    // stacked vertically
                    MainGrid.ColumnDefinitions = new ColumnDefinitionCollection();
                    MainGrid.RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Star }
                    };
                    Grid.SetColumn(PrimaryScroll, 0);
                    Grid.SetRow(PrimaryScroll, 0);
                    Grid.SetColumn(SecondaryScroll, 0);
                    Grid.SetRow(SecondaryScroll, 1);
                }

                // Force layout immediately so sizes apply without waiting for a resize
                MainGrid.InvalidateMeasure();
                PrimaryScroll.InvalidateMeasure();
                SecondaryScroll.InvalidateMeasure();
            }
            catch { }
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

            var storedPath = Preferences.Get(Const.libPathPreferenceKey, string.Empty);

            UpdateLibLabel(storedPath);

            if (string.IsNullOrEmpty(storedPath))
            {
                ShowNoLibrary();
                return;
            }

#if ANDROID
            if (storedPath.StartsWith(Const.androidContentUriPrefix))
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

            // Add an empty spacer at the end so user can long-press on the current folder area
            AddCurrentFolderSpacer();
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

            BreadcrumbLabel.Text = relative == Const.currentFolderDot
                ? Path.GetFileName(rootPath)
                : $"{Path.GetFileName(rootPath)} / {relative.Replace(Path.DirectorySeparatorChar, Const.breadcrumbSeparatorChar)}";
        }
        private void AddParentItem()
        {
            if (currentPath == rootPath)
                return;

            FilesStack.Children.Add(CreateItemView(Const.parentFolderDots, true, () =>
            {
                var parent = Directory.GetParent(currentPath);

                if (parent == null)
                    return;

                currentPath = parent.FullName;
                _ = LoadLibPathAsync();
            }, null, enableMenu: false));
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
                    }, dir));
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
            foreach (var file in Directory.GetFiles(currentPath, Const.pdfFilePattern))
            {
                FilesStack.Children.Add(
                    CreateItemView(Path.GetFileName(file), false, () => onOpen(file), file));
                count++;
            }
            return count;
        }

        private void onOpen(string file)
        {
            //Todo: Implement the logic to open the PDF file
            //throw new NotImplementedException();
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
                ? $"{storedPath} / {string.Join(Const.breadcrumbSeparator, androidNameStack.Reverse())}"
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
                    // show message but still allow parent navigation and the spacer so user can create a new folder
                    ShowMessage(AppResources.FolderReadError, Colors.Gray);
                }

                AddAndroidParent(uri);

                foreach (var item in items)
                {
                    AddAndroidItem(item);
                }

                // Add spacer for current folder long-press as well (even when folder is empty)
                AddCurrentFolderSpacer();
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
            // Pass the Android tree URI + document id as the "fullPath" marker so the context menu is shown
            // in a way that can be detected as a content:// location.
            FilesStack.Children.Add(CreateItemView(item.Name, item.IsDirectory, () =>
            {
                if (item.IsDirectory)
                {
                    androidDocStack.Push(androidCurrentDocId);
                    androidNameStack.Push(item.Name);
                    androidCurrentDocId = item.DocumentId;
                    _ = LoadLibPathAsync();
                }
                else
                {
                    onOpen(androidTreeUriStr + Const.pipeSeparator + item.DocumentId);
                }
            }, androidTreeUriStr + Const.pipeSeparator + item.DocumentId));
        }

        private void AddAndroidParent(Android.Net.Uri uri)
        {
            var rootDoc = DocumentsContract.GetTreeDocumentId(uri);

            if (androidCurrentDocId == rootDoc && androidDocStack.Count == 0)
                return;

            FilesStack.Children.Add(CreateItemView(Const.parentFolderDots, true, NavigateAndroidUp, null, enableMenu: false));
        }
        private void NavigateAndroidUp()
        {
            var rootDoc = DocumentsContract.GetTreeDocumentId(
                Android.Net.Uri.Parse(androidTreeUriStr));

            if (androidDocStack.Count > 0)
            {
                androidCurrentDocId = androidDocStack.Pop();
            }
            else
            {
                androidCurrentDocId = rootDoc ?? string.Empty;
            }

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
                                    if (mime == Const.androidDocumentDirectoryMimeType)
                                    {
                                        list.Add(new AndroidEntry { Name = name, DocumentId = docId, IsDirectory = true });
                                    }
                                    else if (string.Equals(mime, Const.applicationPdfMimeType, StringComparison.OrdinalIgnoreCase) || name.EndsWith(Const.pdfFileExtension, StringComparison.OrdinalIgnoreCase))
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

        private View CreateItemView(string name, bool isFolder, Action onTapped = null, string fullPath = null, bool enableMenu = true)
        {
            var icon = isFolder ? Const.folderEmojiGlyph : Const.bookEmojiGlyph;
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
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Fill
            };

            // Use Grid instead of StackLayout so we can use star sizing (recommended over FillAndExpand)
            var row = new Grid
            {
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions = new RowDefinitionCollection
                {
                    new RowDefinition { Height = GridLength.Auto }
                },
                ColumnSpacing = 4,
                VerticalOptions = LayoutOptions.Center
            };

            Grid.SetColumn(iconLabel, 0);
            Grid.SetColumn(nameLabel, 1);
            row.Children.Add(iconLabel);
            row.Children.Add(nameLabel);

            if (onTapped != null)
            {
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) => onTapped();
                // Attach the tap to the name label so buttons inside the row still receive touches on Android
                nameLabel.GestureRecognizers.Add(tap);
            }

            // Add a small inline menu button (three-dot) so user can open context menu on each item.
            try
            {
                if (enableMenu)
                {
                    // nameLabel already uses HorizontalOptions = Fill via Grid star column; no FillAndExpand needed.

                    var menuButton = new Button
                    {
                        Text = Const.menuButtonText,
                        FontSize = 16,
                        WidthRequest = 44,
                        HeightRequest = 36,
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.End,
                        Margin = new Thickness(8, 0, 0, 0),
                        TextColor = Colors.Black,
                        BackgroundColor = Colors.Transparent,
                        BorderWidth = 0,
                        Padding = new Thickness(0)
                    };

                    menuButton.Clicked += async (s, e) =>
                    {
                        await ShowContextMenuAsync(name, isFolder, fullPath, onTapped, false, false);

                    };
                    Grid.SetColumn(menuButton, 2);
                    row.Children.Add(menuButton);
                }
            }
            catch { }

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

        private void AddCurrentFolderSpacer()
        {
            try
            {
                // Visual spacer with centered "..." so user recognises the empty-area menu target
                var spacerGrid = new Grid
                {
                    HeightRequest = 44,
                    BackgroundColor = Colors.Transparent,
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Star }
                    }
                };

                var dots = new Label
                {
                    Text = Const.currentFolderDotMenus,
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    TextColor = Colors.Gray,
                    BackgroundColor = Colors.Transparent
                };

                spacerGrid.Children.Add(dots);

                // Overlay a transparent button to capture taps on the spacer
                var overlay = new Button
                {
                    Text = string.Empty,
                    BackgroundColor = Colors.Transparent,
                    HeightRequest = 44,
                    BorderWidth = 0,
                    Padding = new Thickness(0)
                };

                overlay.Clicked += async (s, e) =>
                {
                    var displayName = string.IsNullOrEmpty(currentPath) ? string.Empty : Path.GetFileName(currentPath);
#if ANDROID
                    string fp = null;
                    if (!string.IsNullOrEmpty(androidTreeUriStr))
                    {
                        // use tree uri + current doc id so ShowContextMenuAsync sees a content:// marker
                        fp = androidTreeUriStr + (string.IsNullOrEmpty(androidCurrentDocId) ? string.Empty : Const.pipeSeparator + androidCurrentDocId);
                    }
                    else
                    {
                        fp = currentPath;
                    }
                    await ShowContextMenuAsync(displayName, true, fp, null, true, false);
#else
                    await ShowContextMenuAsync(displayName, true, currentPath, null, true, false);
#endif
                };

                spacerGrid.Children.Add(overlay);

                FilesStack.Children.Add(spacerGrid);
            }
            catch { }
        }

        private async Task ShowContextMenuAsync(string name, bool isFolder, string fullPath, Action onOpen, bool isCurrent, bool isSecondary = false)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                // nothing else we can do
                return;
            }
            // Build menu options based on available data
            string[] options = BuildContextMenuOptions(isFolder, fullPath, isCurrent, isSecondary);

            string action = null;
            action = await DisplayActionSheetAsync(name, AppResources.Cancel, null, options);

            if (action == AppResources.MenuOpen)
            {
                onOpen?.Invoke();
                return;
            }

            try
            {
                if (action == AppResources.MenuDelete)
                {
                    bool flowControl = await DeleteItemAsync(name, isFolder, fullPath);
                    if (!flowControl)
                    {
                        return;
                    }
                }
                else if (action == AppResources.MenuRename)
                {
                    bool flowControl = await RenameFileOrFolderAsync(name, isFolder, fullPath);
                    if (!flowControl)
                    {
                        return;
                    }
                }
                else if (action == AppResources.MenuOpenInNewView && isFolder)
                {
                    // Open folder in split view (secondary pane) for filesystem paths; for content:// open a new window
                    if (!string.IsNullOrEmpty(fullPath) && fullPath.StartsWith(Const.androidContentUriPrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        // Opening a new OS window for content:// locations is not supported in this app.
                        //TODO: If needed, implement opening a new window for content:// locations on Android using platform-specific APIs.
                        await DisplayAlertAsync(AppResources.Info, AppResources.OpenInNewViewNotSupported, AppResources.Ok);
                    }
                    else
                    {
                        ShowSecondaryForPath(fullPath);
                    }
                }
                else if (action == AppResources.MenuCloseView && isSecondary)
                {
                    CloseSecondaryView();
                    return;
                }
                else if (action == AppResources.MenuNewFolder && isFolder)
                {
                    bool flowControl = await CreateNewFolderAsync(fullPath);
                    if (!flowControl)
                    {
                        return;
                    }
                }
                else if (action == AppResources.MenuMoveNewView && isFolder)
                {
                    // Move the folder to a new OS window (if supported)
                    //TODO: Implement moving to a new view if needed
                }
                else if (action == AppResources.MenuMoveNewView && !isFolder)
                {
                    // Move the file to a new OS window (if supported)
                    //TODO: Implement moving to a new view if needed
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"{AppResources.Error}: {ex.Message}", Colors.Red);
            }
        }

        private async Task<bool> CreateNewFolderAsync(string fullPath)
        {
            // If this is an Android content URI, attempt to create via DocumentsContract
            if (!string.IsNullOrEmpty(fullPath) && fullPath.StartsWith(Const.androidContentUriPrefix, StringComparison.OrdinalIgnoreCase))
            {
#if ANDROID
                var folderName = await DisplayPromptAsync(AppResources.NewFolderTitle, AppResources.NewFolderPrompt, placeholder: AppResources.NewFolderPlaceholder);
                if (string.IsNullOrWhiteSpace(folderName))
                    return false;

                try
                {
                    // fullPath format: treeUri or treeUri|docId
                    var parts = fullPath.Split(new[] { '|' }, 2);
                    var treeUri = Android.Net.Uri.Parse(parts[0]);
                    var resolver = Android.App.Application.Context.ContentResolver;

                    // Ensure we have write permission on the tree
                    bool hasWrite = false;
                    if (resolver.PersistedUriPermissions != null)
                    {
                        foreach (var p in resolver.PersistedUriPermissions)
                        {
                            if (p.Uri.Equals(treeUri) && p.IsWritePermission)
                            {
                                hasWrite = true;
                                break;
                            }
                        }
                    }
                    if (!hasWrite)
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                        return false;
                    }

                    // Parent document id for the tree
                    var treeDocId = Android.Provider.DocumentsContract.GetTreeDocumentId(treeUri);
                    var parentDocUri = Android.Provider.DocumentsContract.BuildDocumentUriUsingTree(treeUri, treeDocId);

                    var created = Android.Provider.DocumentsContract.CreateDocument(resolver, parentDocUri, "vnd.android.document/directory", folderName.Trim());
                    if (created == null)
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                        return false;
                    }

                    await LoadLibPathAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    ShowMessage($"{AppResources.Error}: {ex.Message}", Colors.Red);
                    return false;
                }
#else
                await DisplayAlertAsync(AppResources.CreateFolderNotSupported, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                return false;
#endif
            }

            // Filesystem path
            var folderNameFs = await DisplayPromptAsync(AppResources.NewFolderTitle, AppResources.NewFolderPrompt, placeholder: AppResources.NewFolderPlaceholder);
            if (string.IsNullOrWhiteSpace(folderNameFs))
                return false;

            try
            {
                var newPath = Path.Combine(fullPath ?? string.Empty, folderNameFs.Trim());
                Directory.CreateDirectory(newPath);
                await LoadLibPathAsync();
            }
            catch (Exception ex)
            {
                ShowMessage($"{AppResources.Error}: {ex.Message}", Colors.Red);
                return false;
            }

            return true;
        }

        private async Task<bool> RenameFileOrFolderAsync(string name, bool isFolder, string fullPath)
        {
            if (!string.IsNullOrEmpty(fullPath) && fullPath.StartsWith(Const.androidContentUriPrefix, StringComparison.OrdinalIgnoreCase))
            {
#if ANDROID
                // Check persisted write permission for the selected tree
                var partsCheck = fullPath.Split(new[] { '|' }, 2);
                var treeUriCheck = Android.Net.Uri.Parse(partsCheck[0]);
                var resolverCheck = Android.App.Application.Context.ContentResolver;
                bool hasWriteCheck = false;
                foreach (var p in resolverCheck.PersistedUriPermissions)
                {
                    if (p.Uri.Equals(treeUriCheck) && p.IsWritePermission)
                    {
                        hasWriteCheck = true;
                        break;
                    }
                }
                if (!hasWriteCheck)
                {
                    await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                    return false;
                }

                var result = await DisplayPromptAsync(AppResources.RenameTitle, AppResources.RenamePrompt, initialValue: Path.GetFileNameWithoutExtension(name)) + Const.pdfFileExtension;
                if (string.IsNullOrEmpty(result)) return false;

                try
                {
                    var parts = fullPath.Split(new[] { '|' }, 2);
                    var treeUri = Android.Net.Uri.Parse(parts[0]);
                    var docId = parts.Length > 1 ? parts[1] : null;
                    if (string.IsNullOrEmpty(docId))
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                        return false;
                    }

                    var resolver = Android.App.Application.Context.ContentResolver;
                    var docUri = Android.Provider.DocumentsContract.BuildDocumentUriUsingTree(treeUri, docId);
                    var renamed = Android.Provider.DocumentsContract.RenameDocument(resolver, docUri, result);
                    if (renamed == null)
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                        return false;
                    }

                    await LoadLibPathAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    ShowMessage($"{AppResources.Error}: {ex.Message}", Colors.Red);
                    return false;
                }
#else
                await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                return false;
#endif
            }

            var resultFs = await DisplayPromptAsync(AppResources.RenameTitle, AppResources.RenamePrompt, initialValue: Path.GetFileNameWithoutExtension(name)) + Const.pdfFileExtension;
            if (string.IsNullOrEmpty(resultFs)) return false;

            var parent = Path.GetDirectoryName(fullPath) ?? string.Empty;
            var newPath = Path.Combine(parent, resultFs);

            if (isFolder)
                Directory.Move(fullPath, newPath);
            else
                File.Move(fullPath, newPath);

            await LoadLibPathAsync();
            return true;
        }

        private async Task<bool> DeleteItemAsync(string name, bool isFolder, string fullPath)
        {
            if (!string.IsNullOrEmpty(fullPath) && fullPath.StartsWith(Const.androidContentUriPrefix, StringComparison.OrdinalIgnoreCase))
            {
#if ANDROID
                // Check write permission first
                var partsCheck = fullPath.Split(new[] { '|' }, 2);
                var treeUriCheck = Android.Net.Uri.Parse(partsCheck[0]);
                var resolverCheck = Android.App.Application.Context.ContentResolver;
                bool hasWriteCheck = false;
                if (resolverCheck.PersistedUriPermissions != null)
                {
                    foreach (var p in resolverCheck.PersistedUriPermissions)
                    {
                        if (p.Uri.Equals(treeUriCheck) && p.IsWritePermission)
                        {
                            hasWriteCheck = true;
                            break;
                        }
                    }
                }
                if (!hasWriteCheck)
                {
                    await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                    return false;
                }

                var confirm = await DisplayAlertAsync(AppResources.Confirm, string.Format(AppResources.ConfirmDeleteMessage, name), AppResources.Yes, AppResources.No);
                if (!confirm) return false;

                try
                {
                    var parts = fullPath.Split(new[] { '|' }, 2);
                    var treeUri = Android.Net.Uri.Parse(parts[0]);
                    var docId = parts.Length > 1 ? parts[1] : null;
                    if (string.IsNullOrEmpty(docId))
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                        return false;
                    }

                    var resolver = Android.App.Application.Context.ContentResolver;
                    var docUri = Android.Provider.DocumentsContract.BuildDocumentUriUsingTree(treeUri, docId);
                    var deleted = Android.Provider.DocumentsContract.DeleteDocument(resolver, docUri);
                    if (!deleted)
                    {
                        await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                        return false;
                    }

                    await LoadLibPathAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    ShowMessage($"{AppResources.Error}: {ex.Message}", Colors.Red);
                    return false;
                }
#else
                await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                return false;
#endif
            }

            var confirmFs = await DisplayAlertAsync(AppResources.Confirm, string.Format(AppResources.ConfirmDeleteMessage, name), AppResources.Yes, AppResources.No);
            if (!confirmFs) return false;

            if (isFolder)
                Directory.Delete(fullPath, true);
            else
                File.Delete(fullPath);

            await LoadLibPathAsync();
            return true;
        }

        private string[] BuildContextMenuOptions(bool isFolder, string fullPath, bool isCurrent, bool isSecondary = false)
        {
            string[] options;
            if (string.IsNullOrEmpty(fullPath))
            {
                options = new[] { AppResources.MenuOpen };
            }
            // for folders (when invoked from the empty area spacer), allow New Folder, open, rename, delete, open in new view
            else if (isFolder)
            {
                if (isCurrent)
                {
                    if (isSecondaryViewVisible)
                        options = [AppResources.MenuNewFolder];
                    else
                        options = [AppResources.MenuNewFolder, AppResources.MenuOpenInNewView];
                }
                else
                {
                    if (isSecondaryViewVisible)
                        options = [AppResources.MenuOpen, AppResources.MenuRename, AppResources.MenuDelete, AppResources.MenuMoveNewView];
                    else
                        options = [AppResources.MenuOpen, AppResources.MenuRename, AppResources.MenuDelete, AppResources.MenuOpenInNewView];
                }
            }
            // for files, allow open, rename, delete
            else
            {
                if (isSecondaryViewVisible)
                    options = [AppResources.MenuOpen, AppResources.MenuRename, AppResources.MenuDelete, AppResources.MenuMoveNewView];
                else
                    options = [AppResources.MenuOpen, AppResources.MenuRename, AppResources.MenuDelete];
            }

            // If this is the secondary view, add Close View option at the end
            if (isSecondary)
            {
                var list = new System.Collections.Generic.List<string>(options);
                list.Add(AppResources.MenuCloseView);
                return list.ToArray();
            }

            return options;
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
