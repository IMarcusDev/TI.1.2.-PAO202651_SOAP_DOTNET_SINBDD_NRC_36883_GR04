using MOV_CON_UNI_DOTNET_SOAP.Config;
using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Enums;
using MOV_CON_UNI_DOTNET_SOAP.Models.Interface;
using SoapCatalog = MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Catalog;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Repositories
{
    public class CatalogRepository : ICatalogRepository
    {
        public async Task<CatalogResult> GetCatalogAsync(string token, Magnitud magnitud)
        {
            if (SoapErrores.SinRed())
                return new CatalogResult(false, SoapErrores.SinConexion, []);

            SoapCatalog.CatalogWebServiceClient? client = null;
            try
            {
                client = new SoapCatalog.CatalogWebServiceClient(
                    SoapCatalog.CatalogWebServiceClient.EndpointConfiguration.BasicHttpBinding_ICatalogWebService_soap,
                    SoapConfig.BaseUrl + "CatalogWebService.svc"
                    );

                SoapCatalog.GetCatalogResponse response = await client.GetCatalogAsync(magnitud.CodigoServidor(), token);
                await client.CloseAsync();

                SoapCatalog.CatalogResponseDTO? dto = response?.Body?.GetCatalogResult;
                if (dto == null)
                    return new CatalogResult(false, SoapErrores.RespuestaIncompleta, []);

                if (dto.estado != SoapCatalog.EstadosEnum.OK)
                    return new CatalogResult(
                        Success: false,
                        Message: SoapErrores.MensajeServidor(dto.mensaje, "No se pudo obtener el catálogo"),
                        Unidades: [],
                        SesionExpirada: SoapErrores.EsTokenInvalido(dto.mensaje)
                        );

                List<string> unidades = (dto.resultado ?? [])
                    .Where(u => !string.IsNullOrWhiteSpace(u))
                    .Select(u => u.Trim())
                    .Distinct()
                    .ToList();

                if (unidades.Count == 0)
                    return new CatalogResult(false, $"El servidor no devolvió unidades para {magnitud.Nombre()}.", []);

                return new CatalogResult(true, dto.mensaje ?? "", unidades);
            }
            catch (Exception ex)
            {
                client?.Abort();
                return new CatalogResult(false, SoapErrores.Mensaje(ex), []);
            }
        }
    }
}
