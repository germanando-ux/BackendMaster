using Store.Web.Models;
using System.Net.Http.Json;
using static System.Net.WebRequestMethods;

namespace Store.Web.Services
{
    public class CategoryService: ICategoryService
    {
        private readonly HttpClient _http;

        public CategoryService(HttpClient http)
        {
            _http = http;
        }
        // Obtener la lista completa de productos
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _http.GetFromJsonAsync<List<Category>>("categories") ?? new();
        }

    }
}
