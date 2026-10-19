using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace MOV_CON_UNI_DOTNET_SOAP.Controllers
{
    public class LoginController(ILoginRepository loginRepository)
    {
        private readonly ILoginRepository _loginRepository = loginRepository;
        public async Task<LoginResult> LoginAsync(string? username,  string? password)
        {
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return new LoginResult(false, "Ingrese usuario o contraseña", null);
            }
            return await _loginRepository.LoginAsync(username, password);
        }
    }
}
