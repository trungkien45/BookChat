namespace BookChat.StorageService.Inteface
{
    public interface IStorageService
    {
        /// <summary>
        /// if recursive is true, only get pdf file
        /// </summary>
        /// <param name="storageItem"></param>
        /// <param name="recursive"></param>
        /// <returns></returns>
        Task<List<StorageItem>> GetPdfFilesAndFolders(StorageItem storageItem, bool recursive = false);
        Task<bool> Delete(StorageItem storageItem);
        Task<bool> CreateFolder(StorageItem storageItem, string folderName);
        Task<bool> Move(StorageItem source, StorageItem destination);
        Task<bool> Rename(StorageItem storageItem, string newName);
        Task<StorageItem?> GetRootFolder(string rootFolderId);
        Task<StorageItem?> GetParentFolder(StorageItem storageItem, string rootFolderId);
        Task<List<StorageItem>> GetPdfFiles(StorageItem storageItem, bool recursive = false);
    }
}
