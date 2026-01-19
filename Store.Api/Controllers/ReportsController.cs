using Dapper;
using Microsoft.AspNetCore.Mvc;
using Store.Data.Repositories;

namespace Store.Api.Controllers
{
    /// <summary>
    /// Controlador especializado en la generación de informes de alto rendimiento.
    /// Utiliza Dapper para realizar consultas optimizadas directamente a la base de datos,
    /// evitando el overhead de seguimiento de entidades de Entity Framework.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportsController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia de <see cref="ReportsController"/>.
        /// </summary>
        /// <param name="unitOfWork">Unidad de trabajo que provee la conexión a la base de datos.</param>
        /// <param name="logger">Servicio de logging para telemetría y diagnóstico.</param>
        public ReportsController(IUnitOfWork unitOfWork, ILogger<ReportsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene un resumen del inventario agrupado por categoría.
        /// Este endpoint utiliza SQL puro mediante Dapper para maximizar la velocidad de respuesta.
        /// </summary>
        /// <remarks>
        /// La consulta realiza un JOIN entre productos y categorías, calculando el total de items 
        /// y el valor financiero del stock por cada grupo.
        /// </remarks>
        /// <returns>Una lista de objetos dinámicos con el resumen por categoría.</returns>
        /// <response code="200">Retorna el informe generado exitosamente.</response>
        /// <response code="500">Si ocurrió un error interno en el servidor o en la consulta SQL.</response>
        [HttpGet("inventory-summary")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventorySummary()
        {
            // Iniciamos un cronómetro para enviar telemetría precisa a ElasticSearch
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _logger.LogInformation("Ejecutando consulta de informe de inventario (Dapper Engine)");

            try
            {
                // Definimos el SQL plano. 
                // IMPORTANTE: En PostgreSQL las comillas dobles son necesarias si hay mayúsculas en los nombres de tablas.
                const string sql = @"
                    SELECT 
                        c.""Name"" AS CategoryName, 
                        COUNT(p.""Id"") AS TotalProducts, 
                        SUM(p.""Price"") AS StockValue
                    FROM ""Products"" p
                    INNER JOIN ""Categories"" c ON p.""CategoryId"" = c.""Id""
                    GROUP BY c.""Name""
                    ORDER BY StockValue DESC";

                // Ejecutamos la consulta. Dapper gestiona la apertura y cierre lógico de la conexión
                // si se usa adecuadamente a través del pool de conexiones de EF Core.
                var report = await _unitOfWork.Connection.QueryAsync(sql);

                watch.Stop();

                // Registro de métricas para análisis en Kibana
                _logger.LogInformation("Informe de inventario generado. Registros: {Count} | Tiempo: {Elapsed}ms",
                    report.Count(), watch.ElapsedMilliseconds);

                return Ok(report);
            }
            catch (Exception ex)
            {
                // El log de error incluirá el StackTrace completo para ElasticSearch
                _logger.LogError(ex, "Error crítico al ejecutar el informe de inventario mediante SQL directo.");
                return StatusCode(500, "Error de base de datos al generar el informe solicitado.");
            }
        }
    }
}
