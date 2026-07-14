namespace BookChat.StorageService
{
    public class StorageItem
    {
        /// <summary>
        /// Android: the tree Uri of the selected storage.
        /// in windows, the id is the fullpath of the file or folder
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// in android, the document id is the document id of the file or folder
        /// in windows, the document id is the name of the file or folder
        /// </summary>
        public string DocumentId { get; set; } = string.Empty;
        /// <summary>
        /// in android, the parent document id is the id of the parent folder
        /// in windows, the parent document id is the fullpath of the parent folder
        /// if the item is a root folder, the parent document id is null
        /// </summary>
        public string? ParentDocumentId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
    }
}
