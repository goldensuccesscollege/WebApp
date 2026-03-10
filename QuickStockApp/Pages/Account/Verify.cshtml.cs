using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QuickStockApp.Services;

namespace QuickStockApp.Pages
{
    public class VerifyModel : PageModel
    {
        private readonly IApiService _api;

        public VerifyModel(IApiService api)
        {
            _api = api;
        }

        public string ModalMessage { get; set; } = "";
        public bool ShowModal { get; set; } = false;
        public bool IsSuccess { get; set; } = false;

        public async Task OnGetAsync([FromQuery] string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                ModalMessage = "Invalid verification link.";
                ShowModal = true;
                IsSuccess = false;
                return;
            }

            var result = await _api.VerifyAsync(token);

            ModalMessage = result.Message;
            IsSuccess = result.Success;
            ShowModal = true;
        }
    }
}
