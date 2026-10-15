using WS_CON_UNI_DOTNET_SOAP.Controllers;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;
using WS_CON_UNI_DOTNET_SOAP.Models.Enums;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Services
{
  public class CatalogWebService : ICatalogWebService
  {
    private static string FormatType(string text)
    {
      if (string.IsNullOrEmpty(text))
        return text;

      return char.ToUpper(text[0]) + text.Substring(1).ToLower();
    }

    public CatalogResponseDTO GetCatalog(string type, string token)
    {
      type = FormatType(type);

      if (!LoginController.validateUser(token))
      {
        return new CatalogResponseDTO(
            EstadosEnum.ERROR,
            "Token Inválido",
            new HashSet<string>()
        );
      }

      try
      {
        return new CatalogResponseDTO(
            EstadosEnum.OK,
            "Obtención de Catálogo Exitosa",
            ConverterController.GetConverter(type).GetUnits().ToHashSet()
        );
      }
      catch (Exception)
      {
        return new CatalogResponseDTO(
            EstadosEnum.ERROR,
            "Tipo de Catálogo Inexistente",
            new HashSet<string>()
        );
      }
    }
  }
}
