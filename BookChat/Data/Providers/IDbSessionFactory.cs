namespace BookChat.Data.Providers
{
    public interface IDbSessionFactory
    {
        Task<IDbSession> CreateAsync();
    }
}
