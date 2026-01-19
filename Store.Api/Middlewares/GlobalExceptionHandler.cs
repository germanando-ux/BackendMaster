using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Store.Api.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly bool _showCriticalData;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _showCriticalData = configuration.GetValue<bool>("ParamProgram:ShowCriticalData");
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            
           

            // --- LA MAGIA PARA ELASTICSEARCH ---
            // Al usar parámetros ({Method} {Path}), NLog y Elastic crean campos indexables.
            // Podrás buscar en Kibana: Action: "GET" AND Path: "/api/products"
            _logger.LogError(exception,
                "Error detectado en {Method} {Path}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);

            // Determinamos el StatusCode y el Título según el tipo de excepción
            var (statusCode, title) = exception switch
            {
                ArgumentException => (StatusCodes.Status400BadRequest, "Parámetro incorrecto"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Acceso denegado"),

                 // Errores de EF Core: Concurrencia (alguien editó el registro al mismo tiempo)
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "El recurso fue modificado por otro usuario"),

                // Errores de EF Core: Conflictos de integridad (ej: borrar categoría con productos)
                DbUpdateException => (StatusCodes.Status409Conflict, "Conflicto de integridad en la base de datos"),

               
                OperationCanceledException => (499, "Cliente cerró la solicitud"), // Caso asíncrono
                _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
            };


            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,  // Aquí pasamos el mensaje real ("Acceso denegado de prueba")
                Instance = httpContext.Request.Path,
                // Añade el Type para seguir el estándar RFC
                Type = $"https://httpstatuses.io/{statusCode}"
            };

            // Si la configuración lo permite, añadimos el StackTrace para debugging
            if (_showCriticalData)
            {
                problemDetails.Extensions.Add("stackTrace", exception.StackTrace);
            }

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // Indica que la excepción ha sido manejada
        }

    }
}
