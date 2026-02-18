using Microsoft.JSInterop;
using Store.Web.Models;
using System.Net.Http.Json;

namespace Store.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }
        public async Task<string?> Login(LoginDto loginDto)
        {
            //hacemos la llamada a la api 
            var response = await _httpClient.PostAsJsonAsync("auth/login", loginDto);
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();

                // 3.Lo guardamos en el LocalStorage usando JavaScript
                // El primer parámetro es el nombre de la "llave" y el segundo el valor
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
                return token;
            }

            return null;
        }

        public async Task Logout()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }

        public async Task<bool> Register(RegisterDto registerDto)
        {
            //enviamos los datos de registro a la api
            var response = await _httpClient.PostAsJsonAsync("auth/register", registerDto);

            //Evaluamos si la respuesta fue exitosa(código 200 - 299)
            return response.IsSuccessStatusCode;
        }
    }
}
