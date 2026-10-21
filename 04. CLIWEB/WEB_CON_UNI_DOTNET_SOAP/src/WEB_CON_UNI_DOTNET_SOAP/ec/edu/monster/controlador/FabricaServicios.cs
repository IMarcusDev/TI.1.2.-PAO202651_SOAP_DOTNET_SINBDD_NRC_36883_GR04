using System.ServiceModel;
using ec.edu.monster.modelo;
using ec.edu.monster.ws.catalogo;
using ec.edu.monster.ws.conversor;
using ec.edu.monster.ws.login;

namespace ec.edu.monster.controlador
{
  public interface IFabricaServicios
  {
    /// <summary>Crea el servicio de un usuario, ligado a su sesión.</summary>
    ConversorServicio CrearServicio(Sesion sesion);

    /// <summary>True cuando el servidor responde su WSDL en este momento.</summary>
    Task<bool> DisponibleAsync();
  }

  /// <summary>
  /// Fábrica única de la aplicación. Las ChannelFactory son costosas, así que se
  /// crean una sola vez; cada llamada abre y cierra su propio canal.
  /// </summary>
  public class FabricaServicios : IFabricaServicios
  {
    private static readonly TimeSpan TiempoConexion = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan TiempoPeticion = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan TiempoDisponible = TimeSpan.FromSeconds(2);

    private readonly ChannelFactory<ILoginWebService> login;
    private readonly ChannelFactory<ICatalogWebService> catalogo;
    private readonly ChannelFactory<IConverterWebService> conversor;
    private readonly Uri wsdl;
    private readonly HttpClient http = new() { Timeout = TiempoDisponible };

    public FabricaServicios(string urlBase)
    {
      urlBase = urlBase.TrimEnd('/');

      // El servidor lo comparten varios clientes y puede trabarse: mejor fallar que esperar sin fin.
      var binding = new BasicHttpBinding
      {
        OpenTimeout = TiempoConexion,
        SendTimeout = TiempoPeticion,
        ReceiveTimeout = TiempoPeticion,
        CloseTimeout = TiempoConexion,
        MaxReceivedMessageSize = int.MaxValue,
      };

      login = new ChannelFactory<ILoginWebService>(
        binding, new EndpointAddress(urlBase + "/LoginWebService.svc"));
      catalogo = new ChannelFactory<ICatalogWebService>(
        binding, new EndpointAddress(urlBase + "/CatalogWebService.svc"));
      conversor = new ChannelFactory<IConverterWebService>(
        binding, new EndpointAddress(urlBase + "/ConverterWebService.svc"));
      wsdl = new Uri(urlBase + "/LoginWebService.svc?wsdl");
    }

    public ConversorServicio CrearServicio(Sesion sesion)
    {
      return new ConversorServicio(
        new CanalLogin(login),
        new CanalCatalogo(catalogo),
        new CanalConversor(conversor),
        sesion);
    }

    public async Task<bool> DisponibleAsync()
    {
      // Se pregunta cada vez: el servidor puede caerse en cualquier momento.
      try
      {
        using var respuesta = await http.GetAsync(wsdl, HttpCompletionOption.ResponseHeadersRead);
        return respuesta.IsSuccessStatusCode;
      }
      catch (HttpRequestException)
      {
        return false;
      }
      catch (TaskCanceledException)
      {
        return false;
      }
    }

    // Abre un canal, hace una sola llamada y lo cierra; si falla, lo aborta.
    private static async Task<R> UsarCanalAsync<T, R>(ChannelFactory<T> fabrica, Func<T, Task<R>> llamada)
    {
      T canal = fabrica.CreateChannel();
      var comunicacion = (ICommunicationObject)canal!;
      try
      {
        R resultado = await llamada(canal);
        comunicacion.Close();
        return resultado;
      }
      catch
      {
        comunicacion.Abort();
        throw;
      }
    }

    private sealed class CanalLogin(ChannelFactory<ILoginWebService> fabrica) : ILoginWebService
    {
      public Task<LoginResponse> LoginAsync(LoginRequest request) =>
        UsarCanalAsync(fabrica, c => c.LoginAsync(request));
    }

    private sealed class CanalCatalogo(ChannelFactory<ICatalogWebService> fabrica) : ICatalogWebService
    {
      public Task<GetCatalogResponse> GetCatalogAsync(GetCatalogRequest request) =>
        UsarCanalAsync(fabrica, c => c.GetCatalogAsync(request));
    }

    private sealed class CanalConversor(ChannelFactory<IConverterWebService> fabrica) : IConverterWebService
    {
      public Task<ConvertResponse> ConvertAsync(ConvertRequest request) =>
        UsarCanalAsync(fabrica, c => c.ConvertAsync(request));
    }
  }
}
