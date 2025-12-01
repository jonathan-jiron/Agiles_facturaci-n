namespace UI.Validators;

public static class SriValidator
{
    /// <summary>
    /// Valida un número de RUC ecuatoriano (13 dígitos)
    /// </summary>
    public static bool ValidarRuc(string ruc)
    {
        if (string.IsNullOrWhiteSpace(ruc) || ruc.Length != 13)
            return false;

        if (!ruc.All(char.IsDigit))
            return false;

        // Los primeros 10 dígitos deben ser una cédula válida
        var cedula = ruc.Substring(0, 10);
        if (!ValidarCedula(cedula))
            return false;

        // Los últimos 3 dígitos deben ser 001 o superior
        var establecimiento = ruc.Substring(10, 3);
        return int.TryParse(establecimiento, out int est) && est >= 1;
    }

    /// <summary>
    /// Valida una cédula ecuatoriana (10 dígitos) usando algoritmo módulo 10
    /// </summary>
    public static bool ValidarCedula(string cedula)
    {
        if (string.IsNullOrWhiteSpace(cedula) || cedula.Length != 10)
            return false;

        if (!cedula.All(char.IsDigit))
            return false;

        // Los dos primeros dígitos deben ser entre 01 y 24 (provincias)
        var provincia = int.Parse(cedula.Substring(0, 2));
        if (provincia < 1 || provincia > 24)
            return false;

        // Validación del dígito verificador (módulo 10)
        int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
        int suma = 0;

        for (int i = 0; i < 9; i++)
        {
            int digito = int.Parse(cedula[i].ToString());
            int producto = digito * coeficientes[i];
            suma += producto > 9 ? producto - 9 : producto;
        }

        int digitoVerificador = (10 - (suma % 10)) % 10;
        return digitoVerificador == int.Parse(cedula[9].ToString());
    }

    /// <summary>
    /// Valida formato de email
    /// </summary>
    public static bool ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida que un tipo de identificación sea válido según el SRI
    /// </summary>
    public static bool ValidarTipoIdentificacion(string tipo)
    {
        var tiposValidos = new[] { "RUC", "CEDULA", "PASAPORTE" };
        return tiposValidos.Contains(tipo?.ToUpper());
    }
}
