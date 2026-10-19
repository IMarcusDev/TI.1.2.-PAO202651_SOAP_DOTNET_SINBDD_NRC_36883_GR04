using ConverterServiceRef;
using LoginServiceRef;
using System;
using System.Collections.Generic;
using System.Text;

namespace CONSOLE_CON_UNI_DOTNET_SOAP.Models.HTTP
{
  internal class ConversorHTTP
  {
    private readonly ConverterWebServiceClient _client;

    public ConversorHTTP()
    {
      _client = new ConverterWebServiceClient();
    }

    public async Task<ConverterResponseDTO?> Convert(
      string token,
      string type,
      double value,
      string from,
      string to
    )
    {
      var response = await _client.ConvertAsync(token, type, value, from, to);

      return response.Body.ConvertResult;
    }
  }
}
