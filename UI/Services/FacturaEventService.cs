public class FacturaEventService
{
    public event Action? FacturaGuardada;

    public void NotificarFacturaGuardada()
    {
        FacturaGuardada?.Invoke();
    }
}