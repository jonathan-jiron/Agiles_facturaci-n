namespace Application.Interfaces
{
    public interface IFacturaElectronicaService
    {
        Task<string> GenerarFirmarEnviarFacturaAsync(); // retorna número/estado o mensaje
    }
}
