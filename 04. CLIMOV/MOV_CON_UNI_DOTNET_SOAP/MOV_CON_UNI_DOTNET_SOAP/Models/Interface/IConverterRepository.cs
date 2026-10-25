using MOV_CON_UNI_DOTNET_SOAP.Models.data;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Interface
{
    public interface IConverterRepository
    {
        Task<ConvertResult> ConvertAsync(string token, Conversion conversion);
    }
}
