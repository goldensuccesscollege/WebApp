using QuickStockApp.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace QuickStockApp.Services
{
    public interface ILibraryService
    {
        Task<List<LibraryBookDto>> GetLibraryBooksAsync(int? campusId = null);
        Task<(bool Success, string Message)> AddLibraryBookAsync(LibraryBookDto book);
        Task<(bool Success, string Message)> UpdateLibraryBookAsync(LibraryBookDto book);
        Task<(bool Success, string Message)> AddLibraryBookItemAsync(int bookId, LibraryBookItemDto item);
        Task<(bool Success, string Message)> UpdateLibraryBookItemAsync(int itemId, LibraryBookItemDto item);
        Task<(bool Success, string Message)> DeleteLibraryBookItemAsync(int itemId);
    }

    public class LibraryService : BaseService, ILibraryService
    {
        public LibraryService(HttpClient http, IHttpContextAccessor httpContextAccessor) : base(http, httpContextAccessor) { }

        public async Task<List<LibraryBookDto>> GetLibraryBooksAsync(int? campusId = null)
        {
            try
            {
                var url = "api/Library?";
                if (campusId.HasValue && campusId.Value > 0) url += $"campusId={campusId.Value}";

                var request = await CreateRequestAsync(HttpMethod.Get, url.TrimEnd('?', '&'));
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<LibraryBookDto>>() ?? new();
                }
                return new();
            }
            catch { return new(); }
        }

        public async Task<(bool Success, string Message)> AddLibraryBookAsync(LibraryBookDto book)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, "api/Library", book);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Book added successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateLibraryBookAsync(LibraryBookDto book)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Library/{book.ItemId}", book);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Book updated successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> AddLibraryBookItemAsync(int bookId, LibraryBookItemDto item)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Post, $"api/Library/{bookId}/items", item);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Item added successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> UpdateLibraryBookItemAsync(int itemId, LibraryBookItemDto item)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Put, $"api/Library/items/{itemId}", item);
                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode) return (true, "Item updated successfully");

                var error = await response.Content.ReadAsStringAsync();
                return (false, $"API Error: {error}");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<(bool Success, string Message)> DeleteLibraryBookItemAsync(int itemId)
        {
            try
            {
                var request = await CreateRequestAsync(HttpMethod.Delete, $"api/Library/items/{itemId}");
                var response = await _http.SendAsync(request);
                return (response.IsSuccessStatusCode, response.IsSuccessStatusCode ? "Item deleted" : "Failed to delete item");
            }
            catch { return (false, "Error deleting item"); }
        }
    }
}
