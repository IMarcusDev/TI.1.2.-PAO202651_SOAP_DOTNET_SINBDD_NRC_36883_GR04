using Microsoft.Extensions.Logging;
using MOV_CON_UNI_DOTNET_SOAP.Controllers;
using MOV_CON_UNI_DOTNET_SOAP.Models.Interface;
using MOV_CON_UNI_DOTNET_SOAP.Models.Repositories;
using MOV_CON_UNI_DOTNET_SOAP.Views;

namespace MOV_CON_UNI_DOTNET_SOAP
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Repositorios
            builder.Services.AddSingleton<ILoginRepository, LoginRepository>();
            //builder.Services.AddSingleton<ICatalogRepository, CatalogRepository>();
            //builder.Services.AddSingleton<IConversionRepository, ConversionRepository>();

            // Controladores
            builder.Services.AddTransient<LoginController>();
            //builder.Services.AddTransient<CatalogController>();
            //builder.Services.AddTransient<ConversionController>();

            // Vistas
            builder.Services.AddTransient<LoginPage>();
            //builder.Services.AddTransient<HomePage>();
            //builder.Services.AddTransient<CatalogPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif


            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SinSubrayado", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });
            return builder.Build();
        }
    }
}
