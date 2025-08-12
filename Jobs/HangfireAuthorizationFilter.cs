using Hangfire.Dashboard;
using System.Security.Claims;

namespace K8Intel.Jobs
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Check if the user is authenticated
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // Check if the user is an Admin. This is the secure approach.
            return httpContext.User.IsInRole("Admin");
        }
    }
}