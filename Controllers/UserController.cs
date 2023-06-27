using Expense_BE.Models;
using Expense_BE.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expense_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    // Production endpoints
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterItem newUser)
    {
        // Verify user does not already exist in table
        if (await _userService.GetByUsernameAsync(newUser.Username) != null)
            return Conflict(new { message = "User with given username already exists" });

        var userToCreate = new User {
            Username = newUser.Username,
            HashedPassword = newUser.Password,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName
        };
        await _userService.CreateAsync(userToCreate);
        return NoContent();        
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginItem login)
    {
        // Verify user exists in table
        var user = await _userService.GetByUsernameAsync(login.Username);
        if (user == null)
            return NotFound(new { message = "Invalid username. Please try again." });
        // Validate password
        if (!_userService.CheckPassword(user, login.Password)) {
            return Unauthorized(new { message = "Incorrect password. Please try again."});
        }
        // Login is valid - generate JWT and return as cookie
        return Ok();        
    }

    // The following endpoints are used for development only
    [HttpGet]
    public async Task<List<User>> Get() =>
        await _userService.GetAsync();

    [HttpGet("{id:length(24)}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost]
    public async Task<IActionResult> Post(User newUser)
    {
        await _userService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> Update(string id, User updatedUser)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        updatedUser.Id = user.Id;

        await _userService.UpdateAsync(id, updatedUser);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userService.GetAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        await _userService.RemoveAsync(id);

        return NoContent();
    }
}