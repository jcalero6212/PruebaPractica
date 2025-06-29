namespace MyApp.Entities
{
    public class Articulo
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public int CategoriaId { get; set; }
        public Categoria Categoria { get; set; } = null!;
        public int EstadoId { get; set; }
        public Estado Estado { get; set; } = null!;
        public string? Ubicacion { get; set; }
        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}
