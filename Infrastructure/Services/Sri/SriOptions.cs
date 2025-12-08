namespace Infrastructure.Services.Sri
{
    public class SriOptions
    {
        public string Ambiente { get; set; } = "1"; // 1 pruebas, 2 producción
        public string TipoEmision { get; set; } = "1"; // 1 normal
        public string EmisorRuc { get; set; } = "";
        public string EmisorRazonSocial { get; set; } = "";
        public string EmisorNombreComercial { get; set; } = "";
        public string DirMatriz { get; set; } = "";
        public string Estab { get; set; } = "001";
        public string PtoEmi { get; set; } = "001";
        public string RecepcionWsdl { get; set; } = "";
        public string AutorizacionWsdl { get; set; } = "";
        public string P12Path { get; set; } = "";
        public string? P12Password { get; set; } // viene de secrets/env
        public int EsperaAutorizacionSeg { get; set; } = 8;
    }
}