using BookChat.Data.Providers;
using BookChat.Data.Service;
using BookChat.Resources;
using BookChat.StorageService;
using BookChat.StorageService.Inteface;

namespace BookChat
{
    public partial class MainPage : ContentPage
    {
        private readonly IStogareService _storageService;
        private readonly IBookService _bookService;
        private readonly IDbSessionFactory _dbSessionFactory;
        StorageItem? rootItem = null;
        StorageItem? currentItem = null;
        Stack<StorageItem> navStack = new();

        // Secondary split view state
        StorageItem? secondaryRootItem = null;
        StorageItem? secondaryCurrentItem = null;
        Stack<StorageItem> secondaryNavStack = new();
        bool isSecondaryViewVisible = false;

        public MainPage(IStogareService storageService, IBookService bookService, IDbSessionFactory dbSessionFactory)
        {
            InitializeComponent();
            _storageService = storageService;
            _bookService = bookService;
            _dbSessionFactory = dbSessionFactory;
        }

        private async Task<StorageItem> CreateInitialStorageItem(string storedPath)
        {
            return await _storageService.GetFromId(storedPath, storedPath) ??
                new StorageItem 
            { 
                Id = storedPath, 
                DocumentId = "", 
                DisplayName = Path.GetFileName(storedPath),
                IsDirectory = true 
            };
        }

