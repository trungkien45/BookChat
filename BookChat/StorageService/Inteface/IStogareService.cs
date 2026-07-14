namespace BookChat.StorageService.Inteface
{
    public interface IStogareService
    {
        public Task<List<StorageItem>> GetPdfFilesAndFolders(StorageItem storageItem, bool recursive = false);
        public Task<bool> Delete(StorageItem storageItem);
        public Task<bool> CreateFolder(StorageItem storageItem, string folderName);
        public Task<bool> Move(StorageItem source, StorageItem destination);
        public Task<bool> Rename(StorageItem storageItem, string newName);
        public StorageItem? GetFromId(string id, string rootId);
    }
}
