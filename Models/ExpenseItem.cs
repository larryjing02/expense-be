// Represents a subset of Expense model used for user input
public class ExpenseItem {
    public string UserId { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime Timestamp { get; set; }

    public string Category { get; set; } = null!;

    public string Description { get; set; } = null!;
}