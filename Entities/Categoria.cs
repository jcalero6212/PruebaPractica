namespace MyApp.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public ICollection<Articulo> Articulos { get; set; } = new List<Articulo>();
    }
}
