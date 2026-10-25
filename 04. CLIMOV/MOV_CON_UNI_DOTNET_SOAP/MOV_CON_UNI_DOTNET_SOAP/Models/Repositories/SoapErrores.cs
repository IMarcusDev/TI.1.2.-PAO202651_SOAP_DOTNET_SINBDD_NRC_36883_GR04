using MOV_CON_UNI_DOTNET_SOAP.Config;
using System.ServiceModel;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Repositories
{
    internal static class SoapErrores
    {
        public const string SinConexion = "Sin conexión a internet. Revise el Wi-Fi o los datos móviles.";
        public const string RespuestaIncompleta = "El servidor envió una respuesta vacía o incompleta.";
        public const string SesionExpirada = "Su sesión expiró o no es válida. Inicie sesión nuevamente.";

        public static bool SinRed()
        {
            try
            {
                return Connectivity.Current.NetworkAccess == NetworkAccess.None;
            }
            catch
            {
                return false;
            }
        }

        public static string Mensaje(Exception ex)
        {
            string mensaje = MensajeAmigable(ex);
#if DEBUG
            Exception causa = ex.GetBaseException();
            mensaje += $"\n[Debug] {causa.GetType().Name}: {causa.Message}";
#endif
            return mensaje;
        }

        private static string MensajeAmigable(Exception ex) => ex switch
        {
            EndpointNotFoundException =>
                $"No se encontró el servidor en {SoapConfig.BaseUrl}. Verifique que esté encendido y que la IP sea correcta.",
            FaultException fault =>
                $"El servidor no pudo procesar la solicitud: {fault.Message}",
            CommunicationException =>
                "No se pudo comunicar con el servidor. Verifique que esté encendido y en la misma red.",
            TimeoutException =>
                $"El servidor no respondió en {SoapConfig.Timeout.TotalSeconds:0} segundos. Intente de nuevo.",
            UriFormatException =>
                "La dirección del servidor está mal configurada.",
            _ =>
                "Ocurrió un error inesperado al contactar al servidor.",
        };

        public static string MensajeServidor(string? mensaje, string porDefecto)
            => string.IsNullOrWhiteSpace(mensaje) ? porDefecto : mensaje.Trim();

        public static bool EsTokenInvalido(string? mensaje)
            => mensaje?.Contains("token", StringComparison.OrdinalIgnoreCase) == true;
    }
}
