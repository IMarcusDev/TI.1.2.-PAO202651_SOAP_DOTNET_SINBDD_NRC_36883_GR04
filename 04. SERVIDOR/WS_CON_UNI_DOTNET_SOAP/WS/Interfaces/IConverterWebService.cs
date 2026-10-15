using System.ServiceModel;
using WS_CON_UNI_DOTNET_SOAP.Models.DTO;

namespace WS_CON_UNI_DOTNET_SOAP.WS.Interfaces
{
  [ServiceContract]
  public interface IConverterWebService
  {
    [OperationContract]
    ConverterResponseDTO Convert(
      string token,
      string type,
      double value,
      string from,
      string to
    );
  }
}
