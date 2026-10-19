using MOV_CON_UNI_DOTNET_SOAP.Config;
using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Interface;
using System.ServiceModel;
using SoapLogin = MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Login;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Repositories
{
    public class LoginRepository : ILoginRepository
    {
        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var client = new SoapLogin.LoginWebServiceClient(
                SoapLogin.LoginWebServiceClient.EndpointConfiguration.BasicHttpBinding_ILoginWebService_soap,
                SoapConfig.BaseUrl + "/LoginWebService.svc"
                );
            
            try
            {
                SoapLogin.LoginResponse response = await client.LoginAsync(username, password);
                await client.CloseAsync();

                SoapLogin.LoginResponseDTO dto = response.Body.LoginResult;

                return new LoginResult(
                    Success: dto.estado == SoapLogin.EstadosEnum.OK,
                    Message: dto.mensaje ?? "",
                    Token: dto.token
                    );
            }
            catch(Exception ex ) when (ex is CommunicationException or TimeoutException)
            {
                client.Abort();
                return new LoginResult(false, "No se pudo conectar con el servidor", null);
            }
            catch
            {
                client.Abort();
                throw;
            }
        }
    }
}
