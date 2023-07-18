using Expense_BE.Models;
using MongoDB.Driver;

namespace Expense_BE.Services;

public class ExpenseService {
    private readonly IMongoCollection<Expense> _expenses;

    public ExpenseService(IConfiguration config, IMongoDatabase database) {
        _expenses = database.GetCollection<Expense>(config.GetSection("MongoDb:ExpensesCollectionName").Value);
    }

    public async Task<List<Expense>> GetAsync() =>
        await _expenses.Find(_ => true).SortByDescending(e => e.Timestamp).ToListAsync();

    public async Task<Expense?> GetAsync(string id) =>
        await _expenses.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task<List<KeyValuePair<string, decimal>>> GetExpensesSumByCategoryAsync(string userId) {
        var expenses = await _expenses.Find(x => x.UserId == userId).ToListAsync();
        return expenses.GroupBy(e => e.Category)
                    .Select(g => new KeyValuePair<string, decimal>(g.Key, g.Sum(e => e.Amount)))
                    .OrderByDescending(e => e.Value)
                    .ToList();
    }

    public async Task<List<KeyValuePair<string, decimal>>> GetExpensesSumByDateRangeAsync(string userId, string timeRange, DateTime startDate, DateTime endDate) {
        endDate = endDate.Date.Add(new TimeSpan(23, 59, 59));
        var expenses = await _expenses.Find(x => (x.UserId == userId) &&
                                                (x.Timestamp >= startDate) &&
                                                (x.Timestamp <= endDate)).ToListAsync();
        
        var groupedExpenses = expenses
            .GroupBy(x => GetIntervalKey(x.Timestamp, startDate, timeRange))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var allKeys = GetAllIntervalKeys(startDate, endDate, timeRange);

        var result = allKeys.Select(k => new KeyValuePair<string, decimal>(k, groupedExpenses.ContainsKey(k) ? groupedExpenses[k] : 0)).ToList();

        return result;
    }

    private List<string> GetAllIntervalKeys(DateTime startDate, DateTime endDate, string timeRange) {
        var keys = new List<string>();
        var date = startDate;

        while (date <= endDate) {
            keys.Add(GetIntervalKey(date, startDate, timeRange));

            switch (timeRange.ToLower()) {
                case "day":
                    date = date.AddDays(1);
                    break;
                case "week":
                    date = date.AddDays(7);
                    break;
                case "month":
                    date = date.AddMonths(1);
                    break;
                case "year":
                    date = date.AddYears(1);
                    break;
                default:
                    throw new ArgumentException("Invalid time range");
            }
        }

        return keys;
    }

    private string GetIntervalKey(DateTime timestamp, DateTime startDate, string timeRange) {
        var offset = TimeZoneInfo.Local.GetUtcOffset(timestamp).TotalHours;
        timestamp = timestamp.AddHours(offset);
        switch (timeRange.ToLower()) {
            case "day":
                return timestamp.Date.ToString("yyyy-MM-dd");
            case "week":
                int weekNumber = ((timestamp - startDate).Days / 7);
                return startDate.AddDays(weekNumber * 7).ToString("yyyy-MM-dd");
            case "month":
                return timestamp.ToString("yyyy-MM");
            case "year":
                return timestamp.Year.ToString();
            default:
                throw new ArgumentException("Invalid time range");
        }
    }

    public async Task<Expense?> GetByUserIdAsync(string id, string userId) =>
        await _expenses.Find(x => (x.Id == id) && (x.UserId == userId)).FirstOrDefaultAsync();

    public async Task<List<Expense>> GetAllByUserIdAsync(string userId) =>
        await _expenses.Find(x => x.UserId == userId).SortByDescending(e => e.Timestamp).ToListAsync();

    public async Task<Expense> CreateAsync(Expense newExpense) {
        await _expenses.InsertOneAsync(newExpense);
        return newExpense;
    }

    public async Task UpdateAsync(string id, Expense updatedExpense) =>
        await _expenses.ReplaceOneAsync(x => x.Id == id, updatedExpense);

    public async Task RemoveAsync(string id) =>
        await _expenses.DeleteOneAsync(x => x.Id == id);
    
    public async Task RemoveByUserIdAsync(string id, string userId) =>
        await _expenses.DeleteOneAsync(x => (x.Id == id) && (x.UserId == userId));

}