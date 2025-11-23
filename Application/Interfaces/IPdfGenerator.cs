using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPdfGenerator
    {
        byte[] Generar(string xmlAutorizado);
    }
}
