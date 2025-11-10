namespace Domain.Entities
{
    public class EventoActividad
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public string Icono { get; set; } = "";
        public string Color { get; set; } = "";
        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}