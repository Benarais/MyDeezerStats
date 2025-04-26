using MongoDB.Driver;
using MyDeezerStats.Domain.Entities;
using MyDeezerStats.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDeezerStats.Infrastructure.Mongo
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase database)
        {
            _users = database.GetCollection<User>("users");
        }

        public async Task<User?> GetByUsername(string username)
        {
            return await _users.Find(user => user.Email == username).FirstOrDefaultAsync();
        }

    }
}
