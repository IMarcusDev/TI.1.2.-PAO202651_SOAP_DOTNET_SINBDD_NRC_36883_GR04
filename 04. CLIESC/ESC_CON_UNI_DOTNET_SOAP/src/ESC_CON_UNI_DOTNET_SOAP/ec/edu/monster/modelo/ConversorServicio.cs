using System.ServiceModel;
using ec.edu.monster.ws.catalogo;
using ec.edu.monster.ws.conversor;
using ec.edu.monster.ws.login;

namespace ec.edu.monster.modelo
{
  /// <summary>
  /// Único punto de acceso al servidor SOAP. Los controladores usan esta clase,
  /// nunca los clientes generados por dotnet-svcutil.
  /// </summary>
  public class ConversorServicio
  {
    public const string SinConexion = "No se pudo conectar con el servidor";
    public const string SinSesion = "La sesión no está activa";
    public const string SinRespuesta = "El servidor no devolvió una respuesta";
    public const string SinToken = "El servidor no devolvió un token de sesión";
    public const string ErrorGenerico = "El servidor respondió con un error";

    private readonly ILoginWebService login;
    private readonly ICatalogWebService catalogo;
    private readonly IConverterWebService conversor;
    private readonly Sesion sesion;

    public ConversorServicio(
      ILoginWebService login,
      ICatalogWebService catalogo,
      IConverterWebService conversor,
      Sesion sesion)
    {
      this.login = login;
      this.catalogo = catalogo;
      this.conversor = conversor;
      this.sesion = sesion;
    }

    public async Task IniciarSesionAsync(string usuario, string clave)
    {
      // Un nuevo intento de ingreso reemplaza la sesión anterior, aunque falle.
      sesion.Cerrar();

      LoginResponse? respuesta = await LlamarAsync(() =>
        login.LoginAsync(new LoginRequest(new LoginRequestBody(usuario, clave))));
      LoginResponseDTO? dto = respuesta?.Body?.LoginResult;

      if (dto == null)
        throw new ServicioException(SinRespuesta);
      if (dto.estado != ws.login.EstadosEnum.OK)
        throw new ServicioException(MensajeOGenerico(dto.mensaje));
      if (string.IsNullOrWhiteSpace(dto.token))
        throw new ServicioException(SinToken);

      sesion.Iniciar(usuario, dto.token);
    }

    public async Task<double> ConvertirAsync(string magnitud, double valor, string desde, string hacia)
    {
      string token = TokenActivo();

      ConvertResponse? respuesta = await LlamarAsync(() =>
        conversor.ConvertAsync(new ConvertRequest(
          new ConvertRequestBody(token, magnitud, valor, desde, hacia))));
      ConverterResponseDTO? dto = respuesta?.Body?.ConvertResult;

      if (dto == null)
        throw new ServicioException(SinRespuesta);
      if (dto.estado != ws.conversor.EstadosEnum.OK)
        throw new ServicioException(MensajeOGenerico(dto.mensaje));

      return dto.resultado;
    }

    public async Task<IReadOnlyList<string>> CatalogoAsync(string magnitud)
    {
      string token = TokenActivo();

      GetCatalogResponse? respuesta = await LlamarAsync(() =>
        catalogo.GetCatalogAsync(new GetCatalogRequest(new GetCatalogRequestBody(magnitud, token))));
      CatalogResponseDTO? dto = respuesta?.Body?.GetCatalogResult;

      if (dto == null)
        throw new ServicioException(SinRespuesta);
      if (dto.estado != ws.catalogo.EstadosEnum.OK)
        throw new ServicioException(MensajeOGenerico(dto.mensaje));

      return dto.resultado?.ToList() ?? [];
    }

    public void CerrarSesion()
    {
      sesion.Cerrar();
    }

    private string TokenActivo()
    {
      if (!sesion.EstaActiva())
        throw new ServicioException(SinSesion);

      return sesion.Token!;
    }

    private static async Task<T> LlamarAsync<T>(Func<Task<T>> llamada)
    {
      try
      {
        return await llamada();
      }
      catch (FaultException e)
      {
        // El servidor respondió con un fault: se muestra su mensaje, no un error de conexión.
        throw new ServicioException(MensajeOGenerico(e.Message), e);
      }
      catch (CommunicationException e)
      {
        throw new ServicioException(SinConexion, e);
      }
      catch (TimeoutException e)
      {
        throw new ServicioException(SinConexion, e);
      }
    }

    private static string MensajeOGenerico(string? mensaje)
    {
      return string.IsNullOrWhiteSpace(mensaje) ? ErrorGenerico : mensaje;
    }
  }
}
