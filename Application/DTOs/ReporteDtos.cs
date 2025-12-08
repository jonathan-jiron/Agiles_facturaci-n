namespace Application.DTOs;

public class ReporteVentasDto
{
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public int TotalFacturas { get; set; }
    public decimal TotalVentas { get; set; }
    public decimal TotalIva { get; set; }
    public decimal TotalSubtotal { get; set; }
    public List<ReporteVentasPorDiaDto> VentasPorDia { get; set; } = new();
    public List<ReporteVentasPorClienteDto> VentasPorCliente { get; set; } = new();
    public List<ReporteProductosVendidosDto> ProductosVendidos { get; set; } = new();
}

public class ReporteVentasPorDiaDto
{
    public DateTime Fecha { get; set; }
    public int CantidadFacturas { get; set; }
    public decimal TotalVentas { get; set; }
}

public class ReporteVentasPorClienteDto
{
    public int ClienteId { get; set; }
    public string ClienteNombre { get; set; } = "";
    public int CantidadFacturas { get; set; }
    public decimal TotalVentas { get; set; }
}

public class ReporteProductosVendidosDto
{
    public int ProductoId { get; set; }
    public string ProductoCodigo { get; set; } = "";
    public string ProductoNombre { get; set; } = "";
    public int CantidadVendida { get; set; }
    public decimal TotalVentas { get; set; }
}

public class ReporteRequestDto
{
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public int? ClienteId { get; set; }
    public string? TipoReporte { get; set; } // "ventas", "clientes", "productos"
}

