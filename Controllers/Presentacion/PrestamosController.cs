using Microsoft.AspNetCore.Mvc;
using MyApp.Entities;
using MyApp.DataAccess.UnitOfWork;

namespace MyApp.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrestamosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PrestamosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }



        [HttpPost("solicitar")]
        public async Task<IActionResult> Solicitar([FromBody] SolicitudDto dto)
        {
            var articulo = await _unitOfWork.Articulos.GetByIdAsync(dto.ArticuloId);
            if (articulo == null || articulo.EstadoId != 1)
                return BadRequest("El artículo no está disponible.");

            var prestamo = new Prestamo
            {
                UsuarioId = dto.UsuarioId,
                ArticuloId = dto.ArticuloId,
                FechaSolicitud = DateTime.Now,
                FechaEntrega = dto.FechaEntrega,
                Estado = "Pendiente"
            };

            await _unitOfWork.Prestamos.AddAsync(prestamo);
            await _unitOfWork.SaveAsync();

            return Ok(prestamo);
        }

        [HttpPut("{id}/cambiar-estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoCambioDto dto)
        {
            var prestamo = await _unitOfWork.Prestamos.GetByIdAsync(id);
            if (prestamo == null) return NotFound();

            if (dto.NuevoEstado != "Aprobado" && dto.NuevoEstado != "Rechazado")
                return BadRequest("Estado inválido.");

            prestamo.Estado = dto.NuevoEstado;

            if (dto.NuevoEstado == "Aprobado")
            {
                var articulo = await _unitOfWork.Articulos.GetByIdAsync(prestamo.ArticuloId);
                if (articulo != null) articulo.EstadoId = 2; 
            }

            _unitOfWork.Prestamos.Update(prestamo);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}/devolver")]
        public async Task<IActionResult> Devolver(int id)
        {
            var prestamo = await _unitOfWork.Prestamos.GetByIdAsync(id);
            if (prestamo == null || prestamo.Estado != "Aprobado")
                return BadRequest("No se puede devolver este préstamo.");

            prestamo.FechaDevolucion = DateTime.Now;
            prestamo.Estado = "Devuelto";

            var articulo = await _unitOfWork.Articulos.GetByIdAsync(prestamo.ArticuloId);
            if (articulo != null) articulo.EstadoId = 1; 

            _unitOfWork.Prestamos.Update(prestamo);
            await _unitOfWork.SaveAsync();

            return NoContent();
        }

        [HttpGet("historial")]
            public async Task<IActionResult> Historial(
        [FromQuery] int? usuarioId,
        [FromQuery] int? articuloId,
        [FromQuery] string? estado)
        {
            var prestamos = await _unitOfWork.Prestamos.GetAllAsync();

            if (usuarioId.HasValue)
                prestamos = prestamos.Where(p => p.UsuarioId == usuarioId.Value).ToList();

            if (articuloId.HasValue)
                prestamos = prestamos.Where(p => p.ArticuloId == articuloId.Value).ToList();

            if (!string.IsNullOrWhiteSpace(estado))
                prestamos = prestamos.Where(p => p.Estado.Equals(estado, StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(prestamos);
        }

        [HttpGet("exportar-excel")]
        public async Task<IActionResult> ExportarExcel()
        {
            var prestamos = await _unitOfWork.Prestamos.GetAllAsync();

            using var wb = new ClosedXML.Excel.XLWorkbook();
            var ws = wb.Worksheets.Add("Préstamos");

            ws.Cell(1, 1).Value = "ID";
            ws.Cell(1, 2).Value = "Usuario ID";
            ws.Cell(1, 3).Value = "Artículo ID";
            ws.Cell(1, 4).Value = "Fecha Solicitud";
            ws.Cell(1, 5).Value = "Fecha Entrega";
            ws.Cell(1, 6).Value = "Fecha Devolución";
            ws.Cell(1, 7).Value = "Estado";

            int fila = 2;
            foreach (var p in prestamos)
            {
                ws.Cell(fila, 1).Value = p.Id;
                ws.Cell(fila, 2).Value = p.UsuarioId;
                ws.Cell(fila, 3).Value = p.ArticuloId;
                ws.Cell(fila, 4).Value = p.FechaSolicitud.ToString("yyyy-MM-dd");
                ws.Cell(fila, 5).Value = p.FechaEntrega.ToString("yyyy-MM-dd");
                ws.Cell(fila, 6).Value = p.FechaDevolucion?.ToString("yyyy-MM-dd");
                ws.Cell(fila, 7).Value = p.Estado;
                fila++;
            }

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "prestamos.xlsx");
        }


    }

    public record SolicitudDto(int UsuarioId, int ArticuloId, DateTime FechaEntrega);
    public record EstadoCambioDto(string NuevoEstado);
}

