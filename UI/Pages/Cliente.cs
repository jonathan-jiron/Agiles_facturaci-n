namespace UI.Pages
{
    public class Cliente
    {
        public int Id { get; set; }
        public string TipoIdentificacion { get; set; } = "";
        public string Identificacion { get; set; } = "";
        public string NombreRazonSocial { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Correo { get; set; } = "";
    }
}