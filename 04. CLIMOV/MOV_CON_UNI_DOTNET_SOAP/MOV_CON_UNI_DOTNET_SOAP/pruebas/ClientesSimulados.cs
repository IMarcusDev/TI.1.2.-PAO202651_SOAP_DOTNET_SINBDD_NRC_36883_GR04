using System.ServiceModel.Description;
using MOV_CON_UNI_DOTNET_SOAP.Config;
using MOV_CON_UNI_DOTNET_SOAP.Pruebas;

namespace MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Login
{
    public partial class LoginWebServiceClient
    {
        static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
        {
            SoapEndpoints.AplicarTimeouts(serviceEndpoint);
            RedSoapSimulada.Configurar(serviceEndpoint);
        }
    }
}

namespace MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Catalog
{
    public partial class CatalogWebServiceClient
    {
        static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
        {
            SoapEndpoints.AplicarTimeouts(serviceEndpoint);
            RedSoapSimulada.Configurar(serviceEndpoint);
        }
    }
}

namespace MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Conversion
{
    public partial class ConverterWebServiceClient
    {
        static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
        {
            SoapEndpoints.AplicarTimeouts(serviceEndpoint);
            RedSoapSimulada.Configurar(serviceEndpoint);
        }
    }
}
