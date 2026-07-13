#if ANDROID
using Android.Provider;
#endif
using BookChat.StorageService.Inteface;

namespace BookChat.StorageService.Implement
{
    public class AndroidStogareService : IStogareService
    {
        public Task<bool> CreateFolder(StorageItem storageItem, string folderName)
        {
#if ANDROID
            if (!storageItem.IsDirectory)
                return Task.FromResult(false);

            folderName = folderName.Trim();

            if (string.IsNullOrWhiteSpace(folderName))
                return Task.FromResult(false);

            var treeUri = Android.Net.Uri.Parse(storageItem.Id);
            var resolver = Android.App.Application.Context.ContentResolver;

            // Check write permission
            bool hasWrite = resolver?.PersistedUriPermissions?
                .Any(p => p.Uri?.Equals(treeUri) == true && p.IsWritePermission) == true;

            if (!hasWrite)
                return Task.FromResult(false);

            var targetDocId = storageItem.DocumentId;
            var parentDocUri = string.IsNullOrEmpty(targetDocId)
                ? DocumentsContract.BuildDocumentUriUsingTree(treeUri, DocumentsContract.GetTreeDocumentId(treeUri))
                : DocumentsContract.BuildDocumentUriUsingTree(treeUri, targetDocId);


            if (resolver == null || parentDocUri == null)
            {
                return Task.FromResult(false);
            }
            var created = DocumentsContract.CreateDocument(
                resolver,
                parentDocUri,
                DocumentsContract.Document.MimeTypeDir,
                folderName);

            return Task.FromResult(created != null);
#else
            throw new PlatformNotSupportedException("This method is only supported on Android.");
#endif
        }

        public Task<bool> Delete(StorageItem storageItem)
        {
#if ANDROID
            var treeUri = Android.Net.Uri.Parse(storageItem.Id);
            var resolver = Android.App.Application.Context.ContentResolver;

            // Check write permission
            bool hasWrite = resolver?.PersistedUriPermissions?
                .Any(p => p.Uri?.Equals(treeUri) == true && p.IsWritePermission) == true;

            if (!hasWrite)
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(storageItem.DocumentId))
                return Task.FromResult(false);

            var documentUri = DocumentsContract.BuildDocumentUriUsingTree(
                treeUri,
                storageItem.DocumentId);
            if (documentUri == null)
                return Task.FromResult(false);
            bool deleted = DocumentsContract.DeleteDocument(
                resolver!,
                documentUri);

            return Task.FromResult(deleted);
#else
            throw new PlatformNotSupportedException("This method is only supported on Android.");
#endif
        }

