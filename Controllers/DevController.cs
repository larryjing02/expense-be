using Expense_BE.Models;
using Expense_BE.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expense_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase {
    private readonly UserService _userService;
    private readonly ExpenseService _expenseService;

    public DevController(UserService userService, ExpenseService expenseService) {
        _userService = userService;
        _expenseService = expenseService;
    }

    [HttpGet("user")]
    public async Task<List<User>> Get() =>
        await _userService.GetAsync();

    [HttpGet("user/{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id) {
        var user = await _userService.GetAsync(id);

        if (user is null) {
            return NotFound();
        }

        return user;
    }

    [HttpPost("user")]
    public async Task<IActionResult> Post(User newUser) {
        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [HttpPut("user/{id:length(24)}")]
    public async Task<IActionResult> Update(string id, User updatedUser) {
        var user = await _userService.GetAsync(id);

        if (user is null) {
            return NotFound();
        }

        updatedUser.Id = user.Id;

        await _userService.UpdateAsync(id, updatedUser);

        return NoContent();
    }

    [HttpDelete("user/{id:length(24)}")]
    public async Task<IActionResult> Delete(string id) {
        var user = await _userService.GetAsync(id);

        if (user is null) {
            return NotFound();
        }

        await _userService.RemoveAsync(id);

        return NoContent();
    }

    [HttpGet("expense")]
    public async Task<List<Expense>> GetAllExpenses() {
        return await _expenseService.GetAsync();
    }

    [HttpGet("expense/{id:length(24)}")]
    public async Task<ActionResult<Expense>> GetExpense(string id) {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null) {
            return NotFound();
        }

        return expense;
    }

    [HttpGet("expense/categories/{userId:length(24)}")]
    public async Task<ActionResult<List<KeyValuePair<string, decimal>>>> GetExpensesSumByCategory(string userId) {
        return await _expenseService.GetExpensesSumByCategoryAsync(userId);
    }

    [HttpGet("expense/chart/{userId:length(24)}")]
    public async Task<ActionResult<List<KeyValuePair<string, decimal>>>> GetExpensesSumByDateRange(string userId,
        [FromQuery] string timeRange, [FromQuery] DateTime startDate, [FromQuery] DateTime? endDate = null) {
        return await _expenseService.GetExpensesSumByDateRangeAsync(userId, timeRange, startDate, endDate ?? DateTime.UtcNow.Date);
    }

    [HttpPost("expense")]
    public async Task<IActionResult> PostExpense(Expense newExpense) {
        await _expenseService.CreateAsync(newExpense);

        return CreatedAtAction(nameof(Get), new { id = newExpense.Id }, newExpense);
    }

    [HttpPut("expense/{id:length(24)}")]
    public async Task<IActionResult> UpdateExpense(string id, Expense updatedExpense) {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null) {
            return NotFound();
        }

        updatedExpense.Id = expense.Id;

        await _expenseService.UpdateAsync(id, updatedExpense);

        return NoContent();
    }

    [HttpDelete("expense/{id:length(24)}")]
    public async Task<IActionResult> DeleteExpense(string id) {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null) {
            return NotFound();
        }

        await _expenseService.RemoveAsync(id);

        return NoContent();
    }
}