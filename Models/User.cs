using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
namespace Expense_BE.Models;

public class User {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string Username { get; set; } = null!;

    [BsonElement("Password")]
    [JsonPropertyName("Password")]
    public string HashedPassword { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

}