using System.ServiceModel.Description;
using Binding = System.ServiceModel.Channels.Binding;
using MOV_CON_UNI_DOTNET_SOAP.Config;

namespace MOV_CON_UNI_DOTNET_SOAP.Config
{
    internal static class SoapEndpoints
    {
        public static void AplicarTimeouts(ServiceEndpoint endpoint)
        {
            Binding binding = endpoint.Binding;
            binding.OpenTimeout = SoapConfig.Timeout;
            binding.SendTimeout = SoapConfig.Timeout;
            binding.ReceiveTimeout = SoapConfig.Timeout;
            binding.CloseTimeout = SoapConfig.CloseTimeout;
        }
    }
}

namespace MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Login
{
    public partial class LoginWebServiceClient
    {
        static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
            => SoapEndpoints.AplicarTimeouts(serviceEndpoint);
    }
}

namespace MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Catalog
{
    public partial class CatalogWebServiceClient
    {
        static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
            => SoapEndpoints.AplicarTimeouts(serviceEndpoint);
    }
}

namespace MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Conversion
{
    public partial class ConverterWebServiceClient
    {
        static partial void ConfigureEndpoint(ServiceEndpoint serviceEndpoint, ClientCredentials clientCredentials)
            => SoapEndpoints.AplicarTimeouts(serviceEndpoint);
    }
}
