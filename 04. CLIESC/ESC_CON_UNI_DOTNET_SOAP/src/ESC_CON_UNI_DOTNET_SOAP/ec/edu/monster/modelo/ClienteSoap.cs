using System.ServiceModel;
using ec.edu.monster.ws.catalogo;
using ec.edu.monster.ws.conversor;
using ec.edu.monster.ws.login;

namespace ec.edu.monster.modelo
{
  /// <summary>
  /// Conecta el modelo con el servidor SOAP real. Las ChannelFactory se crean una
  /// sola vez; cada llamada abre y cierra su propio canal.
  /// </summary>
  public static class ClienteSoap
  {
    private static readonly TimeSpan TiempoConexion = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan TiempoPeticion = TimeSpan.FromSeconds(15);

    /// <summary>Crea el servicio de la aplicación, ligado a su sesión.</summary>
    public static ConversorServicio CrearServicio(string urlBase, Sesion sesion)
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

      return new ConversorServicio(
        new CanalLogin(new ChannelFactory<ILoginWebService>(
          binding, new EndpointAddress(urlBase + "/LoginWebService.svc"))),
        new CanalCatalogo(new ChannelFactory<ICatalogWebService>(
          binding, new EndpointAddress(urlBase + "/CatalogWebService.svc"))),
        new CanalConversor(new ChannelFactory<IConverterWebService>(
          binding, new EndpointAddress(urlBase + "/ConverterWebService.svc"))),
        sesion);
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
