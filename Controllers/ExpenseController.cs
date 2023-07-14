using Expense_BE.Models;
using Expense_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Expense_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase {
    private readonly ExpenseService _expenseService;

    public ExpenseController(ExpenseService expenseService) {
        _expenseService = expenseService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Expense>>> GetUserExpenses() {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null) {
            return Unauthorized(new { message = "JWT contains invalid user ID" });
        }
        var expenses = await _expenseService.GetAllByUserIdAsync(userIdClaim.Value);
        return expenses;
    }

    [Authorize]
    [HttpGet("categories")]
    public async Task<ActionResult<List<KeyValuePair<string, decimal>>>> GetExpensesSumByCategory() {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null)
        {
            return Unauthorized(new { message = "JWT contains invalid user ID" });
        }
        var sumByCategory = await _expenseService.GetExpensesSumByCategoryAsync(userIdClaim.Value);
        return sumByCategory;
    }

    [Authorize]
    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Expense>> GetUserExpense(string id) {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null) {
            return Unauthorized(new { message = "JWT contains invalid user ID" });
        }
        var expense = await _expenseService.GetByUserIdAsync(id, userIdClaim.Value);
        if (expense is null) {
            return NotFound(new { message = "Expense with given ID not located for this user" });
        }
        return expense;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> PostUserExpense(ExpenseItem newExpense) {
        // Thread.Sleep(2000);
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null || !String.Equals(userIdClaim.Value, newExpense.UserId)) {
            return Unauthorized(new { message = "JWT contains invalid user ID" });
        }
        var expense = new Expense {
            UserId = newExpense.UserId,
            Amount = newExpense.Amount,
            Timestamp = newExpense.Timestamp,
            Category = newExpense.Category,
            Description = newExpense.Description
        };

        var createdExpense = await _expenseService.CreateAsync(expense);
        return CreatedAtAction(nameof(GetUserExpense), new { id = createdExpense.Id }, createdExpense);
    }

    [Authorize]
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> UpdateUserExpense(string id, ExpenseItem updatedExpense) {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null || !String.Equals(userIdClaim.Value, updatedExpense.UserId)) {
            return Unauthorized(new { message = "JWT contains invalid user ID" });
        }
        var expense = await _expenseService.GetByUserIdAsync(id, userIdClaim.Value);
        if (expense is null) {
            return NotFound(new { message = "Expense with given ID not located for this user" });
        }
        await _expenseService.UpdateAsync(id, new Expense {
            Id = expense.Id,
            UserId = updatedExpense.UserId,
            Amount = updatedExpense.Amount,
            Timestamp = updatedExpense.Timestamp,
            Category = updatedExpense.Category,
            Description = updatedExpense.Description
        });
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> RemoveUserExpense(string id) {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null) {
            return Unauthorized(new { message = "JWT contains invalid user ID" });
        }
        if (await _expenseService.GetByUserIdAsync(id, userIdClaim.Value) is null) {
            return NotFound(new { message = "Expense with given ID not located for this user" });
        }
        await _expenseService.RemoveByUserIdAsync(id, userIdClaim.Value);
        return NoContent();
    }
}