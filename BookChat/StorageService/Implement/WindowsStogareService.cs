using BookChat.StorageService.Inteface;

namespace BookChat.StorageService.Implement
{
    public class WindowsStogareService : IStogareService
    {
#if !WINDOWS
        private const string PlatformNotSupportedMessage = "This method is only supported on Windows.";
#endif
        public async Task<bool> CreateFolder(StorageItem storageItem, string folderName)
        {
#if WINDOWS
            if (!storageItem.IsDirectory)
            {
                return await Task.FromResult(false);
            }
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return await Task.FromResult(false);
            }

            if (Directory.Exists(Path.Combine(storageItem.Id, folderName)))
            {
                return await Task.FromResult(false);
            }

            Directory.CreateDirectory(Path.Combine(storageItem.Id, folderName));
            return await Task.FromResult(true);
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public async Task<bool> Delete(StorageItem storageItem)
        {
#if WINDOWS
            if (storageItem.IsDirectory)
            {
                Directory.Delete(storageItem.Id, true);
            }
            else
            {
                File.Delete(storageItem.Id);
            }
            return await Task.FromResult(true);
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public async Task<List<StorageItem>> GetPdfFilesAndFolders(StorageItem storageItem, bool recursive = false)
        {
#if WINDOWS
            if (!storageItem.IsDirectory)
            {
                return await Task.FromResult(new List<StorageItem>());
            }
            var fullPath = storageItem.Id; // Assuming Id is the full path of the folder
            var directoryInfo = new DirectoryInfo(fullPath);
            var items = new List<StorageItem>();

            if (recursive)
            {
                var filePdfItems = directoryInfo.GetFiles("*.pdf", SearchOption.AllDirectories).Select(p => new StorageItem
                {
                    Id = p.FullName,
                    IsDirectory = false,
                    DocumentId = p.Name,
                    DisplayName = p.Name,
                    ParentDocumentId = storageItem.DocumentId
                });
                items.AddRange(filePdfItems);

            }
            else
            {
                var folderItems = directoryInfo.GetDirectories().Select(p => new StorageItem
                {
                    Id = p.FullName,
                    IsDirectory = true,
                    DocumentId = p.Name,
                    DisplayName = p.Name,
                    ParentDocumentId = storageItem.DocumentId
                });
                var filePdfItems = directoryInfo.GetFiles("*.pdf").Select(p => new StorageItem
                {
                    Id = p.FullName,
                    IsDirectory = false,
                    DocumentId = p.Name,
                    DisplayName = p.Name,
                    ParentDocumentId = storageItem.DocumentId
                });
                items.AddRange(folderItems);
                items.AddRange(filePdfItems);
            }

            return await Task.FromResult(items.OrderByDescending(x => x.IsDirectory)
                .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public async Task<StorageItem?> GetRootFolder(string rootFolderId)
        {
#if WINDOWS
            var rootPath = rootFolderId; // rootId là full path của thư mục gốc

            if (!Directory.Exists(rootPath))
                throw new ArgumentException("The provided rootId does not correspond to an existing directory.");

            var fullPath = Path.GetFullPath(rootPath);
            var displayName = Path.GetFileName(fullPath);

            // Với ổ đĩa gốc kiểu "C:\", Path.GetFileName trả về chuỗi rỗng —
            // fallback về chính đường dẫn để DisplayName không bị trống.
            if (string.IsNullOrEmpty(displayName))
                displayName = fullPath;

            return await Task.FromResult(new StorageItem
            {
                Id = fullPath,
                IsDirectory = true,
                DocumentId = displayName,
                DisplayName = displayName,
                ParentDocumentId = null
            });
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }


        public Task<bool> Move(StorageItem source, StorageItem destination)
        {
#if WINDOWS
            if (destination.IsDirectory)
            {
                var sourcePath = source.Id;
                var destinationPath = Path.Combine(destination.Id, source.DocumentId);
                if (source.IsDirectory)
                {
                    Directory.Move(sourcePath, destinationPath);
                }
                else
                {
                    File.Move(sourcePath, destinationPath);
                }
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public Task<bool> Rename(StorageItem storageItem, string newName)
        {
#if WINDOWS
            if (!storageItem.IsDirectory)
            {
                var directory = Path.GetDirectoryName(storageItem.Id);

                if (directory is null)
                    return Task.FromResult(false);
                File.Move(storageItem.Id, Path.Combine(directory, newName));
            }
            else
            {
                var directory = Path.GetDirectoryName(storageItem.Id);
                if (directory is null)
                    return Task.FromResult(false);
                Directory.Move(storageItem.Id, Path.Combine(directory, newName));
            }
            return Task.FromResult(true);
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public async Task<StorageItem?> GetParentFolder(StorageItem storageItem, string rootFolderId)
        {
#if WINDOWS
            var fullPath = storageItem.Id; // Assuming storageItem.Id is the full path of the item
            var rootPath = rootFolderId; // Assuming rootId is the full path of the root folder
            if (fullPath == rootPath)
            {
                return await Task.FromResult<StorageItem?>(null);
            }
            // 1. Convert both paths to absolute, fully qualified paths
            string absoluteRoot = Path.GetFullPath(rootPath);
            string absoluteTarget = Path.GetFullPath(fullPath);

            // 2. Get the relative path from the root to the target
            string relativePath = Path.GetRelativePath(absoluteRoot, absoluteTarget);

            // 3. Ensure it doesn't escape the root ("..") and isn't a completely different root drive
            bool isOutsideRoot = relativePath.StartsWith("..") || Path.IsPathRooted(relativePath);
            if (isOutsideRoot)
            {
                return await Task.FromResult<StorageItem?>(null);
            }
            var parentPath = Path.GetDirectoryName(fullPath);
            return await Task.FromResult<StorageItem?>(
                new StorageItem()
                {
                    DisplayName = Path.GetFileName(parentPath)!,
                    DocumentId = Path.GetFileName(parentPath)!,
                    Id = parentPath!,
                    IsDirectory = true,
                    ParentDocumentId = parentPath == rootFolderId ? null : Path.GetDirectoryName(parentPath)
                });
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public async Task<List<StorageItem>> GetPdfFiles(StorageItem storageItem, bool recursive = false)
        {
#if WINDOWS
            if (!storageItem.IsDirectory)
            {
                return await Task.FromResult(new List<StorageItem>());
            }
            var fullPath = storageItem.Id; // Assuming Id is the full path of the folder
            var directoryInfo = new DirectoryInfo(fullPath);
            var items = new List<StorageItem>();

            if (recursive)
            {
                var filePdfItems = directoryInfo.GetFiles("*.pdf", SearchOption.AllDirectories).Select(p => new StorageItem
                {
                    Id = p.FullName,
                    IsDirectory = false,
                    DocumentId = p.Name,
                    DisplayName = p.Name,
                    ParentDocumentId = storageItem.DocumentId
                });
                items.AddRange(filePdfItems);
            }
            else
            {
                var filePdfItems = directoryInfo.GetFiles("*.pdf").Select(p => new StorageItem
                {
                    Id = p.FullName,
                    IsDirectory = false,
                    DocumentId = p.Name,
                    DisplayName = p.Name,
                    ParentDocumentId = storageItem.DocumentId
                });
                items.AddRange(filePdfItems);
            }

            return await Task.FromResult(items.OrderByDescending(x => x.IsDirectory)
                .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }
    }
}
