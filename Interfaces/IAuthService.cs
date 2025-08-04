using K8Intel.Dtos;
using K8Intel.Models;
using System.Threading.Tasks;

namespace K8Intel.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(UserRegistrationDto registrationDto);
        Task<string> LoginAsync(UserLoginDto loginDto);
    }
}