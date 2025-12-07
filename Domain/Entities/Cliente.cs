using System.ComponentModel.DataAnnotations;

namespace Domain.Entities;

public class Cliente
{
    public int Id { get; set; }

    [Required, MaxLength(20)]
    public string TipoIdentificacion { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string NombreRazonSocial { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Identificacion { get; set; } = string.Empty;

    [EmailAddress, MaxLength(150)]
    public string? Email { get; set; } // Renombrado

    [MaxLength(30)]
    public string? Telefono { get; set; }

    [MaxLength(250)]
    public string? Direccion { get; set; }

    [MaxLength(20)]
    public string? TipoCliente { get; set; } // Nuevo campo

    // Soft delete
    public bool IsDeleted { get; set; } = false;

    // Concurrency opcional
    public byte[]? RowVersion { get; set; }
}
