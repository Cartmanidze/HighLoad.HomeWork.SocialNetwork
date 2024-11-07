using HighLoad.HomeWork.SocialNetwork.Interfaces;
using HighLoad.HomeWork.SocialNetwork.Models;
using HighLoad.HomeWork.SocialNetwork.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HighLoad.HomeWork.SocialNetwork.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserService userService, IPasswordHasher passwordHasher) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await userService.UserExistsAsync(request.Email))
        {
            return BadRequest("User already exists");
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Interests = request.Interests,
            City = request.City,
            Email = request.Email,
            PasswordHash = passwordHash
            // Id будет сгенерирован в SaveUserAsync, если он равен Guid.Empty
        };

        await userService.SaveUserAsync(user);

        return Ok("User registered successfully");
    }

    [Authorize]
    [HttpGet("get/{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await userService.GetUserByIdAsync(id);

        if (user == null)
        {
            return NotFound("User not found");
        }

        var userProfile = new
        {
            user.Id,
            user.FirstName,
            user.LastName,
            user.DateOfBirth,
            user.Gender,
            user.Interests,
            user.City,
            user.Email
        };

        return Ok(userProfile);
    }
}