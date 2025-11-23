using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISriClientService
    {
        Task<string> EnviarRecepcionAsync(string xmlFirmado);
        Task<string> EnviarAutorizacionAsync(string claveAcceso);
    }
}
