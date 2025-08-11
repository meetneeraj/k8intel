using Microsoft.AspNetCore.Authentication;

namespace K8Intel.Security
{
    public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
    {
        public const string DefaultScheme = "ApiKey";
        public string HeaderName { get; set; } = "X-Api-Key";
    }
}