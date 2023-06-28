using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Expense_BE.Models;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Expense_BE.Services;

public class UserService {
    private readonly IMongoCollection<User> _users;
    private readonly IConfiguration _configuration;

    public UserService(IConfiguration config, IMongoDatabase database, IConfiguration configuration) {
        _users = database.GetCollection<User>(config.GetSection("MongoDb:UsersCollectionName").Value);
        _configuration = configuration;
    }

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _users.Find(x => x.Username == username).FirstOrDefaultAsync();
        
    public async Task<List<User>> GetAsync() =>
        await _users.Find(_ => true).ToListAsync();

    public async Task<User?> GetAsync(string id) =>
        await _users.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(User newUser) {
        newUser.HashedPassword = BCrypt.Net.BCrypt.HashPassword(newUser.HashedPassword);
        await _users.InsertOneAsync(newUser);
    }

    public async Task UpdateAsync(string id, User updatedUser) =>
        await _users.ReplaceOneAsync(x => x.Id == id, updatedUser);

    public async Task RemoveAsync(string id) =>
        await _users.DeleteOneAsync(x => x.Id == id);

    public bool CheckPassword(User user, string password) {
        return BCrypt.Net.BCrypt.Verify(password, user.HashedPassword);
    }

    public string GenerateJwtToken(User user) {
        string? jwtSecret = _configuration["JwtSettings:Secret"];
        if (string.IsNullOrEmpty(jwtSecret)) {
            throw new Exception("JWT Secret key not found in configuration");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        string? userId = user.Id;
        if (String.IsNullOrEmpty(userId)) {
            throw new Exception("Attempt to generate JWT with invalid user");
        }
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim("userId", userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            _configuration["JwtSettings:Issuer"],
            _configuration["JwtSettings:Audience"],
            claims,
            expires: DateTime.Now.AddMinutes(120),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}