using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Expense_BE.Models;
public class Expense {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string UserId { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime Timestamp { get; set; }

    public string Category { get; set; } = null!;

    public string Description { get; set; } = null!;
}
