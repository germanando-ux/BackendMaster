using Store.Web.Models;
using System.Net.Http.Json;

namespace Store.Web.Services
{
    public class VentaService : IVentaService
    {
        private readonly HttpClient _http;

        public VentaService(HttpClient http)
        {
            _http = http;
        }

        public async Task<int> CrearVentaAsync(VentaCreateDto ventaDto)
        {
            var response = await _http.PostAsJsonAsync("ventas", ventaDto);

            if (response.IsSuccessStatusCode)
            {
                var ventaId = await response.Content.ReadFromJsonAsync<int>();
                return ventaId;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error en la venta: {error}");
            }
        }
    }
}
