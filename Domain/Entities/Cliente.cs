using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Cliente
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string TipoIdentificacion { get; set; } = string.Empty; // <-- AGREGA ESTA LÍNEA

    [Required, MaxLength(200)]
    public string NombreRazonSocial { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Identificacion { get; set; } = string.Empty;

    [EmailAddress, MaxLength(150)]
    public string? Correo { get; set; }

    [MaxLength(30)]
    public string? Telefono { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }

    // Concurrency opcional
    public byte[]? RowVersion { get; set; }
}
