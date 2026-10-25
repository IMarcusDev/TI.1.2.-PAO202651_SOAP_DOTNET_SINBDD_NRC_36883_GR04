using MOV_CON_UNI_DOTNET_SOAP.Config;
using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Enums;
using MOV_CON_UNI_DOTNET_SOAP.Models.Interface;
using SoapConversion = MOV_CON_UNI_DOTNET_SOAP.Services.Soap.Conversion;

namespace MOV_CON_UNI_DOTNET_SOAP.Models.Repositories
{
    public class ConvertRepository : IConverterRepository
    {
        public async Task<ConvertResult> ConvertAsync(string token, Conversion conversion)
        {
            if (SoapErrores.SinRed())
                return new ConvertResult(false, SoapErrores.SinConexion, 0);

            SoapConversion.ConverterWebServiceClient? client = null;
            try
            {
                client = new SoapConversion.ConverterWebServiceClient(
                    SoapConversion.ConverterWebServiceClient.EndpointConfiguration.BasicHttpBinding_IConverterWebService_soap,
                    SoapConfig.BaseUrl + "ConverterWebService.svc"
                    );

                SoapConversion.ConvertResponse response = await client.ConvertAsync(
                    token,
                    conversion.Magnitude.CodigoServidor(),
                    conversion.Value,
                    conversion.From,
                    conversion.To);

                await client.CloseAsync();

                SoapConversion.ConverterResponseDTO? dto = response?.Body?.ConvertResult;
                if (dto == null)
                    return new ConvertResult(false, SoapErrores.RespuestaIncompleta, 0);

                if (dto.estado != SoapConversion.EstadosEnum.OK)
                    return new ConvertResult(
                        Success: false,
                        Message: SoapErrores.MensajeServidor(dto.mensaje, "No se pudo hacer la conversión"),
                        Value: 0,
                        SesionExpirada: SoapErrores.EsTokenInvalido(dto.mensaje)
                        );

                if (!double.IsFinite(dto.resultado))
                    return new ConvertResult(false, "El servidor devolvió un resultado no válido.", 0);

                return new ConvertResult(
                    Success: true,
                    Message: dto.mensaje ?? "",
                    Value: dto.resultado
                    );
            }
            catch (Exception ex)
            {
                client?.Abort();
                return new ConvertResult(false, SoapErrores.Mensaje(ex), 0);
            }
        }
    }
}
