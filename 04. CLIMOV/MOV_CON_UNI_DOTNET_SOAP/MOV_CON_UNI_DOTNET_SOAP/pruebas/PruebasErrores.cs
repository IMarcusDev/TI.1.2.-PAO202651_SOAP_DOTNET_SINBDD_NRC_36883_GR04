using MOV_CON_UNI_DOTNET_SOAP.Config;
using MOV_CON_UNI_DOTNET_SOAP.Models.Repositories;
using System.Net.Sockets;
using System.ServiceModel;
using Java = Simulado.Java;
using SoapLogin = MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Login;

namespace MOV_CON_UNI_DOTNET_SOAP.Pruebas
{
    public static class PruebasErrores
    {
        private const string NoEncontrado = "No se encontró el servidor en http://192.168.1.2:5052/. Verifique que esté encendido y que la IP sea correcta.";
        private const string Timeout = "El servidor no respondió en 15 segundos. Intente de nuevo.";
        private const string SinComunicacion = "No se pudo comunicar con el servidor. Verifique que esté encendido y en la misma red.";
        private const string Inesperado = "Ocurrió un error inesperado al contactar al servidor.";

        public static Ejecutor Crear()
        {
            var e = new Ejecutor();

            (string Nombre, Exception Ex, string Mensaje)[] excepciones =
            [
                ("Servidor no encontrado", new EndpointNotFoundException("x"), NoEncontrado),
                ("Conexión rechazada (.NET)", new CommunicationException("x", new SocketException((int)SocketError.ConnectionRefused)), NoEncontrado),
                ("Host desconocido (.NET)", new CommunicationException("x", new HttpRequestException("x", new SocketException((int)SocketError.HostNotFound))), NoEncontrado),
                ("Host inalcanzable (.NET)", new CommunicationException("x", new SocketException((int)SocketError.HostUnreachable)), NoEncontrado),
                ("Error de resolución DNS (.NET)", new CommunicationException("x", new HttpRequestException(HttpRequestError.NameResolutionError, "x")), NoEncontrado),
                ("ConnectException (Android)", new CommunicationException("x", new HttpRequestException("x", new Java.ConnectException("refused"))), NoEncontrado),
                ("UnknownHostException (Android)", new CommunicationException("x", new Java.UnknownHostException("host")), NoEncontrado),
                ("NoRouteToHostException (Android)", new CommunicationException("x", new Java.NoRouteToHostException("route")), NoEncontrado),
                ("SocketTimeoutException (Android)", new CommunicationException("x", new Java.SocketTimeoutException("t")), Timeout),
                ("IOException de Java (cleartext bloqueado)", new CommunicationException("x", new Java.IOException("Cleartext HTTP traffic not permitted")), SinComunicacion),
                ("SOAP Fault", new FaultException("Object reference not set"), "El servidor no pudo procesar la solicitud: Object reference not set"),
                ("Respuesta HTTP inesperada", new ProtocolException("(400) Bad Request"), SinComunicacion),
                ("Comunicación interrumpida", new CommunicationException("x"), SinComunicacion),
                ("Tiempo agotado", new TimeoutException("x"), Timeout),
                ("URL mal formada", new UriFormatException("x"), "La dirección del servidor está mal configurada."),
                ("Cualquier otra excepción", new InvalidOperationException("x"), Inesperado),
            ];
            foreach (var (nombre, ex, mensaje) in excepciones)
                e.Caso($"Excepción: {nombre}", () => Afirmar.EmpiezaCon(mensaje, SoapErrores.Mensaje(ex)));

#if DEBUG
            e.Caso("Debug agrega la causa más interna", () =>
            {
                string m = SoapErrores.Mensaje(new CommunicationException("x", new InvalidOperationException("causa")));
                Afirmar.Verdadero(m.EndsWith("\n[Debug] InvalidOperationException: causa"), m);
            });
#endif

            e.Caso("«Token Inválido» → sesión expirada", () => Afirmar.Verdadero(SoapErrores.EsTokenInvalido("Token Inválido"), "debía detectarse"));
            e.Caso("«Tipo de Catálogo Inexistente» → NO es sesión", () =>
                Afirmar.Verdadero(!SoapErrores.EsTokenInvalido("Tipo de Catálogo Inexistente"), "no debía detectarse"));
            e.Caso("«No se pudo hacer la conversión» → NO es sesión", () =>
                Afirmar.Verdadero(!SoapErrores.EsTokenInvalido("No se pudo hacer la conversión"), "no debía detectarse"));
            e.Caso("«Credenciales Incorrectas» → NO es sesión", () =>
                Afirmar.Verdadero(!SoapErrores.EsTokenInvalido("Credenciales Incorrectas"), "no debía detectarse"));
            e.Caso("Mensaje nulo → NO es sesión", () => Afirmar.Verdadero(!SoapErrores.EsTokenInvalido(null), "no debía detectarse"));

            e.Caso("Mensaje vacío del servidor → mensaje por defecto", () =>
                Afirmar.Igual("Por defecto", SoapErrores.MensajeServidor("  ", "Por defecto")));
            e.Caso("Mensaje del servidor sin espacios sobrantes", () =>
                Afirmar.Igual("Credenciales Incorrectas", SoapErrores.MensajeServidor(" Credenciales Incorrectas ", "x")));

            e.Caso("Clientes WCF: 15 s para abrir, enviar y recibir; 5 s para cerrar", () =>
            {
                var cliente = new SoapLogin.LoginWebServiceClient(
                    SoapLogin.LoginWebServiceClient.EndpointConfiguration.BasicHttpBinding_ILoginWebService_soap,
                    SoapConfig.BaseUrl + "LoginWebService.svc");
                try
                {
                    var binding = cliente.Endpoint.Binding;
                    Afirmar.Igual(TimeSpan.FromSeconds(15), binding.OpenTimeout, "OpenTimeout");
                    Afirmar.Igual(TimeSpan.FromSeconds(15), binding.SendTimeout, "SendTimeout");
                    Afirmar.Igual(TimeSpan.FromSeconds(15), binding.ReceiveTimeout, "ReceiveTimeout");
                    Afirmar.Igual(TimeSpan.FromSeconds(5), binding.CloseTimeout, "CloseTimeout");
                }
                finally
                {
                    cliente.Abort();
                }
            });

            return e;
        }
    }
}
