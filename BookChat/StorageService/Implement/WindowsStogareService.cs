using BookChat.StorageService.Inteface;

namespace BookChat.StorageService.Implement
{
    public class WindowsStogareService : IStogareService
    {
        public Task<bool> CreateFolder(StorageItem storageItem, string folderName)
        {
#if WINDOWS
            if (!storageItem.IsDirectory)
            {
                return Task.FromResult(false);
            }
            if (string.IsNullOrWhiteSpace(folderName))
            {
                return Task.FromResult(false);
            }

            if (Directory.Exists(Path.Combine(storageItem.Id, folderName)))
            {
                return Task.FromResult(false);
            }

            Directory.CreateDirectory(Path.Combine(storageItem.Id, folderName));
            return Task.FromResult(true);
#else
            throw new PlatformNotSupportedException("This method is only supported on Windows.");
#endif
        }

        public Task<bool> Delete(StorageItem storageItem)
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
            return Task.FromResult(true);
#else
            throw new PlatformNotSupportedException("This method is only supported on Windows.");
#endif
        }

        public Task<List<StorageItem>> GetFilesAndFolders(StorageItem storageItem)
        {
#if WINDOWS
            if (!storageItem.IsDirectory)
            {
                return Task.FromResult(new List<StorageItem>());
            }
            var fullPath = storageItem.Id; // Assuming Id is the full path of the folder
            DirectoryInfo directoryInfo = new DirectoryInfo(fullPath);
            var items = new List<StorageItem>();
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

            return Task.FromResult(items.OrderByDescending(x => x.IsDirectory)
                .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                .ToList());
#else
            throw new PlatformNotSupportedException("This method is only supported on Windows.");
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
            throw new PlatformNotSupportedException("This method is only supported on Windows.");
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
            throw new PlatformNotSupportedException("This method is only supported on Windows.");
#endif
        }
    }
}
