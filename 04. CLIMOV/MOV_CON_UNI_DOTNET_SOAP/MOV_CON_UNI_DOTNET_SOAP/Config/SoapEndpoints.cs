using System.ServiceModel.Description;
using Binding = System.ServiceModel.Channels.Binding;

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
