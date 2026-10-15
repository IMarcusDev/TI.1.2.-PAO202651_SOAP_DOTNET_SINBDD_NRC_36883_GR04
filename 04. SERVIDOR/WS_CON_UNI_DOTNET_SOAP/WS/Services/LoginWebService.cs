using WS_CON_UNI_DOTNET_SOAP.Controllers;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;
using WS_CON_UNI_DOTNET_SOAP.Models.Enums;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Services
{
  public class LoginWebService : ILoginWebService
  {
    public LoginResponseDTO Login(string username, string password)
    {
      LoginResponseDTO dto;

      try
      {
        string token = LoginController.login(username, password);

        dto = new LoginResponseDTO(
           EstadosEnum.OK,
           "Ingreso de Sesión Exitoso!",
           token
         );
      }
      catch (Exception e)
      {
        dto = new LoginResponseDTO(
          EstadosEnum.ERROR,
          "Credenciales Incorrectas",
          ""
        );
      }

      return dto;
    }
  }
}
