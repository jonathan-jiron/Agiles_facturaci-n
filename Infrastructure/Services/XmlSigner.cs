using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using FirmaXadesNet;                         // XadesService
using FirmaXadesNet.Crypto;                 // Signer
using FirmaXadesNet.Signature.Parameters;   // SignatureParameters, SignatureMethod, SignaturePackaging, DigestMethod

namespace Infrastructure.Services
{
    public class XmlSigner : IXmlSigner
    {
        private readonly IConfiguration _config;

        public XmlSigner(IConfiguration config)
        {
            _config = config;
        }

        public byte[] FirmarXml(string xmlComprobante)
        {
            string p12Path = _config["SRI:CertificadoRuta"] ?? throw new ArgumentNullException("SRI:CertificadoRuta");
            string p12Pass = _config["SRI:CertificadoPassword"] ?? throw new ArgumentNullException("SRI:CertificadoPassword");

            if (!File.Exists(p12Path))
                throw new FileNotFoundException($"Certificado .p12 no encontrado: {p12Path}");

            // 1) Cargar certificado
            var cert = new X509Certificate2(p12Path, p12Pass,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.MachineKeySet);

            // 2) Crear el signer
            var signer = new Signer(cert);

            // 3) Configurar parámetros de firma
            var parametros = new SignatureParameters
            {
                Signer = signer,
                SignatureMethod = SignatureMethod.RSAwithSHA256,
                DigestMethod = DigestMethod.SHA256,
                SigningDate = DateTime.UtcNow,
                SignaturePackaging = SignaturePackaging.ENVELOPED
            };

            // 4) Convertir XML a Stream
            using var xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlComprobante));

            // 5) Firmar
            var xades = new XadesService();
            var resultado = xades.Sign(xmlStream, parametros);

            if (resultado == null)
                throw new InvalidOperationException("No se pudo generar la firma (resultado nulo)");

            // 6) Tomar el XML firmado correctamente
            if (resultado.Document != null)
                return Encoding.UTF8.GetBytes(resultado.Document.OuterXml);

            // Si no existe Document → error
            throw new InvalidOperationException(
                $"El objeto de firma no expone XML firmado. Tipo recibido: {resultado.GetType().FullName}"
            );
        }
    }
}
