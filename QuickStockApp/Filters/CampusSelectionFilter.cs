using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

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

            // 1. Skip check for Login/Account pages, Campus selection page, and error pages
            if (path.Contains("/account/") || path.Contains("/campuses") || path.Contains("/error"))
            {
                await next();
                return;
            }

            // 2. If authenticated, ensure ActiveCampusId exists (skip for Admin role)
            if (user.Identity?.IsAuthenticated == true)
            {
                var role = user.FindFirst(ClaimTypes.Role)?.Value ?? "";
                var isAdmin = role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

                if (!isAdmin)
                {
                    var activeCampusId = user.FindFirst("ActiveCampusId")?.Value;

                    if (string.IsNullOrEmpty(activeCampusId))
                    {
                        // Missing campus selection — redirect to campus picker (do NOT sign out)
                        context.Result = new RedirectToPageResult("/Campus/Campuses");
                        return;
                    }
                }
            }

            await next();
        }
    }
}
