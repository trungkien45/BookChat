#if ANDROID
using Android.Net;
using Android.Provider;
#endif
using BookChat.StorageService.Inteface;

namespace BookChat.StorageService.Implement
{
    public class AndroidStogareService : IStogareService
    {
#if !ANDROID
        private const string PlatformNotSupportedMessage = "This method is only supported on Android.";
#endif
        public Task<bool> CreateFolder(StorageItem storageItem, string folderName)
        {
#if ANDROID
            if (!storageItem.IsDirectory)
                return Task.FromResult(false);

            folderName = folderName.Trim();

            if (string.IsNullOrWhiteSpace(folderName))
                return Task.FromResult(false);

            var treeUri = GetTreeUriFromDocumentUri(storageItem);
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
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public Task<bool> Delete(StorageItem storageItem)
        {
#if ANDROID
            var treeUri = GetTreeUriFromDocumentUri(storageItem);
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
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public async Task<List<StorageItem>> GetPdfFilesAndFolders(StorageItem storageItem, bool recursive = false)
        {
#if ANDROID
            return await Task.Run(() =>
            {
                var result = new List<StorageItem>();

                var treeUri = GetTreeUriFromDocumentUri(storageItem);
                var resolver = Android.App.Application.Context.ContentResolver;
                if (resolver == null || treeUri == null)
                    return result;

                var itemsToProcess = new Queue<StorageItem>();
                itemsToProcess.Enqueue(storageItem);

                while (itemsToProcess.Count > 0)
                {
                    var currentItem = itemsToProcess.Dequeue();
                    var parentDocumentId = string.IsNullOrWhiteSpace(currentItem.DocumentId)
                        ? DocumentsContract.GetTreeDocumentId(treeUri)
                        : currentItem.DocumentId;

                    if (parentDocumentId == null)
                        continue;

                    var childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(
                        treeUri,
                        parentDocumentId);

                    if (childrenUri == null)
                        continue;

                    string[] projection =
                    {
                        DocumentsContract.Document.ColumnDocumentId,
                        DocumentsContract.Document.ColumnDisplayName,
                        DocumentsContract.Document.ColumnMimeType,
                        DocumentsContract.Document.ColumnFlags,
                        DocumentsContract.Document.ColumnSize,
                        DocumentsContract.Document.ColumnLastModified
                    };

                    using var cursor = resolver.Query(
                        childrenUri,
                        projection,
                        null,
                        null,
                        null);

                    if (cursor == null)
                        continue;

                    int idIndex = cursor.GetColumnIndex(DocumentsContract.Document.ColumnDocumentId);
                    int nameIndex = cursor.GetColumnIndex(DocumentsContract.Document.ColumnDisplayName);
                    int mimeIndex = cursor.GetColumnIndex(DocumentsContract.Document.ColumnMimeType);

                    var folderItems = new List<StorageItem>();
                    var fileItems = new List<StorageItem>();

                    while (cursor.MoveToNext())
                    {
                        var documentId = cursor.GetString(idIndex);
                        var displayName = cursor.GetString(nameIndex);
                        var mimeType = cursor.GetString(mimeIndex);

                        if (string.IsNullOrWhiteSpace(documentId) || string.IsNullOrWhiteSpace(displayName))
                            continue;

                        var documentUri = DocumentsContract.BuildDocumentUriUsingTree(treeUri, documentId);
                        if (documentUri == null || documentUri.ToString() == null)
                            continue;

                        var item = new StorageItem
                        {
                            Id = documentUri.ToString()!,
                            DocumentId = documentId,
                            ParentDocumentId = parentDocumentId,
                            DisplayName = displayName,
                            IsDirectory = mimeType == DocumentsContract.Document.MimeTypeDir
                        };

                        if (item.IsDirectory)
                        {
                            folderItems.Add(item);
                            if (recursive)
                            {
                                itemsToProcess.Enqueue(item);
                            }
                        }
                        else if (displayName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) || mimeType == "application/pdf")
                        {
                            fileItems.Add(item);
                        }
                    }

                    if (!recursive)
                    {
                        result.AddRange(folderItems);
                        result.AddRange(fileItems);
                    }
                    else
                    {
                        result.AddRange(fileItems);
                    }
                }

                return result.OrderByDescending(x => x.IsDirectory)
                    .ThenBy(x => x.DisplayName, StringComparer.CurrentCultureIgnoreCase)
                    .ToList();
            });
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public async Task<StorageItem?> GetFromId(string id, string rootId)
        {
#if ANDROID
            if (string.IsNullOrWhiteSpace(id))
                return await Task.FromResult<StorageItem?>(null);

            var resolver = Android.App.Application.Context.ContentResolver;
            if (resolver == null)
                return await Task.FromResult<StorageItem?>(null);

            Android.Net.Uri? uri;

            if (id == rootId)
            {
                var treeUri = Android.Net.Uri.Parse(rootId);
                if (treeUri == null)
                    return null;

                uri = DocumentsContract.BuildDocumentUriUsingTree(
                    treeUri,
                    DocumentsContract.GetTreeDocumentId(treeUri));
            }
            else
            {
                uri = Android.Net.Uri.Parse(id);
            }

            if (uri == null)
                return await Task.FromResult<StorageItem?>(null);

            string[] projection =
            {
                DocumentsContract.Document.ColumnDocumentId,
                DocumentsContract.Document.ColumnDisplayName,
                DocumentsContract.Document.ColumnMimeType
            };


            using var cursor = resolver.Query(uri, projection, null, null, null);

            if (cursor == null || !cursor.MoveToFirst())
                return null;

            var documentId = cursor.GetString(
                cursor.GetColumnIndexOrThrow(DocumentsContract.Document.ColumnDocumentId));

            if (documentId == null)
                return null;

            var displayName = cursor.GetString(
                cursor.GetColumnIndexOrThrow(DocumentsContract.Document.ColumnDisplayName));

            if (displayName == null)
                return null;

            var mimeType = cursor.GetString(
                cursor.GetColumnIndexOrThrow(DocumentsContract.Document.ColumnMimeType));

            return await Task.FromResult<StorageItem?>(
                new StorageItem
                {
                    Id = id,
                    DocumentId = documentId,
                    ParentDocumentId = null,
                    DisplayName = displayName,
                    IsDirectory = mimeType == DocumentsContract.Document.MimeTypeDir
                });
#else
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }
        public Task<bool> Move(StorageItem source, StorageItem destination)
        {
#if ANDROID
            if (source == null || destination == null)
                return Task.FromResult(false);

            var resolver = Android.App.Application.Context.ContentResolver;

            var sourceTreeUri = GetTreeUriFromDocumentUri(source);
            var destinationTreeUri = GetTreeUriFromDocumentUri(destination);
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
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }

        public Task<bool> Rename(StorageItem storageItem, string newName)
        {
#if ANDROID
            if (string.IsNullOrWhiteSpace(newName))
                return Task.FromResult(false);

            newName = newName.Trim();

            var treeUri = GetTreeUriFromDocumentUri(storageItem);
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
            throw new PlatformNotSupportedException(PlatformNotSupportedMessage);
#endif
        }
        public Task<StorageItem?> GetParentFolder(string id, string rootFolderId)
        {
            throw new NotImplementedException();
        }

        public Task<List<StorageItem>> GetPdfFiles(StorageItem storageItem, bool recursive = false)
        {
            throw new NotImplementedException();
        }

#if ANDROID

        private static Android.Net.Uri? GetTreeUriFromDocumentUri(StorageItem storageItem)
        {
            if (string.IsNullOrWhiteSpace(storageItem.Id))
                return null;

            var documentUri = Android.Net.Uri.Parse(storageItem.Id);
            if (documentUri == null)
                return null;

            var treeDocumentId = DocumentsContract.GetTreeDocumentId(documentUri);

            return DocumentsContract.BuildTreeDocumentUri(
                documentUri.Authority!,
                treeDocumentId);
        }

#endif
    }
}
