using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISriClientService
    {
        Task<string> EnviarRecepcionAsync(string xmlBase64);
        Task<string> EnviarAutorizacionAsync(string claveAcceso);
    }
}
