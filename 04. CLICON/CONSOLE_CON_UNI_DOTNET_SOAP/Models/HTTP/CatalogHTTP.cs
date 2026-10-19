using CatalogServiceRef;
using ConverterServiceRef;
using System;
using System.Collections.Generic;
using System.Text;

namespace CONSOLE_CON_UNI_DOTNET_SOAP.Models.HTTP
{
  internal class CatalogHTTP
  {
    private readonly CatalogWebServiceClient _client;

    public CatalogHTTP()
    {
      _client = new CatalogWebServiceClient();
    }

    public async Task<CatalogResponseDTO?> GetCatalog(
      string token,
      string type
    )
    {
      var response = await _client.GetCatalogAsync(type, token);

      return response.Body.GetCatalogResult;
    }
  }
}
