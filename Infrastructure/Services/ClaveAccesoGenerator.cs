using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class ClaveAccesoGenerator
{
    private readonly IConfiguration _config;

    public ClaveAccesoGenerator(IConfiguration config) => _config = config;

    /// <summary>
    /// Genera la Clave de Acceso de 49 dígitos, incluyendo el dígito verificador Módulo 11.
    /// </summary>
    public string GenerarClaveAcceso(DateTime fechaEmision, string secuencial)
    {
        string ruc = _config["SRI:RucEmisor"]!;
        string ambiente = _config["SRI:Ambiente"]!;
        string tipoEmision = _config["SRI:TipoEmision"]!;
        string codDoc = "01"; // Factura

        // Formatos de 8, 2, 13, 1, 6, 9, 8, 1 dígitos respectivamente (total 48)
        string fecha = fechaEmision.ToString("ddMMyyyy");
        string serie = $"{_config["SRI:CodigoEstablecimiento"]}{_config["SRI:PuntoEmision"]}"; // ej: 001001
        string codigoNumerico = GenerateRandomNumericCode(8);

        string baseClave = $"{fecha}{codDoc}{ruc}{ambiente}{serie}{secuencial}{codigoNumerico}{tipoEmision}";

        int dv = CalcularDigitoVerificador(baseClave);

        return baseClave + dv.ToString();
    }

    private int CalcularDigitoVerificador(string baseClave)
    {
        int total = 0;
        int factor = 2;

        for (int i = baseClave.Length - 1; i >= 0; i--)
        {
            int digito = int.Parse(baseClave[i].ToString());
            total += digito * factor;
            factor++;
            if (factor > 7) factor = 2; // Ciclo de 2 a 7
        }

        int modulo = total % 11;

        if (modulo == 0) return 0;
        return 11 - modulo;
    }

    private string GenerateRandomNumericCode(int length)
    {
        // Generación de código numérico aleatorio de 8 dígitos para la Clave de Acceso
        var bytes = RandomNumberGenerator.GetBytes(length);
        var sb = new StringBuilder();
        foreach (byte b in bytes)
        {
            sb.Append((b % 10).ToString());
        }
        return sb.ToString().Substring(0, length);
    }
}