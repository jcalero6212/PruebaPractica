namespace MyApp.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public int RolId { get; set; }
        public Rol Rol { get; set; } = null!;
        public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    }
}
