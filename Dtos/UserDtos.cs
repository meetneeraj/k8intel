using System.ComponentModel.DataAnnotations;

namespace K8Intel.Dtos
{
    public record UserRegistrationDto([Required] string Username, [Required] string Password, string Role = "Viewer");
    public record UserLoginDto([Required] string Username, [Required] string Password);
    public record UserDto(int Id, string Username, string Role);
    public record LoginResponseDto(string Token);
}