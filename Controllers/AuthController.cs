using K8Intel.Dtos;
using K8Intel.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;

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
                var token = await _authService.LoginAsync(loginDto);
                return Ok(new LoginResponseDto(token));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}