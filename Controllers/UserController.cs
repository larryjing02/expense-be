using Expense_BE.Models;
using Expense_BE.Services;
using Microsoft.AspNetCore.Mvc;

namespace Expense_BE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase {
    private readonly UserService _userService;

    public UserController(UserService userService) {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterItem newUser) {
        // Verify user does not already exist in table
        if (await _userService.GetByUsernameAsync(newUser.Username) != null) {
            return Conflict(new { message = "User with given username already exists" });
        }
        var userToCreate = new User {
            Username = newUser.Username,
            HashedPassword = newUser.Password,
            FirstName = newUser.FirstName,
            LastName = newUser.LastName
        };
        await _userService.CreateAsync(userToCreate);
        // Registration is valid - generate JWT and return as cookie
        var token = _userService.GenerateJwtToken(userToCreate);
        return Ok(new { message = "Registration successful",
                        token = token,
                        firstName = userToCreate.FirstName,
                        userId = userToCreate.Id });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginItem login) {
        // Verify user exists in table
        var user = await _userService.GetByUsernameAsync(login.Username);
        if (user == null) {
            return NotFound(new { message = "Username not found. Please try again. " });
        } else if (!_userService.CheckPassword(user, login.Password)) {
            return Unauthorized(new { message = "Incorrect username or password. Please try again." });
        }
        // Login is valid - generate JWT and return as cookie
        var token = _userService.GenerateJwtToken(user);
        Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });
        return Ok(new { message = "Login successful",
                        token = token,
                        firstName = user.FirstName,
                        userId = user.Id });
    }
}