        private void CloseSecondaryView()
        {
            try
            {
                SecondaryScroll.IsVisible = false;
                SecondaryFilesStack.Children.Clear();
                SecondaryBreadcrumbLabel.Text = string.Empty;
                secondaryCurrentItem = null;
                secondaryRootItem = null;
                secondaryNavStack.Clear();
                isSecondaryViewVisible = false;

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

        private void ShowSecondaryForPath(StorageItem item)
        {
            try
            {
                // Always reset secondary navigation to the opened folder as root
                secondaryRootItem = item;
                secondaryCurrentItem = item;
                secondaryNavStack.Clear();

                SecondaryFilesStack.Children.Clear();
                SecondaryScroll.IsVisible = true;
                isSecondaryViewVisible = true;
                ApplySplitLayout(MainGrid.Width, MainGrid.Height);

                _ = LoadSecondaryPathAsync();
            }
            catch (Exception ex)
            {
                SecondaryFilesStack.Children.Add(new Label { Text = ex.Message, TextColor = Colors.Red });
            }
        }

        private void UpdateSecondaryBreadcrumb()
        {
            if (secondaryRootItem == null || secondaryCurrentItem == null)
            {
                SecondaryBreadcrumbLabel.Text = string.Empty;
                return;
            }

            var pathNames = new List<string>();
            foreach (var item in secondaryNavStack)
            {
                pathNames.Add(string.IsNullOrEmpty(item.DisplayName) ? item.Id : item.DisplayName);
            }
            pathNames.Reverse();
            if (!string.IsNullOrEmpty(secondaryCurrentItem.DisplayName) && secondaryCurrentItem != secondaryRootItem)
            {
                pathNames.Add(secondaryCurrentItem.DisplayName);
            }

            var rootName = string.IsNullOrEmpty(secondaryRootItem.DisplayName) ? secondaryRootItem.Id : secondaryRootItem.DisplayName;
            pathNames.Remove(rootName);

            if (pathNames.Count == 0)
                SecondaryBreadcrumbLabel.Text = rootName;
            else
                SecondaryBreadcrumbLabel.Text = $"{rootName}/{string.Join(Const.breadcrumbSeparator, pathNames)}";
        }

        private void AddSecondaryParentItem()
        {
            if (secondaryCurrentItem == null || secondaryRootItem == null)
                return;

            if (secondaryCurrentItem.Id == secondaryRootItem.Id)
                return;

            SecondaryFilesStack.Children.Add(CreateItemView(Const.parentFolderDots, true, () =>
            {
                if (secondaryNavStack.Count > 0)
                {
                    secondaryCurrentItem = secondaryNavStack.Pop();
                }
                else
                {
                    secondaryCurrentItem = secondaryRootItem;
                }
                _ = LoadSecondaryPathAsync();
            }, null, enableMenu: false));
        }

        private async Task LoadSecondaryPathAsync()
        {
            if (secondaryCurrentItem == null) return;

            var items = await _storageService.GetPdfFilesAndFolders(secondaryCurrentItem);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SecondaryFilesStack.Children.Clear();

                if (items == null || items.Count == 0)
                {
                    SecondaryFilesStack.Children.Add(new Label { Text = AppResources.FolderEmpty, TextColor = Colors.Gray });
                }

                UpdateSecondaryBreadcrumb();
                AddSecondaryParentItem();

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var capturedItem = item; // capture loop variable to avoid closure bug
                        SecondaryFilesStack.Children.Add(CreateItemView(
                            string.IsNullOrEmpty(capturedItem.DisplayName) ? capturedItem.DocumentId : capturedItem.DisplayName,
                            capturedItem.IsDirectory,
                            () =>
                            {
                                if (capturedItem.IsDirectory)
                                {
                                    secondaryNavStack.Push(secondaryCurrentItem);
                                    secondaryCurrentItem = capturedItem;
                                    _ = LoadSecondaryPathAsync();
                                }
                                else
                                {
                                    onOpen(capturedItem);
                                }
                            },
                            capturedItem));
                    }
                }

                AddSecondaryFolderSpacer();
            });
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
                    var displayName = secondaryCurrentItem?.DisplayName ?? string.Empty;
                    await ShowContextMenuAsync(displayName, true, secondaryCurrentItem, null, true, true);
                };

                spacerGrid.Children.Add(overlay);
                SecondaryFilesStack.Children.Add(spacerGrid);
            }
            catch { }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadLibPathAsync();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            ApplySplitLayout(width, height);
        }

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

            var initialItem = await CreateInitialStorageItem(storedPath);

            // Initialize navigation state only here (root entry point)
            rootItem = initialItem;
            currentItem = initialItem;
            navStack.Clear();
            if (rootItem != null)
            {
                var allPdfItems = await _storageService.GetPdfFilesAndFolders(rootItem, true);
                var pdfFiles = allPdfItems.Select(x => x.Id).ToList();
                await _bookService.SyncBooksAsync(pdfFiles);
            }

            await LoadFileSystemLibrary();
        }

        private async Task LoadFileSystemLibrary()
        {
            if (currentItem == null) return;

            var items = await _storageService.GetPdfFilesAndFolders(currentItem);

            // Clear before re-rendering
            FilesStack.Children.Clear();

            // Show ".." only when NOT at root
            AddParentItem();
            UpdateBreadcrumb();

            if (items == null || items.Count == 0)
            {
                ShowMessage(AppResources.FolderEmpty, Colors.Gray);
            }
            else
            {
                foreach (var item in items)
                {
                    var capturedItem = item; // capture loop variable to avoid closure bug
                    FilesStack.Children.Add(CreateItemView(
                        string.IsNullOrEmpty(capturedItem.DisplayName) ? capturedItem.DocumentId : capturedItem.DisplayName,
                        capturedItem.IsDirectory,
                        () =>
                        {
                            if (capturedItem.IsDirectory)
                            {
                                navStack.Push(currentItem);
                                currentItem = capturedItem;
                                _ = LoadFileSystemLibrary();
                            }
                            else
                            {
                                onOpen(capturedItem);
                            }
                        },
                        capturedItem));
                }
            }

            AddCurrentFolderSpacer();
        }

        private void UpdateBreadcrumb()
        {
            if (rootItem == null || currentItem == null)
            {
                BreadcrumbLabel.Text = string.Empty;
                return;
            }

            var pathNames = new List<string>();
            foreach (var item in navStack)
            {
                pathNames.Add(string.IsNullOrEmpty(item.DisplayName) ? item.Id : item.DisplayName);
            }
            pathNames.Reverse();
            if (!string.IsNullOrEmpty(currentItem.DisplayName) && currentItem != rootItem)
            {
                pathNames.Add(currentItem.DisplayName);
            }
            var rootName = string.IsNullOrEmpty(rootItem.DisplayName) ? rootItem.Id : rootItem.DisplayName;
            pathNames.Remove(rootName);

            if (pathNames.Count == 0)
                BreadcrumbLabel.Text = rootName;
            else
                BreadcrumbLabel.Text = $"{rootName}/{string.Join(Const.breadcrumbSeparator, pathNames)}";
        }

        private void AddParentItem()
        {
            if (currentItem == null || rootItem == null)
                return;

            // Don't show ".." at root level
            if (currentItem.Id == rootItem.Id)
                return;

            FilesStack.Children.Add(CreateItemView(Const.parentFolderDots, true, () =>
            {
                if (navStack.Count > 0)
                    currentItem = navStack.Pop();
                else
                    currentItem = rootItem;

                _ = LoadFileSystemLibrary();
            }, null, enableMenu: false));
        }

        private void UpdateLibLabel(string storedPath)
        {
            LibPathLabel.Text = string.IsNullOrEmpty(storedPath)
                ? AppResources.NoLibPathConfigured
                : $"{AppResources.LibPathLabel}: {storedPath}";
        }

        private async void onOpen(StorageItem file)
        {
            //Todo: Implement the logic to open the PDF file
            await Shell.Current.GoToAsync(nameof(ViewBook), new ShellNavigationQueryParameters
            {
                ["File"] = file
            });
        }


        private View CreateItemView(string name, bool isFolder, Action? onTapped = null, StorageItem? item = null, bool enableMenu = true)
        {
            var icon = isFolder ? Const.folderEmojiGlyph : Const.bookEmojiGlyph;
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
                LineBreakMode = LineBreakMode.TailTruncation,
                HorizontalOptions = LayoutOptions.Fill
            };

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
                nameLabel.GestureRecognizers.Add(tap);
            }

            try
            {
                if (enableMenu)
                {
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
                        await ShowContextMenuAsync(name, isFolder, item, onTapped, false, false);
                    };
                    Grid.SetColumn(menuButton, 2);
                    row.Children.Add(menuButton);
                }
            }
            catch { }

            return row;
        }

        private void ShowMessage(string text, Color color)
        {
            FilesStack.Children.Add(new Label
            {
                Text = text,
                TextColor = color
            });
        }

        private void AddCurrentFolderSpacer()
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
                    BackgroundColor = Colors.Transparent,
                    HeightRequest = 44,
                    BorderWidth = 0,
                    Padding = new Thickness(0)
                };

                overlay.Clicked += async (s, e) =>
                {
                    var displayName = currentItem?.DisplayName ?? string.Empty;
                    await ShowContextMenuAsync(displayName, true, currentItem, null, true, false);
                };

                spacerGrid.Children.Add(overlay);
                FilesStack.Children.Add(spacerGrid);
            }
            catch { }
        }

        private async Task ShowContextMenuAsync(string name, bool isFolder, StorageItem? item, Action? onOpen, bool isCurrent, bool isSecondary = false)
        {
            if (item == null)
            {
                return;
            }

            string[] options = BuildContextMenuOptions(isFolder, item, isCurrent, isSecondary);

            string action = await DisplayActionSheetAsync(name, AppResources.Cancel, null, options);

            if (action == AppResources.MenuOpen)
            {
                onOpen?.Invoke();
                return;
            }

            try
            {
                if (action == AppResources.MenuDelete)
                {
                    bool flowControl = await DeleteItemAsync(name, item);
                    if (!flowControl) return;
                }
                else if (action == AppResources.MenuRename)
                {
                    bool flowControl = await RenameFileOrFolderAsync(name, item);
                    if (!flowControl) return;
                }
                else if (action == AppResources.MenuOpenInNewView && isFolder)
                {
                    ShowSecondaryForPath(item);
                }
                else if (action == AppResources.MenuCloseView && isSecondary)
                {
                    CloseSecondaryView();
                    return;
                }
                else if (action == AppResources.MenuNewFolder && isFolder)
                {
                    bool flowControl = await CreateNewFolderAsync(item);
                    if (!flowControl) return;
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

        private async Task<bool> CreateNewFolderAsync(StorageItem parentItem)
        {
            var folderName = await DisplayPromptAsync(AppResources.NewFolderTitle, AppResources.NewFolderPrompt, placeholder: AppResources.NewFolderPlaceholder);
            if (string.IsNullOrWhiteSpace(folderName))
                return false;

            var success = await _storageService.CreateFolder(parentItem, folderName.Trim());
            if (!success)
            {
                await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                return false;
            }

            await RefreshPrimaryAndSecondaryAsync();
            return true;
        }

        private async Task<bool> RenameFileOrFolderAsync(string name, StorageItem item)
        {
            var result = await DisplayPromptAsync(AppResources.RenameTitle, AppResources.RenamePrompt, initialValue: Path.GetFileNameWithoutExtension(name));

            if (string.IsNullOrEmpty(result)) return false;

            if (!item.IsDirectory)
                result = result + Const.pdfFileExtension;

            var success = await _storageService.Rename(item, result);
            if (!success)
            {
                await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                return false;
            }

            if (rootItem != null)
            {
                var allPdfItems = await _storageService.GetPdfFilesAndFolders(rootItem, true);
                var pdfFiles = allPdfItems.Select(x => x.Id).ToList();
                await _bookService.SyncBooksAsync(pdfFiles);
            }

            await RefreshPrimaryAndSecondaryAsync();
            return true;
        }

        private async Task RefreshPrimaryAndSecondaryAsync()
        {
            if (currentItem != null)
                await LoadFileSystemLibrary();

            if (isSecondaryViewVisible && secondaryCurrentItem != null)
                await LoadSecondaryPathAsync();
        }

        private async Task<bool> DeleteItemAsync(string name, StorageItem item)
        {
            var confirm = await DisplayAlertAsync(AppResources.Confirm, string.Format(AppResources.ConfirmDeleteMessage, name), AppResources.Yes, AppResources.No);
            if (!confirm) return false;

            List<string> pathsToDelete = new List<string>();
            if (item.IsDirectory)
            {
                var pdfItems = await _storageService.GetPdfFilesAndFolders(item, true);
                pathsToDelete.AddRange(pdfItems.Select(x => x.Id));
            }
            else
            {
                pathsToDelete.Add(item.Id);
            }

            var success = await _storageService.Delete(item);
            if (!success)
            {
                await DisplayAlertAsync(AppResources.Info, AppResources.CreateFolderNotSupportedMessage, AppResources.Ok);
                return false;
            }

            foreach (var path in pathsToDelete)
            {
                var book = await _bookService.GetBookByPathAsync(path);
                if (book != null)
                {
                    await _bookService.DeleteBookAsync(book);
                }
            }

            await RefreshPrimaryAndSecondaryAsync();
            return true;
        }

        private string[] BuildContextMenuOptions(bool isFolder, StorageItem item, bool isCurrent, bool isSecondary = false)
        {
            string[] options;
            if (item == null)
            {
                options = new[] { AppResources.MenuOpen };
            }
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
            else
            {
                if (isSecondaryViewVisible)
                    options = [AppResources.MenuOpen, AppResources.MenuRename, AppResources.MenuDelete, AppResources.MenuMoveNewView];
                else
                    options = [AppResources.MenuOpen, AppResources.MenuRename, AppResources.MenuDelete];
            }

            if (isSecondary)
            {
                var list = new List<string>(options)
                {
                    AppResources.MenuCloseView
                };
                return [.. list];
            }

            return options;
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(SettingsPage));
        }
    }
}
