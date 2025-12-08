using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using FirmaXadesNetCore;
using FirmaXadesNetCore.Signature.Parameters;
using FirmaXadesNetCore.Signature;
using System.IO;

namespace Infrastructure.Services.Sri
{
    public class XadesSigner
    {
        private readonly SriOptions _opt;
        public XadesSigner(IOptions<SriOptions> options) => _opt = options.Value;

        public string SignXml(string unsignedXml)
        {
            var p12Path = Path.Combine(AppContext.BaseDirectory, _opt.P12Path);
            var password = _opt.P12Password ?? Environment.GetEnvironmentVariable("SRI_P12_PASSWORD");
            if (string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("No se encontró la contraseña del .p12 (Sri:P12Password o SRI_P12_PASSWORD)");
            if (!File.Exists(p12Path))
                throw new FileNotFoundException("No se encontró el archivo .p12", p12Path);

            var cert = new X509Certificate2(
                p12Path,
                password,
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

            using var inputXmlStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(unsignedXml));
            using var outputXmlStream = new MemoryStream();

            var xadesService = new XadesService();
            var parameters = new SignatureParameters
            {
                SignaturePolicyInfo = new SignaturePolicyInfo
                {
                    PolicyIdentifier = "http://www.w3.org/2000/09/xmldsig#",
                    PolicyHash = "nulo"
                },
                SignaturePackaging = SignaturePackaging.ENVELOPED,
                Signer = new FirmaXadesNetCore.Crypto.Signer(cert)
            };

            // Cargar el XML en un XmlDocument
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.PreserveWhitespace = true;
            inputXmlStream.Position = 0;
            using (var xmlReader = new StreamReader(inputXmlStream, System.Text.Encoding.UTF8, false, 1024, true))
            {
                xmlDoc.Load(xmlReader);
            }

            // Firma el XML (inputXmlStream, parameters)
            inputXmlStream.Position = 0;
            var signedXmlDoc = xadesService.Sign(inputXmlStream, parameters);

            // Escribir el XML firmado al output stream
            signedXmlDoc.Save(outputXmlStream);
            outputXmlStream.Position = 0;
            using var reader = new StreamReader(outputXmlStream);
            return reader.ReadToEnd();
        }

        public bool TestCertificate()
        {
            try
            {
                var p12Path = Path.Combine(AppContext.BaseDirectory, _opt.P12Path);
                var password = _opt.P12Password ?? Environment.GetEnvironmentVariable("SRI_P12_PASSWORD");
                var cert = new X509Certificate2(
                    p12Path,
                    password,
                    X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
                return cert != null && cert.HasPrivateKey;
            }
            catch
            {
                return false;
            }
        }

        public object VerificarCertificadoYRuc()
        {
            try
            {
                var p12Path = Path.Combine(AppContext.BaseDirectory, _opt.P12Path);
                var password = _opt.P12Password ?? Environment.GetEnvironmentVariable("SRI_P12_PASSWORD");
                var cert = new X509Certificate2(
                    p12Path,
                    password,
                    X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);

                var subject = cert.Subject;
                var rucEnCertificado = subject.Contains(_opt.EmisorRuc);
                return new
                {
                    CertificadoValido = cert.HasPrivateKey,
                    RucCoincide = rucEnCertificado,
                    Subject = subject
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    CertificadoValido = false,
                    RucCoincide = false,
                    Error = ex.Message
                };
            }
        }
    }
}
