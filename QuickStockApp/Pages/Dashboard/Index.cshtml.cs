using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace QuickStockApp.Pages.Dashboard
{
    [Authorize]
    public class IndexModel : PageModel
    {
        public int TotalProducts { get; set; } = 1250;
        public int LowStockCount { get; set; } = 12;
        public int TotalValue { get; set; } = 45200;
        public List<RecentActivity> Activities { get; set; } = new();

        public void OnGet()
        {
            Activities = new List<RecentActivity>
            {
                new() { Action = "Restock", Item = "Wireless Mouse", Time = "10 mins ago", Status = "Success" },
                new() { Action = "Sale", Item = "Mechanical Keyboard", Time = "45 mins ago", Status = "Warning" },
                new() { Action = "Delete", Item = "USB-C Cable", Time = "2 hours ago", Status = "Danger" }
            };
        }
    }

    public class RecentActivity
    {
        public string Action { get; set; } = "";
        public string Item { get; set; } = "";
        public string Time { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
