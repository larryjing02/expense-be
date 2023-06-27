using Expense_BE.Models;
using MongoDB.Driver;

namespace Expense_BE.Services;

public class UserService
{
    private readonly IMongoCollection<User> _users;

    public UserService(IConfiguration config, IMongoDatabase database)
    {
        _users = database.GetCollection<User>(config.GetSection("MongoDb:UsersCollectionName").Value);
    }

    public async Task<List<User>> GetAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _users.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) =>
        await _users.InsertOneAsync(newUser);

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _users.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _users.DeleteOneAsync(x => x.Id == id);
}