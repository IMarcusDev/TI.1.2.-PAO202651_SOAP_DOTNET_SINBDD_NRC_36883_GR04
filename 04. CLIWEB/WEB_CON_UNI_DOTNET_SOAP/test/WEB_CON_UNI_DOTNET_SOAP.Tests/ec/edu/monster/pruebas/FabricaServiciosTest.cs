using ec.edu.monster.controlador;
using ec.edu.monster.modelo;

namespace ec.edu.monster.pruebas
{
  public class FabricaServiciosTest
  {
    // Puerto 9 (discard): nadie escucha ahí, así que la conexión falla enseguida.
    private const string SinServidor = "http://127.0.0.1:9";

    [Fact]
    public async Task SinServidorNoEstaDisponible()
    {
      Assert.False(await new FabricaServicios(SinServidor).DisponibleAsync());
    }

    [Fact]
    public async Task SinServidorElLoginEsErrorDeConexion()
    {
      var servicio = new FabricaServicios(SinServidor).CrearServicio(new Sesion());

      var error = await Assert.ThrowsAsync<ServicioException>(() => servicio.IniciarSesionAsync("user", "1234"));

      Assert.Equal(ConversorServicio.SinConexion, error.Message);
    }
  }
}
