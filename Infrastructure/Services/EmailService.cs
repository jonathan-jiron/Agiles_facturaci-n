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
            var mail = new MailMessage
            {
                From = new MailAddress(_config["SMTP:User"]),
                Subject = "Factura electrónica autorizada",
                Body = "Adjuntamos su factura autorizada por el SRI.",
            };

            mail.To.Add(destinatario);
            mail.Attachments.Add(new Attachment(new MemoryStream(xml), "factura.xml"));
            mail.Attachments.Add(new Attachment(new MemoryStream(pdf), "factura.pdf"));

            using var smtp = new SmtpClient(_config["SMTP:Host"], int.Parse(_config["SMTP:Port"]))
            {
                Credentials = new NetworkCredential(_config["SMTP:User"], _config["SMTP:Pass"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }
    }
}
