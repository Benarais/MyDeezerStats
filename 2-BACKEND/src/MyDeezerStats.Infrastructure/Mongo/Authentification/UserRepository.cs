using MongoDB.Driver;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Infrastructure.Mongo.Authentification
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
        }

        public async Task<bool> CreateAsync(User user)
        {
            await _users.InsertOneAsync(user);
            return true;
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _users.Find(user => user.Email == username).FirstOrDefaultAsync();
        }

    }
}
