using Microsoft.AspNetCore.Mvc;
using MyApp.Entities;
using MyApp.DataAccess.UnitOfWork;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Microsoft.AspNetCore.Authorization;

namespace MyApp.Presentation.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ArticulosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ArticulosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var articulos = await _unitOfWork.Articulos.GetAllAsync();
            return Ok(articulos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var articulo = await _unitOfWork.Articulos.GetByIdAsync(id);
            if (articulo == null) return NotFound();
            return Ok(articulo);
        }


        [HttpGet("exportar-pdf")]
        [Authorize(Roles = "Administrador")] // solo para rol Administrador (si configuras roles)
        public async Task<IActionResult> ExportarPdf()

        {
            var articulos = await _unitOfWork.Articulos.GetAllAsync();

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Arial", 12, XFontStyle.Regular);
            int y = 40;

            gfx.DrawString("Listado de Artículos", new XFont("Arial", 14, XFontStyle.Bold), XBrushes.Black, new XPoint(40, 20));
            gfx.DrawString("Código", font, XBrushes.Black, new XPoint(40, y));
            gfx.DrawString("Nombre", font, XBrushes.Black, new XPoint(120, y));
            gfx.DrawString("Categoría", font, XBrushes.Black, new XPoint(240, y));
            gfx.DrawString("Estado", font, XBrushes.Black, new XPoint(360, y));
            gfx.DrawString("Ubicación", font, XBrushes.Black, new XPoint(460, y));
            y += 20;

            foreach (var a in articulos)
            {
                if (y > page.Height - 40)
                {
                    page = document.AddPage();
                    gfx = XGraphics.FromPdfPage(page);
                    y = 40;
                }

                gfx.DrawString(a.Codigo, font, XBrushes.Black, new XPoint(40, y));
                gfx.DrawString(a.Nombre, font, XBrushes.Black, new XPoint(120, y));
                gfx.DrawString(a.Categoria?.Nombre ?? "", font, XBrushes.Black, new XPoint(240, y));
                gfx.DrawString(a.Estado?.Nombre ?? "", font, XBrushes.Black, new XPoint(360, y));
                gfx.DrawString(a.Ubicacion, font, XBrushes.Black, new XPoint(460, y));
                y += 20;
            }

            using var stream = new MemoryStream();
            document.Save(stream, false);
            return File(stream.ToArray(), "application/pdf", "articulos.pdf");
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromBody] Articulo articulo)
        {
            if (await _unitOfWork.Articulos.ExistsAsync(a => a.Codigo == articulo.Codigo))
                return BadRequest("El código ya está en uso.");

            await _unitOfWork.Articulos.AddAsync(articulo);
            await _unitOfWork.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = articulo.Id }, articulo);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] Articulo articulo)
        {
            var existente = await _unitOfWork.Articulos.GetByIdAsync(id);
            if (existente == null) return NotFound();

            if (articulo.Codigo != existente.Codigo &&
                await _unitOfWork.Articulos.ExistsAsync(a => a.Codigo == articulo.Codigo))
                return BadRequest("El nuevo código ya está en uso por otro artículo.");

            existente.Codigo = articulo.Codigo;
            existente.Nombre = articulo.Nombre;
            existente.CategoriaId = articulo.CategoriaId;
            existente.EstadoId = articulo.EstadoId;
            existente.Ubicacion = articulo.Ubicacion;

            _unitOfWork.Articulos.Update(existente);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var articulo = await _unitOfWork.Articulos.GetByIdAsync(id);
            if (articulo == null) return NotFound();

            bool tienePrestamosActivos = articulo.Prestamos.Any(p => p.Estado != "Devuelto");
            if (tienePrestamosActivos)
                return BadRequest("No se puede eliminar el artículo porque tiene préstamos activos.");

            _unitOfWork.Articulos.Delete(articulo);
            await _unitOfWork.SaveAsync();
            return NoContent();
        }

        [HttpGet("buscar")]
        public async Task<IActionResult> Buscar([FromQuery] string? texto, [FromQuery] int? categoriaId, [FromQuery] int? estadoId)
        {
            var articulos = await _unitOfWork.Articulos.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(texto))
                articulos = articulos.Where(a =>
                    a.Nombre.Contains(texto, StringComparison.OrdinalIgnoreCase) ||
                    a.Codigo.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();

            if (categoriaId.HasValue)
                articulos = articulos.Where(a => a.CategoriaId == categoriaId.Value).ToList();

            if (estadoId.HasValue)
                articulos = articulos.Where(a => a.EstadoId == estadoId.Value).ToList();

            return Ok(articulos);
        }
    }
}


