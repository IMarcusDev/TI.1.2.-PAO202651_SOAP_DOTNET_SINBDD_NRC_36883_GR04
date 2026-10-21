namespace ec.edu.monster.modelo
{
  public interface IEstadoServidor
  {
    /// <summary>True cuando el servidor responde su WSDL en este momento.</summary>
    Task<bool> DisponibleAsync();
  }

  /// <summary>
  /// Pregunta al servidor SOAP si está arriba pidiendo su WSDL. Se consulta cada
  /// vez, porque el servidor puede caerse en cualquier momento.
  /// </summary>
  public class EstadoServidor : IEstadoServidor
  {
    private readonly Uri wsdl;
    private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(2) };

    public EstadoServidor(string urlBase)
    {
      wsdl = new Uri(urlBase.TrimEnd('/') + "/LoginWebService.svc?wsdl");
    }

    public async Task<bool> DisponibleAsync()
    {
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
  }
}
