using Microsoft.AspNetCore.Authentication;

namespace QuickStockApp.Services
{
    public class UnauthorizedHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UnauthorizedHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                var context = _httpContextAccessor.HttpContext;
                if (context != null && context.User.Identity?.IsAuthenticated == true)
                {
                    // Kill the session
                    await context.SignOutAsync("MyCookieAuth");
                    
                    // Force redirect to login
                    context.Response.Redirect("/Account/Login?Message=" + Uri.EscapeDataString("Your session has expired. Please log in again."));
                }
            }

            return response;
        }
    }
}
