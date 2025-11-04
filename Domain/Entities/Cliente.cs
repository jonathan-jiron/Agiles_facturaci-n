namespace Domain.Entities
{
    public class Cliente
    {
        public int Id { get; set; }
        public string CedulaRuc { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Direccion { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
    }
}
