using Application.Interfaces;
using System.Xml.Linq;

namespace Infrastructure.Services;

public class FacturacionElectronicaService : IFacturacionElectronicaService
{
    private readonly IXmlValidator _xmlValidator;
    private readonly IXmlSigner _xmlSigner;
    private readonly ISriClientService _sriClient;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IEmailService _emailService;

    public FacturacionElectronicaService(
        IXmlValidator xmlValidator,
        IXmlSigner xmlSigner,
        ISriClientService sriClient,
        IPdfGenerator pdfGenerator,
        IEmailService emailService)
    {
        _xmlValidator = xmlValidator;
        _xmlSigner = xmlSigner;
        _sriClient = sriClient;
        _pdfGenerator = pdfGenerator;
        _emailService = emailService;
    }

    public async Task<string> ProcesarFacturaAsync(string xmlOriginal, string emailCliente)
    {
        // 1. Validar XML (Omitido por falta de XSD, pero la estructura está lista)
        // _xmlValidator.Validate(xmlOriginal, "path/to/xsd");

        // 2. Firmar XML
        byte[] signedXmlBytes;
        try
        {
            signedXmlBytes = _xmlSigner.FirmarXml(xmlOriginal);
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al firmar XML: {ex.Message}", ex);
        }

        string signedXmlBase64 = Convert.ToBase64String(signedXmlBytes);

        // 3. Enviar Recepción
        string recepcionResponse = await _sriClient.EnviarRecepcionAsync(signedXmlBase64);
        
        // El SRI puede devolver "RECIBIDA" o "DEVUELTA". 
        // Si es DEVUELTA, lanzamos excepción con el motivo.
        if (!recepcionResponse.Contains("Estado: RECIBIDA"))
        {
             throw new Exception($"SRI Recepción Fallida: {recepcionResponse}");
        }

        // 4. Extraer Clave de Acceso del XML
        string claveAcceso = ExtraerClaveAcceso(xmlOriginal);
        if (string.IsNullOrEmpty(claveAcceso))
        {
             throw new Exception("No se pudo extraer la Clave de Acceso del XML.");
        }

        // 5. Enviar Autorización (SRI demora un poco, a veces se requiere espera, pero intentamos directo)
        // En producción se recomienda un patrón de reintento o cola.
        await Task.Delay(3000); // Espera prudencial de 3s
        string xmlAutorizado = await _sriClient.EnviarAutorizacionAsync(claveAcceso);

        if (xmlAutorizado.Contains("NO AUTORIZADO") || xmlAutorizado.Contains("RECHAZADO"))
        {
             throw new Exception($"SRI Autorización Fallida: {xmlAutorizado}");
        }

        // Extraer el XML de la respuesta (formato: "Estado: X, AutorizacionXml: <xml>, Motivo: Y")
        string xmlParaPdf = xmlAutorizado;
        if (xmlAutorizado.Contains("AutorizacionXml:"))
        {
            var startIndex = xmlAutorizado.IndexOf("AutorizacionXml:") + "AutorizacionXml:".Length;
            var endIndex = xmlAutorizado.IndexOf(", Motivo:");
            if (endIndex > startIndex)
            {
                xmlParaPdf = xmlAutorizado.Substring(startIndex, endIndex - startIndex).Trim();
            }
        }

        Console.WriteLine($"[DEBUG PDF] XML completo recibido (primeros 200 chars): {xmlAutorizado.Substring(0, Math.Min(200, xmlAutorizado.Length))}");
        Console.WriteLine($"[DEBUG PDF] XML para PDF (primeros 200 chars): {xmlParaPdf.Substring(0, Math.Min(200, xmlParaPdf.Length))}");

        // 6. Generar PDF
        Console.WriteLine("[PDF] Generando PDF RIDE...");
        byte[] pdfBytes = _pdfGenerator.Generar(xmlParaPdf);

        // 7. Enviar Correo
        try 
        {
            Console.WriteLine($"[EMAIL] Intentando enviar correo a: {emailCliente}");
            await _emailService.Enviar(emailCliente, signedXmlBytes, pdfBytes);
            Console.WriteLine($"[EMAIL] Correo enviado exitosamente a: {emailCliente}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR EMAIL] No se pudo enviar el correo: {ex.Message}");
            Console.WriteLine($"[ERROR EMAIL] Stack trace: {ex.StackTrace}");
            // Log error de correo pero no fallar el proceso de facturación
        }

        return "AUTORIZADO";
    }

    private string ExtraerClaveAcceso(string xml)
    {
        try
        {
            var doc = XDocument.Parse(xml);
            // Buscar elemento claveAcceso sin importar namespace
            var claveNode = doc.Descendants().FirstOrDefault(n => n.Name.LocalName == "claveAcceso");
            return claveNode?.Value ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}
