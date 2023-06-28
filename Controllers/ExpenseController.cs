using Expense_BE.Models;
using Expense_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Expense_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExpenseController : ControllerBase
{
    private readonly ExpenseService _expenseService;

    public ExpenseController(ExpenseService expenseService)
    {
        _expenseService = expenseService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<Expense>>> GetUserExpenses() {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null) {
            return Unauthorized(new { message = "JWT contains invalid user ID"});
        }
        var expenses = await _expenseService.GetByUserIdAsync(userIdClaim.Value);
        return expenses;
    }

    // Used for development only
    [HttpGet("all")]
    public async Task<List<Expense>> GetAllExpenses() {
        return await _expenseService.GetAsync();
    }

    

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<Expense>> Get(string id)
    {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null)
        {
            return NotFound();
        }

        return expense;
    }

    [HttpPost]
    public async Task<IActionResult> Post(Expense newExpense)
    {
        await _expenseService.CreateAsync(newExpense);

        return CreatedAtAction(nameof(Get), new { id = newExpense.Id }, newExpense);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, Expense updatedExpense)
    {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null)
        {
            return NotFound();
        }

        updatedExpense.Id = expense.Id;

        await _expenseService.UpdateAsync(id, updatedExpense);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var expense = await _expenseService.GetAsync(id);

        if (expense is null)
        {
            return NotFound();
        }

        await _expenseService.RemoveAsync(id);

        return NoContent();
    }
}