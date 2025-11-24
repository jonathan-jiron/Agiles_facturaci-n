using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config) => _config = config;

        public async Task Enviar(string destinatario, byte[] xml, byte[] pdf)
        {
            if (string.IsNullOrEmpty(destinatario))
                throw new ArgumentException("El destinatario no puede ser nulo o vacío.", nameof(destinatario));

            var mail = new MailAddress(destinatario);

            var smtpUser = _config["SMTP:User"];
            var smtpHost = _config["SMTP:Host"];
            var smtpPass = _config["SMTP:Pass"];
            var smtpPortStr = _config["SMTP:Port"];

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpPass) || string.IsNullOrEmpty(smtpPortStr))
                throw new InvalidOperationException("Configuración SMTP incompleta.");

            if (!int.TryParse(smtpPortStr, out int smtpPort))
                throw new InvalidOperationException("El puerto SMTP no es válido.");

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser),
                Subject = "Factura electrónica autorizada",
                Body = "Adjuntamos su factura autorizada por el SRI.",
            };

            mailMessage.To.Add(mail);
            mailMessage.Attachments.Add(new Attachment(new MemoryStream(xml), "factura.xml"));
            mailMessage.Attachments.Add(new Attachment(new MemoryStream(pdf), "factura.pdf"));

            using var smtp = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mailMessage);
        }
    }
}
