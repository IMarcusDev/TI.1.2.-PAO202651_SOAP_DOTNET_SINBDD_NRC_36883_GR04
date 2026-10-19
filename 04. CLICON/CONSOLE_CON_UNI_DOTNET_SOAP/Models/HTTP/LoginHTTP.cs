using LoginServiceRef;
using System;
using System.Collections.Generic;
using System.Text;

namespace CONSOLE_CON_UNI_DOTNET_SOAP.Models.HTTP
{
  internal class LoginHTTP
  {
    private readonly LoginWebServiceClient _client;

    public LoginHTTP()
    {
      _client = new LoginWebServiceClient();
    }

    public async Task<LoginResponseDTO?> LoginAsync(
      string username,
      string password
    )
    {
      var response = await _client.LoginAsync(username, password);

      return response.Body.LoginResult;
    }
  }
}
