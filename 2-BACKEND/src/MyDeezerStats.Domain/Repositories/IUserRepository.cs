using MyDeezerStats.Domain.Entities;


namespace MyDeezerStats.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByUsername(string username);
    }
}
