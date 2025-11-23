using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task Enviar(string destinatario, byte[] xml, byte[] pdf);
    }
}
