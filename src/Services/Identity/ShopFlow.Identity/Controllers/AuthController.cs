using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopFlow.Identity.Data;
using ShopFlow.Identity.Models;
using ShopFlow.Identity.Services;
using System.Security.Claims;

namespace ShopFlow.Identity.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { success = false, message = "Email and password are required." });

        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return BadRequest(new { success = false, message = "Email is already registered." });

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { success = false, message = "Registration failed.", errors = result.Errors.Select(e => e.Description) });

        const string defaultRole = "Customer";
        if (!await _roleManager.RoleExistsAsync(defaultRole))
            await _roleManager.CreateAsync(new IdentityRole<Guid>(defaultRole));

        await _userManager.AddToRoleAsync(user, defaultRole);

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles, out var expires);

        return Ok(new
        {
            success = true,
            message = "Registered successfully.",
            data = new AuthResponse(token, expires, new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToList()))
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized(new { success = false, message = "Invalid email or password." });

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized(new { success = false, message = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.CreateToken(user, roles, out var expires);

        return Ok(new
        {
            success = true,
            message = "Login successful.",
            data = new AuthResponse(token, expires, new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToList()))
        });
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new
        {
            success = true,
            data = new UserDto(user.Id, user.Email!, user.FirstName, user.LastName, roles.ToList())
        });
    }
}
