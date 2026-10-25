using MOV_CON_UNI_DOTNET_SOAP.Config;
using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Interface;
using SoapLogin = MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Login;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            if (SoapErrores.SinRed())
                return new LoginResult(false, SoapErrores.SinConexion, null);

            SoapLogin.LoginWebServiceClient? client = null;
            try
            {
                client = new SoapLogin.LoginWebServiceClient(
                    SoapLogin.LoginWebServiceClient.EndpointConfiguration.BasicHttpBinding_ILoginWebService_soap,
                    SoapConfig.BaseUrl + "LoginWebService.svc"
                    );

                SoapLogin.LoginResponse response = await client.LoginAsync(username, password);
                await client.CloseAsync();

                SoapLogin.LoginResponseDTO? dto = response?.Body?.LoginResult;
                if (dto == null)
                    return new LoginResult(false, SoapErrores.RespuestaIncompleta, null);

                if (dto.estado != SoapLogin.EstadosEnum.OK)
                    return new LoginResult(false, SoapErrores.MensajeServidor(dto.mensaje, "Usuario o contraseña incorrectos"), null);

                if (string.IsNullOrWhiteSpace(dto.token))
                    return new LoginResult(false, SoapErrores.RespuestaIncompleta, null);

                return new LoginResult(
                    Success: true,
                    Message: dto.mensaje ?? "",
                    Token: dto.token
                    );
            }
            catch (Exception ex)
            {
                client?.Abort();
                return new LoginResult(false, SoapErrores.Mensaje(ex), null);
            }
        }
    }
}
