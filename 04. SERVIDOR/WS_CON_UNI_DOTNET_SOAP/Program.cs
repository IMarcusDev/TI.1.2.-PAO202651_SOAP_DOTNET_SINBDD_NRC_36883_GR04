using SoapCore;
using WS_CON_UNI_DOTNET_SOAP.WS.Interfaces;
using WS_CON_UNI_DOTNET_SOAP.WS.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSoapCore();

builder.Services.AddSingleton<ILoginWebService, LoginWebService>();

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
  _ = endpoints.UseSoapEndpoint<ILoginWebService>(
    "/LoginWebService.svc",
    new SoapEncoderOptions(),
    SoapSerializer.XmlSerializer
  );
});

app.Run();
