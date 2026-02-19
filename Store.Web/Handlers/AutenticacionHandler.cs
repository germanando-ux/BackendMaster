using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Store.Web.Handlers
{
    /// <summary>
    /// Intercepta las comunicaciones HTTP salientes para adjuntar el token de seguridad
    /// y gestionar el cierre de sesión si el token no es válido (401).
    /// </summary>
    public class AutenticacionHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;
        private readonly NavigationManager _navigation;

        /// <summary>
        /// Constructor del interceptor de autenticación.
        /// </summary>
        /// <param name="js">Servicio para interactuar con el localStorage del navegador.</param>
        /// <param name="navigation">Servicio para redirigir al usuario en caso de error.</param>
        public AutenticacionHandler(IJSRuntime js, NavigationManager navigation)
        {
            _js = js;
            _navigation = navigation;
        }
        /// <summary>
        /// Método que procesa la petición, añade el encabezado Bearer y vigila la respuesta del servidor.
        /// </summary>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Recuperamos el token almacenado
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Si el servidor responde con 401, limpiamos el rastro y mandamos al Login
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
                _navigation.NavigateTo("/");
            }

            return response;
        }
    }
}
