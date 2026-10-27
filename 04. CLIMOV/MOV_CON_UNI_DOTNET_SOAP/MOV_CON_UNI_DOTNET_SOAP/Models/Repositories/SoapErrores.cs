using MOV_CON_UNI_DOTNET_SOAP.Config;
using System.Net.Sockets;
using System.ServiceModel;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Repositories
{
    internal static partial class SoapErrores
    {
        public const string SinConexion = "Sin conexión a internet. Revise el Wi-Fi o los datos móviles.";
        public const string RespuestaIncompleta = "El servidor envió una respuesta vacía o incompleta.";
        public const string SesionExpirada = "Su sesión expiró o no es válida. Inicie sesión nuevamente.";

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
                NoEncontrado(),
            FaultException fault =>
                $"El servidor no pudo procesar la solicitud: {fault.Message}",
            CommunicationException when Causas(ex).Any(EsServidorNoEncontrado) =>
                NoEncontrado(),
            CommunicationException when Causas(ex).Any(EsTimeout) =>
                SinRespuesta(),
            CommunicationException =>
                "No se pudo comunicar con el servidor. Verifique que esté encendido y en la misma red.",
            TimeoutException =>
                SinRespuesta(),
            UriFormatException =>
                "La dirección del servidor está mal configurada.",
            _ =>
                "Ocurrió un error inesperado al contactar al servidor.",
        };

        private static string NoEncontrado()
            => $"No se encontró el servidor en {SoapConfig.BaseUrl}. Verifique que esté encendido y que la IP sea correcta.";

        private static string SinRespuesta()
            => $"El servidor no respondió en {SoapConfig.Timeout.TotalSeconds:0} segundos. Intente de nuevo.";

        private static IEnumerable<Exception> Causas(Exception ex)
        {
            for (Exception? actual = ex; actual is not null; actual = actual.InnerException)
                yield return actual;
        }

        private static bool EsServidorNoEncontrado(Exception ex) => ex switch
        {
            SocketException s => s.SocketErrorCode is SocketError.ConnectionRefused or SocketError.HostNotFound
                or SocketError.HostUnreachable or SocketError.NetworkUnreachable or SocketError.NoData,
            HttpRequestException h => h.HttpRequestError == HttpRequestError.NameResolutionError,
            _ => ex.GetType().Name is "ConnectException" or "UnknownHostException" or "NoRouteToHostException",
        };

        private static bool EsTimeout(Exception ex)
            => ex is TimeoutException || ex.GetType().Name == "SocketTimeoutException";

        public static string MensajeServidor(string? mensaje, string porDefecto)
            => string.IsNullOrWhiteSpace(mensaje) ? porDefecto : mensaje.Trim();

        public static bool EsTokenInvalido(string? mensaje)
            => mensaje?.Contains("token", StringComparison.OrdinalIgnoreCase) == true;
    }
}
