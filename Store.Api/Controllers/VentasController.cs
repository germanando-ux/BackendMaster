using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Store.Api.Services;
using Store.Data.DTO;

namespace Store.Api.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class VentasController : ControllerBase
    {
        private readonly IVentaService _ventaService;

        public VentasController(IVentaService ventaService)
        {
            _ventaService = ventaService;
        }
        [HttpPost]
        public async Task<IActionResult> CrearVenta( VentaCreateDto ventaDto)
        {
            // El servicio ya hace las validaciones de productos y precios
            var ventaId = await _ventaService.CrearVentaAsync(ventaDto);

            // Retornamos un 201 Created y la ubicación del nuevo recurso
            return CreatedAtAction(nameof(ObtenerVentaPorId), new { id = ventaId }, ventaId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> ObtenerVentaPorId(int id)
        {
            var venta = await _ventaService.ObtenerVentaPorIdAsync(id);

            if (venta == null) return NotFound();

            return Ok(venta);
        }
    }
}
