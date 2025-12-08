using System.Globalization;
using System.Text;
namespace Infrastructure.Services.Sri;
public class ClaveAccesoGenerator
{
public string Generar(
DateTime fechaEmision,
string codDoc, // "01" factura
string rucEmisor, // 13
string ambiente, // "1" pruebas
string estab, // 3
string ptoEmi, // 3
string secuencial, // 9
string codigoNumerico, // 8 (aleatorio/param)
string tipoEmision // "1"
)
{
string fecha = fechaEmision.ToString("ddMMyyyy", CultureInfo.InvariantCulture);
string base49 = $"{fecha}{codDoc}{rucEmisor}{ambiente}{estab}{ptoEmi}{secuencial}{codigoNumerico}{tipoEmision}";
int digito = Modulo11(base49);
return base49 + digito.ToString(CultureInfo.InvariantCulture);
}
// Regla SRI: módulo 11 con factores 2..7 en ciclo.
private static int Modulo11(string cadena)
{
int factor = 2;
int suma = 0;
for (int i = cadena.Length - 1; i >= 0; i--)
{
int num = cadena[i] - '0';
suma += num * factor;
factor++;
if (factor > 7) factor = 2;
}
int mod = suma % 11;
int verificador = 11 - mod;
if (verificador == 11) verificador = 0;
if (verificador == 10) verificador = 1;
return verificador;
}
public static string Secuencial9(int numero) => numero.ToString("D9");
public static string CodigoNumerico8(int seed)
{
// “random” determinístico simple (para pruebas). En prod usa RNGCrypto.
var rnd = new Random(seed);
return rnd.Next(0, 99999999).ToString("D8");
}
}