using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Asp.Versioning;
using HighLoad.HomeWork.SocialNetwork.Interfaces;
using HighLoad.HomeWork.SocialNetwork.Models;
using HighLoad.HomeWork.SocialNetwork.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace HighLoad.HomeWork.SocialNetwork.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthenticationController(
    IConfiguration configuration,
    IUserService userService,
    IPasswordHasher passwordHasher)
    : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await userService.GetByEmailAsync(request.Email);

        if (user == null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid credentials");
        }

        var token = GenerateJwtToken(user);

        return Ok(new { Token = token });
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await userService.ExistsAsync(request.Email))
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
        };

        await userService.SaveAsync(user);

        return Ok("User registered successfully");
    }
    
    [HttpPost("generate-users")]
    public async Task<IActionResult> GenerateUsers([FromQuery] int count)
    {
        if (count <= 0)
        {
            return BadRequest("Count must be greater than 0");
        }

        const int batchSize = 100_000;
        var faker = new Bogus.Faker();
        var totalBatches = (int)Math.Ceiling((double)count / batchSize);
        var existingEmails = await userService.GetAllEmailsAsync();
        var usedEmails = new HashSet<string>(existingEmails);

        for (var batch = 0; batch < totalBatches; batch++)
        {
            var currentBatchSize = Math.Min(batchSize, count - batch * batchSize);
            var users = new List<User>(currentBatchSize);

            for (var i = 0; i < currentBatchSize; i++)
            {
                string email;
                
                do
                {
                    email = faker.Internet.Email();
                } while (!usedEmails.Add(email));

                var password = faker.Internet.Password(length: 12, memorable: true, regexPattern: "\\w", prefix: "");

                var user = new User
                {
                    FirstName = faker.Name.FirstName(),
                    LastName = faker.Name.LastName(),
                    DateOfBirth = faker.Date.Past(30, DateTime.Now.AddYears(-18)),
                    Gender = faker.PickRandom("Male", "Female"),
                    Interests = faker.Random.Words(3),
                    City = faker.Address.City(),
                    Email = email,
                    PasswordHash = passwordHasher.HashPassword(password)
                };

                users.Add(user);
            }
            
            await userService.BulkInsertAsync(users);
        }

        return Ok($"{count} users generated and inserted successfully");
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim("UserId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}