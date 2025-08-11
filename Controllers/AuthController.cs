using K8Intel.Dtos;
using K8Intel.Interfaces;
using K8Intel.Models;
using Microsoft.AspNetCore.Authentication; // --->>> ADD THIS
using Microsoft.AspNetCore.Authentication.Cookies; // --->>> ADD THIS
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic; // --->>> ADD THIS
using System.Security.Claims; // --->>> ADD THIS
using System;
using System.Threading.Tasks;

namespace K8Intel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
        {
            try
            {
                var user = await _authService.RegisterAsync(registrationDto);
                var userDto = new UserDto(user.Id, user.Username ?? string.Empty, user.Role ?? string.Empty);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, userDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginDto loginDto)
        {
            try
            {
                // We change the IAuthService to return a full User object on successful login
                var (user, jwtToken) = await _authService.LoginAsync(loginDto);
                if (user == null)
                {
                    return Unauthorized("Invalid credentials.");
                }

                // --- START: COOKIE SIGN-IN LOGIC ---
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));
                // --- END: COOKIE SIGN-IN LOGIC ---
                
                // Return the JWT token in the body as before
                return Ok(new LoginResponseDto(jwtToken));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}