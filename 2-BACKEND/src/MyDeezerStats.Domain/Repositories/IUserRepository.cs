using MyDeezerStats.Domain.Entities;


namespace MyDeezerStats.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<bool> CreateAsync(User user);
        Task<User?> GetByUsername(string username);
    }
}
