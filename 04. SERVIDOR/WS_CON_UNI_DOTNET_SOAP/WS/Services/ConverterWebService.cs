using WS_CON_UNI_DOTNET_SOAP.Controllers;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;
using WS_CON_UNI_DOTNET_SOAP.Models.Enums;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Services
{
  public class ConverterWebService : IConverterWebService
  {
    public ConverterResponseDTO Convert(string token, string type, double value, string from, string to)
    {
      if (!LoginController.validateUser(token))
      {
        return new ConverterResponseDTO(
            EstadosEnum.ERROR,
            "Token Inválido",
            0
        );
      }

      try
      {
        return new ConverterResponseDTO(
            EstadosEnum.OK,
            "Conversión Exitosa",
            ConverterController.Convert(type, value, from, to)
        );
      }
      catch (Exception)
      {
        return new ConverterResponseDTO(
            EstadosEnum.ERROR,
            "No se pudo hacer la conversión",
            0
        );
      }
    }
  }
}
