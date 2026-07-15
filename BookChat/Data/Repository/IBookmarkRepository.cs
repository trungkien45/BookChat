namespace BookChat.Data.Repository
{
    public interface IBookmarkRepository
    {
        Task<int> DeleteBookmarksByBookIdAsync(int id);
    }
}