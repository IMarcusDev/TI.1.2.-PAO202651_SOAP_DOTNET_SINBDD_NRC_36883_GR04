using System.ServiceModel;
using ec.edu.monster.controlador;
using ec.edu.monster.modelo;
using ec.edu.monster.ws.catalogo;
using ec.edu.monster.ws.conversor;
using ec.edu.monster.ws.login;

namespace ec.edu.monster.pruebas
{
  /// <summary>
  /// Servidor SOAP en memoria con las mismas reglas que el real: usuario "user",
  /// clave "1234" y token obligatorio. Puede simular caídas y faults.
  /// </summary>
  public class ServidorFalso : ILoginWebService, ICatalogWebService, IConverterWebService
  {
    public const string Token = "token-prueba";

    public bool Caido { get; set; }
    public string? Fault { get; set; }
    public bool SinRespuesta { get; set; }
    public (string Token, string Tipo, double Valor, string Desde, string Hacia)? UltimaConversion { get; private set; }

    private static readonly Dictionary<string, string[]> Catalogo = new()
    {
      ["Longitud"] = ["m", "cm", "ft"],
      ["Masa"] = ["g", "kg"],
      ["Volumen"] = ["mL", "L"],
      ["Tiempo"] = ["s", "min"],
      ["Temperatura"] = ["°C", "°F"],
      ["Area"] = ["m2", "ha"],
    };

    public Task<LoginResponse> LoginAsync(LoginRequest request)
    {
      Simular();
      var dto = request.Body.username == "user" && request.Body.password == "1234"
        ? new LoginResponseDTO { estado = ws.login.EstadosEnum.OK, mensaje = "Ingreso de Sesión Exitoso!", token = Token }
        : new LoginResponseDTO { estado = ws.login.EstadosEnum.ERROR, mensaje = "Credenciales Incorrectas", token = "" };

      return Task.FromResult(new LoginResponse(new LoginResponseBody(SinRespuesta ? null! : dto)));
    }

    public Task<GetCatalogResponse> GetCatalogAsync(GetCatalogRequest request)
    {
      Simular();
      var dto = new CatalogResponseDTO();

      if (request.Body.token != Token)
      {
        dto.estado = ws.catalogo.EstadosEnum.ERROR;
        dto.mensaje = "Token Inválido";
      }
      else if (Catalogo.TryGetValue(request.Body.type, out var unidades))
      {
        dto.estado = ws.catalogo.EstadosEnum.OK;
        dto.mensaje = "Obtención de Catálogo Exitosa";
        dto.resultado = [.. unidades];
      }
      else
      {
        dto.estado = ws.catalogo.EstadosEnum.ERROR;
        dto.mensaje = "Tipo de Catálogo Inexistente";
      }

      return Task.FromResult(new GetCatalogResponse(new GetCatalogResponseBody(SinRespuesta ? null! : dto)));
    }

    public Task<ConvertResponse> ConvertAsync(ConvertRequest request)
    {
      Simular();
      var b = request.Body;
      UltimaConversion = (b.token, b.type, b.value, b.from, b.to);

      var dto = b.token == Token
        ? new ConverterResponseDTO { estado = ws.conversor.EstadosEnum.OK, mensaje = "Conversión Exitosa", resultado = b.value * 100 }
        : new ConverterResponseDTO { estado = ws.conversor.EstadosEnum.ERROR, mensaje = "Token Inválido" };

      return Task.FromResult(new ConvertResponse(new ConvertResponseBody(SinRespuesta ? null! : dto)));
    }

    private void Simular()
    {
      if (Caido)
        throw new EndpointNotFoundException("Servidor caído");
      if (Fault != null)
        throw new FaultException(Fault);
    }
  }

  /// <summary>Fábrica que conecta la aplicación con el servidor en memoria.</summary>
  public class FabricaFalsa(ServidorFalso servidor) : IFabricaServicios
  {
    public ConversorServicio CrearServicio(Sesion sesion) =>
      new(servidor, servidor, servidor, sesion);

    public Task<bool> DisponibleAsync() => Task.FromResult(!servidor.Caido);
  }
}
