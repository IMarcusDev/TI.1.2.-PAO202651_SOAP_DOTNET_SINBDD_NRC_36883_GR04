using ec.edu.monster.modelo;

namespace ec.edu.monster.pruebas
{
  /// <summary>
  /// Conexión real (ClienteSoap y EstadoServidor) contra una dirección donde no
  /// hay nadie escuchando.
  /// </summary>
  public class ConexionServidorTest
  {
    // Puerto 9 (discard): nadie escucha ahí, así que la conexión falla enseguida.
    private const string SinServidor = "http://127.0.0.1:9";

    [Fact]
    public async Task SinServidorNoEstaDisponible()
    {
      Assert.False(await new EstadoServidor(SinServidor).DisponibleAsync());
    }

    [Fact]
    public async Task SinServidorElLoginEsErrorDeConexion()
    {
      var servicio = ClienteSoap.CrearServicio(SinServidor, new Sesion());

      var error = await Assert.ThrowsAsync<ServicioException>(() => servicio.IniciarSesionAsync("user", "1234"));

      Assert.Equal(ConversorServicio.SinConexion, error.Message);
    }
  }
}
