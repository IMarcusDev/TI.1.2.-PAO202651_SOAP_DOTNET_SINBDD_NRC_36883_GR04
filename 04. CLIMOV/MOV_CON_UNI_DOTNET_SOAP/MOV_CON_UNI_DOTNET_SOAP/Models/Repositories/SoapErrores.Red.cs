namespace MOV_CON_UNI_DOTNET_SOAP.Models.Repositories
{
    internal static partial class SoapErrores
    {
        public static bool SinRed()
        {
            try
            {
                return Connectivity.Current.NetworkAccess == NetworkAccess.None;
            }
            catch
            {
                return false;
            }
        }
    }
}
