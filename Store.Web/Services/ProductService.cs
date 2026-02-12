using Store.Web.Models;
using System.Net.Http.Json;

namespace Store.Web.Services
{
    public class ProductService
    {
        private readonly HttpClient _http;

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        // Obtener la lista completa de productos
        public async Task<List<Product>> GetProductsAsync()
        {
            return await _http.GetFromJsonAsync<List<Product>>("products") ?? new();
        }

        // Crear o actualizar producto
        public async Task SaveProductAsync(Product product)
        {
            if (product.Id == 0 || product.Id == null)
            {
                // Si el Id es 0 o null, es un producto nuevo
                await _http.PostAsJsonAsync("products", product);
            }
            else
            {
                // Si tiene Id, actualizamos el existente
                await _http.PutAsJsonAsync($"products/{product.Id}", product);
            }
        }

        // Eliminar producto
        public async Task DeleteProductAsync(int id)
        {
            await _http.DeleteAsync($"products/{id}");
        }
    }
}