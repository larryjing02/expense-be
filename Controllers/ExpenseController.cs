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

    // The following endpoints are used for development only
    [HttpGet("dev")]
    public async Task<List<Expense>> GetAllExpenses() {
        return await _expenseService.GetAsync();
    }

    [HttpGet("dev/{id:length(24)}")]
    public async Task<ActionResult<Expense>> Get(string id) {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null) {
            return NotFound();
        }

        return expense;
    }

    [HttpPost("dev")]
    public async Task<IActionResult> Post(Expense newExpense) {
        await _expenseService.CreateAsync(newExpense);

        return CreatedAtAction(nameof(Get), new { id = newExpense.Id }, newExpense);
    }

    [HttpPut("dev/{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Expense updatedExpense) {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null) {
            return NotFound();
        }

        updatedExpense.Id = expense.Id;

        await _expenseService.UpdateAsync(id, updatedExpense);

        return NoContent();
    }

    [HttpDelete("dev/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id) {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null) {
            return NotFound();
        }

        await _expenseService.RemoveAsync(id);

        return NoContent();
    }
}