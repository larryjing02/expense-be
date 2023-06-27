using Expense_BE.Models;
using MongoDB.Driver;

namespace Expense_BE.Services;

public class ExpenseService
{
    private readonly IMongoCollection<Expense> _expenses;

    public ExpenseService(IConfiguration config, IMongoDatabase database)
    {
        _expenses = database.GetCollection<Expense>(config.GetSection("MongoDb:ExpensesCollectionName").Value);
    }

    public async Task<List<Expense>> GetAsync() =>
        await _expenses.Find(_ => true).ToListAsync();

    public async Task<Expense?> GetAsync(string id) =>
        await _expenses.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Expense newExpense) =>
        await _expenses.InsertOneAsync(newExpense);

    public async Task UpdateAsync(string id, Expense updatedExpense) =>
        await _expenses.ReplaceOneAsync(x => x.Id == id, updatedExpense);

    public async Task RemoveAsync(string id) =>
        await _expenses.DeleteOneAsync(x => x.Id == id);
}