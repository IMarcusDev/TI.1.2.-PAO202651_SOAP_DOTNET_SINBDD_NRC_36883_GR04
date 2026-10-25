using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Enums;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Interface
{
    public interface ICatalogRepository
    {
        Task<CatalogResult> GetCatalogAsync(string token, Magnitud magnitud);
    }
}
