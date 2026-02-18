using Microsoft.AspNetCore.Components;

namespace Store.Web.Handlers
{
    /// <summary>
    /// Interceptor de peticiones HTTP que gestiona la inclusión del token de autenticación 
    /// y el cierre de sesión automático en caso de error 401.
    /// </summary>
    public class AutenticacionHandler: DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigation;
    }
}