        public Task<List<StorageItem>> GetFilesAndFolders(StorageItem storageItem)
        {
#if ANDROID
            var result = new List<StorageItem>();

            var treeUri = Android.Net.Uri.Parse(storageItem.Id);
            var resolver = Android.App.Application.Context.ContentResolver;
            if (resolver == null || treeUri == null)
                return Task.FromResult(result);
            var parentDocumentId = string.IsNullOrWhiteSpace(storageItem.DocumentId)
                ? DocumentsContract.GetTreeDocumentId(treeUri)
                : storageItem.DocumentId;
            if (parentDocumentId == null)
                return Task.FromResult(result);

            var childrenUri =
                DocumentsContract.BuildChildDocumentsUriUsingTree(
                    treeUri,
                    parentDocumentId);

            string[] projection =
            {
                        DocumentsContract.Document.ColumnDocumentId,
                        DocumentsContract.Document.ColumnDisplayName,
                        DocumentsContract.Document.ColumnMimeType,
                        DocumentsContract.Document.ColumnFlags,
                        DocumentsContract.Document.ColumnSize,
                        DocumentsContract.Document.ColumnLastModified
                    };
            if (childrenUri == null)
                return Task.FromResult(result);

            using var cursor = resolver.Query(
                childrenUri,
                projection,
                null,
                null,
                null);

            if (cursor == null)
                return Task.FromResult(result);

            int idIndex = cursor.GetColumnIndex(
                DocumentsContract.Document.ColumnDocumentId);

            int nameIndex = cursor.GetColumnIndex(
                DocumentsContract.Document.ColumnDisplayName);

            int mimeIndex = cursor.GetColumnIndex(
                DocumentsContract.Document.ColumnMimeType);

            var folderItems = new List<StorageItem>();
            var fileItems = new List<StorageItem>();

            while (cursor.MoveToNext())
            {
                var documentId = cursor.GetString(idIndex);
                var displayName = cursor.GetString(nameIndex);
                var mimeType = cursor.GetString(mimeIndex);

                if (string.IsNullOrWhiteSpace(documentId) ||
                    string.IsNullOrWhiteSpace(displayName))
                    continue;

                var item = new StorageItem
                {
                    Id = storageItem.Id,
                    DocumentId = documentId,
                    ParentDocumentId = parentDocumentId,
                    DisplayName = displayName,
                    IsDirectory = mimeType == DocumentsContract.Document.MimeTypeDir
                };

                if (item.IsDirectory)
                    folderItems.Add(item);
                else
                    fileItems.Add(item);
            }

            result.AddRange(folderItems);
            result.AddRange(fileItems);


            return Task.FromResult(result);
#else
            throw new PlatformNotSupportedException("This method is only supported on Android.");
#endif
        }
        public Task<bool> Move(StorageItem source, StorageItem destination)
        {
#if ANDROID
            if (source == null || destination == null)
                return Task.FromResult(false);

            var resolver = Android.App.Application.Context.ContentResolver;

            var sourceTreeUri = Android.Net.Uri.Parse(source.Id);
            var destinationTreeUri = Android.Net.Uri.Parse(destination.Id);
            if (resolver == null || sourceTreeUri == null || destinationTreeUri == null)
                return Task.FromResult(false);
            // MoveDocument chỉ hỗ trợ trong cùng một DocumentsProvider
            if (!string.Equals(sourceTreeUri.Authority, destinationTreeUri.Authority, StringComparison.Ordinal))
                return Task.FromResult(false);

            // Check write permission
            bool hasWrite = resolver?.PersistedUriPermissions?.Any(p =>
                (p.Uri?.Equals(sourceTreeUri) == true ||
                 p.Uri?.Equals(destinationTreeUri) == true) &&
                p.IsWritePermission) == true;

            if (!hasWrite)
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(source.DocumentId) ||
                string.IsNullOrWhiteSpace(source.ParentDocumentId) ||
                string.IsNullOrWhiteSpace(destination.DocumentId))
                return Task.FromResult(false);

            var sourceUri = DocumentsContract.BuildDocumentUriUsingTree(
                sourceTreeUri,
                source.DocumentId);

            var sourceParentUri = DocumentsContract.BuildDocumentUriUsingTree(
                sourceTreeUri,
                source.ParentDocumentId);

            var destinationParentUri = DocumentsContract.BuildDocumentUriUsingTree(
                destinationTreeUri,
                destination.DocumentId);
            if (sourceUri == null || sourceParentUri == null || destinationParentUri == null)
                return Task.FromResult(false);
            var moved = DocumentsContract.MoveDocument(
                resolver!,
                sourceUri,
                sourceParentUri,
                destinationParentUri);

            return Task.FromResult(moved != null);
#else
            throw new PlatformNotSupportedException("This method is only supported on Android.");
#endif
        }

        public Task<bool> Rename(StorageItem storageItem, string newName)
        {
#if ANDROID
            if (string.IsNullOrWhiteSpace(newName))
                return Task.FromResult(false);

            newName = newName.Trim();

            var treeUri = Android.Net.Uri.Parse(storageItem.Id);
            var resolver = Android.App.Application.Context.ContentResolver;

            // Ensure we have write permission on the tree
            bool hasWrite = resolver?.PersistedUriPermissions?.Any(p =>
                p.Uri?.Equals(treeUri) == true &&
                p.IsWritePermission) == true;

            if (!hasWrite)
                return Task.FromResult(false);

            if (string.IsNullOrWhiteSpace(storageItem.DocumentId))
                return Task.FromResult(false);

            var documentUri = DocumentsContract.BuildDocumentUriUsingTree(
                treeUri,
                storageItem.DocumentId);
            if (documentUri == null)
                return Task.FromResult(false);
            var renamed = DocumentsContract.RenameDocument(
                resolver!,
                documentUri,
                newName);

            return Task.FromResult(renamed != null);
#else
            throw new PlatformNotSupportedException("This method is only supported on Android.");
#endif
        }
    }
}
