namespace MOV_CON_UNI_DOTNET_SOAP.Config
{
    public class SoapConfig
    {
        public const string BaseUrl = "http://192.168.1.2:5052/";

        public static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);
        public static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(5);
    }
}
