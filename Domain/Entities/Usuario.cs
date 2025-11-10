namespace Domain.Entities
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty; // 👈 Cambiar de NombreUsuario
        public string PasswordHash { get; set; } = string.Empty; // 👈 Cambiar de Contrasena
        public string Rol { get; set; } = "USER"; // 👈 NUEVO
    }
}
