using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml.Linq;

namespace MOV_CON_UNI_DOTNET_SOAP.Pruebas
{
    public class ManejadorSimulado(Func<ManejadorSimulado.Peticion, CancellationToken, Task<HttpResponseMessage>> responder)
    {
        public record Peticion(string Ruta, string? Accion, string Cuerpo)
        {
            public string? Valor(string elemento)
                => XDocument.Parse(Cuerpo).Descendants().FirstOrDefault(x => x.Name.LocalName == elemento)?.Value;
        }

        private readonly List<Peticion> _peticiones = [];

        public IReadOnlyList<Peticion> Peticiones
        {
            get
            {
                lock (_peticiones)
                    return [.. _peticiones];
            }
        }

        public async Task<HttpResponseMessage> EnviarAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
        {
            string cuerpo = peticion.Content is null ? "" : await peticion.Content.ReadAsStringAsync(cancelacion);
            string? accion = peticion.Headers.TryGetValues("SOAPAction", out var valores) ? valores.FirstOrDefault()?.Trim('"') : null;
            var registro = new Peticion(peticion.RequestUri!.AbsolutePath, accion, cuerpo);
            lock (_peticiones)
                _peticiones.Add(registro);
            HttpResponseMessage respuesta = await responder(registro, cancelacion);
            respuesta.RequestMessage ??= peticion;
            return respuesta;
        }

        public static ManejadorSimulado Responde(int codigo, string xml)
            => new((_, _) => Task.FromResult(Respuesta(codigo, xml)));

        public static ManejadorSimulado Segun(Func<Peticion, HttpResponseMessage> elegir)
            => new((p, _) => Task.FromResult(elegir(p)));

        public static ManejadorSimulado Lanza(Exception ex)
            => new((_, _) => Task.FromException<HttpResponseMessage>(ex));

        public static ManejadorSimulado Demora(TimeSpan espera, string xml)
            => new(async (_, cancelacion) =>
            {
                await Task.Delay(espera, cancelacion);
                return Respuesta(200, xml);
            });

        public static HttpResponseMessage Respuesta(int codigo, string xml)
            => new((HttpStatusCode)codigo) { Content = new StringContent(xml, Encoding.UTF8, "text/xml") };

        public static HttpResponseMessage SinCuerpo(int codigo)
            => new((HttpStatusCode)codigo) { Content = new ByteArrayContent([]) };

        public static string Sobre(string operacion, string contenido)
            => $"""<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"><s:Body><{operacion}Response xmlns="http://tempuri.org/"><{operacion}Result>{contenido}</{operacion}Result></{operacion}Response></s:Body></s:Envelope>""";

        public static string SobreSinResultado(string operacion)
            => $"""<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><{operacion}Response xmlns="http://tempuri.org/" /></s:Body></s:Envelope>""";

        public static string Falla(string codigo, string mensaje)
            => $"""<s:Envelope xmlns:s="http://schemas.xmlsoap.org/soap/envelope/"><s:Body><s:Fault><faultcode>{codigo}</faultcode><faultstring>{mensaje}</faultstring></s:Fault></s:Body></s:Envelope>""";
    }

    public static class RedSoapSimulada
    {
        private static ManejadorSimulado? _manejador;
        private static string? _urlBase;
        private static TimeSpan? _espera;

        public static IDisposable Usar(ManejadorSimulado manejador, TimeSpan? espera = null)
        {
            _manejador = manejador;
            _espera = espera;
            return new Restaurar();
        }

        public static IDisposable Servidor(string urlBase)
        {
            _urlBase = urlBase;
            return new Restaurar();
        }

        internal static void Configurar(ServiceEndpoint endpoint)
        {
            if (_urlBase is string url)
            {
                string servicio = endpoint.Address.Uri.Segments[^1];
                endpoint.Address = new EndpointAddress(url.TrimEnd('/') + "/" + servicio);
            }
            if (_espera is TimeSpan espera)
            {
                endpoint.Binding.OpenTimeout = espera;
                endpoint.Binding.SendTimeout = espera;
                endpoint.Binding.ReceiveTimeout = espera;
            }
            if (_manejador is ManejadorSimulado manejador)
                endpoint.EndpointBehaviors.Add(new Inyector(manejador));
        }

        private sealed class Restaurar : IDisposable
        {
            public void Dispose()
            {
                _manejador = null;
                _urlBase = null;
                _espera = null;
            }
        }

        private sealed class Inyector(ManejadorSimulado manejador) : IEndpointBehavior
        {
            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parametros)
                => parametros.Add(new Func<HttpClientHandler, HttpMessageHandler>(_ => new Puente(manejador)));

            public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime cliente) { }

            public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher despachador) { }

            public void Validate(ServiceEndpoint endpoint) { }
        }

        private sealed class Puente(ManejadorSimulado manejador) : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken cancelacion)
                => manejador.EnviarAsync(peticion, cancelacion);
        }
    }
}

namespace Simulado.Java
{
    public class ConnectException(string m) : Exception(m);
    public class UnknownHostException(string m) : Exception(m);
    public class NoRouteToHostException(string m) : Exception(m);
    public class SocketTimeoutException(string m) : Exception(m);
    public class IOException(string m) : Exception(m);
}
