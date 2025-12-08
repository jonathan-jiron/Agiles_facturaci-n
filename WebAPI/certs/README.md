# Certificados SRI

Esta carpeta contiene los certificados digitales (.p12 o .pfx) para firmar los comprobantes electrónicos del SRI.

## ⚠️ IMPORTANTE

- **NO subir certificados al repositorio Git** (ya están en .gitignore)
- **Mantener la contraseña del certificado en User Secrets o variables de entorno**

## Configuración

1. Coloca tu certificado `.p12` o `.pfx` en esta carpeta
2. Actualiza `appsettings.Development.json` con el nombre correcto del archivo:
   ```json
   "P12Path": "certs/tuCertificado.p12"
   ```
3. Configura la contraseña usando User Secrets:
   ```powershell
   cd WebAPI
   dotnet user-secrets set "Sri:P12Password" "TU_CONTRASEÑA"
   ```

## Notas

- El certificado debe tener la clave privada incluida
- Para producción, usa variables de entorno o Azure Key Vault
- El certificado debe ser válido y emitido por una CA reconocida por el SRI

