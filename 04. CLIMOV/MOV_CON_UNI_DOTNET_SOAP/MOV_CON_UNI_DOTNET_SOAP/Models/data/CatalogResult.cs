namespace MOV_CON_UNI_DOTNET_SOAP.Models.data
{
    public record CatalogResult(bool Success, string Message, IReadOnlyList<string> Unidades, bool SesionExpirada = false);
}
