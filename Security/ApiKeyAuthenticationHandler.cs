using K8Intel.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace K8Intel.Security
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
    {
        private readonly AppDbContext _context;

        public ApiKeyAuthenticationHandler(
            IOptionsMonitor<ApiKeyAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            AppDbContext context) : base(options, logger, encoder)
        {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Check if the API Key header exists in the request.
            if (!Request.Headers.TryGetValue(Options.HeaderName, out var apiKeyHeaderValues))
            {
                return AuthenticateResult.NoResult(); // No header, pass to the next auth scheme (JWT).
            }

            var providedApiKey = apiKeyHeaderValues.ToString();
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return AuthenticateResult.Fail("API Key is missing or empty.");
            }

            // 2. Find a cluster with a matching API Key in the database.
            var cluster = await _context.Clusters
                .FirstOrDefaultAsync(c => c.AgentApiKey == providedApiKey);

            if (cluster == null)
            {
                Logger.LogWarning("Invalid API Key provided.");
                return AuthenticateResult.Fail("Invalid API Key.");
            }

            // 3. If the key is valid, create an identity for the agent.
            Logger.LogInformation($"Successfully authenticated agent for Cluster: {cluster.Name} (ID: {cluster.Id})");

            var claims = new List<Claim>
            {
                // We create a "Claim" to identify the authenticated client.
                // This is useful for logging or advanced authorization.
                new Claim(ClaimTypes.Name, $"ClusterAgent-{cluster.Id}"),
                new Claim("ClusterId", cluster.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}