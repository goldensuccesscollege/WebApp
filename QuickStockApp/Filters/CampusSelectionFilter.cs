using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace QuickStockApp.Filters
{
    public class CampusSelectionFilter : IAsyncPageFilter
    {
        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
        {
            return Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            var user = context.HttpContext.User;
            var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";

            // 1. Skip check for anonymous paths (Login, etc.) and the Campus selection page itself
            if (path.Contains("/account/") || path.Contains("/campuses") || path.Contains("/error"))
            {
                await next();
                return;
            }

            // 2. If authenticated, ensure ActiveCampusId exists
            if (user.Identity?.IsAuthenticated == true)
            {
                var activeCampusId = user.FindFirst("ActiveCampusId")?.Value;
                
                if (string.IsNullOrEmpty(activeCampusId))
                {
                    // For security: if missing, sign out and force login
                    await context.HttpContext.SignOutAsync("MyCookieAuth");
                    context.Result = new RedirectToPageResult("/Account/Login");
                    return;
                }
            }

            await next();
        }
    }
}
