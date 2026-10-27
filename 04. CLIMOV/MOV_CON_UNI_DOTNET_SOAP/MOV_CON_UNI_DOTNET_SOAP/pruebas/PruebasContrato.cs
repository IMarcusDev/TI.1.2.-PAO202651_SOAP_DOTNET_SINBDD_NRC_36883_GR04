using MOV_CON_UNI_DOTNET_SOAP.Controllers;
using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Enums;
using MOV_CON_UNI_DOTNET_SOAP.Models.Repositories;
using MOV_CON_UNI_DOTNET_SOAP.Services;
using MOV_CON_UNI_DOTNET_SOAP.Utils;

namespace MOV_CON_UNI_DOTNET_SOAP.Pruebas
{
    public static class PruebasContrato
    {
        private static readonly Dictionary<Magnitud, string> UnidadesEsperadas = new()
        {
            [Magnitud.Area] = "cm2,m2,km2,ha,ft2,ac",
            [Magnitud.Longitud] = "m,cm,ft,yd,mi",
            [Magnitud.Masa] = "g,kg,oz,lb,qq",
            [Magnitud.Temperatura] = "°C,°F,K,°N,°Ré",
            [Magnitud.Volumen] = "mL,L,m3,ft3,gal",
            [Magnitud.Tiempo] = "ms,s,min,h,d,w",
        };

        public static Ejecutor Crear(string url)
        {
            var e = new Ejecutor();
            var sesion = new SessionService();
            var conversion = new ConversionController(new ConvertRepository(), new CatalogRepository(), sesion);

            void Caso(string nombre, Func<Task> prueba, string? servidor = null)
                => e.Caso(nombre, async () =>
                {
                    using (RedSoapSimulada.Servidor(servidor ?? url))
                        await prueba();
                });

            Caso("Login con credenciales incorrectas", async () =>
            {
                LoginResult r = await new LoginController(new LoginRepository(), new SessionService()).LoginAsync("user", "mala");
                Afirmar.Verdadero(!r.Success, "debía fallar");
                Afirmar.Igual("Credenciales Incorrectas", r.Message);
            });
            Caso("Login user/1234 guarda el token", async () =>
            {
                LoginResult r = await new LoginController(new LoginRepository(), sesion).LoginAsync("user", "1234");
                Afirmar.Verdadero(r.Success && Guid.TryParse(sesion.Token, out _), r.Message);
            });

            foreach (var (magnitud, esperadas) in UnidadesEsperadas)
                Caso($"Unidades de {magnitud}: {esperadas}", async () =>
                {
                    CatalogResult r = await conversion.ObtenerUnidadesAsync(magnitud);
                    Afirmar.Verdadero(r.Success, r.Message);
                    Afirmar.Igual(esperadas, string.Join(",", r.Unidades));
                });

            Caso("Catálogo completo en paralelo", async () =>
            {
                CatalogoCompletoResult r = await new CatalogController(new CatalogRepository(), sesion).ObtenerCatalogoAsync();
                Afirmar.Verdadero(r.Success, r.Message);
                Afirmar.Igual("6,5,5,5,5,6", string.Join(",", r.Grupos.Select(g => g.Count)));
            });

            (Magnitud Mag, string Valor, string De, string A, string Texto)[] conversiones =
            [
                (Magnitud.Area, "1", "km2", "ac", "247,1052"),
                (Magnitud.Longitud, "1234", "cm", "m", "12,34"),
                (Magnitud.Masa, "1", "kg", "g", "1.000"),
                (Magnitud.Temperatura, "100", "°C", "°Ré", "80"),
                (Magnitud.Temperatura, "-40", "°C", "°F", "-40"),
                (Magnitud.Temperatura, "0", "K", "°C", "-273,15"),
                (Magnitud.Temperatura, "33", "°N", "°C", "100"),
                (Magnitud.Volumen, "1", "L", "mL", "1.000"),
                (Magnitud.Tiempo, "1", "h", "min", "60"),
            ];
            foreach (var (mag, valor, de, a, texto) in conversiones)
                Caso($"Convertir {valor} {de} → {a} = {texto}", async () =>
                {
                    ConvertResult r = await conversion.ConvertAsync(mag, valor, de, a);
                    Afirmar.Verdadero(r.Success, r.Message);
                    Afirmar.Igual(texto, FormatoNumero.Formatear(r.Value));
                });

            Caso("Valor máximo 999999999,999999 w → ms sin notación científica", async () =>
            {
                ConvertResult r = await conversion.ConvertAsync(Magnitud.Tiempo, "999999999,999999", "w", "ms");
                Afirmar.Verdadero(r.Success && double.IsFinite(r.Value), r.Message);
                Afirmar.Verdadero(!FormatoNumero.Formatear(r.Value).Contains('E'), FormatoNumero.Formatear(r.Value));
            });

            Caso("Token desconocido (servidor reiniciado) → sesión expirada en conversión", async () =>
            {
                var vencida = new SessionService();
                vencida.Iniciar("user", Guid.NewGuid().ToString());
                var control = new ConversionController(new ConvertRepository(), new CatalogRepository(), vencida);
                ConvertResult r = await control.ConvertAsync(Magnitud.Area, "1", "km2", "ac");
                Afirmar.Verdadero(r.SesionExpirada && !vencida.IsAuthenticated, r.Message);
            });
            Caso("Token desconocido → sesión expirada en catálogo", async () =>
            {
                var vencida = new SessionService();
                vencida.Iniciar("user", Guid.NewGuid().ToString());
                CatalogoCompletoResult r = await new CatalogController(new CatalogRepository(), vencida).ObtenerCatalogoAsync();
                Afirmar.Verdadero(r.SesionExpirada && !vencida.IsAuthenticated, r.Message);
            });

            Caso("Servicio no publicado en esa ruta → «No se encontró el servidor»", async () =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.EmpiezaCon("No se encontró el servidor en", r.Message);
            }, servidor: url.TrimEnd('/') + "/noexiste/");
            Caso("Puerto sin servidor → «No se encontró el servidor»", async () =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.Verdadero(!r.Success, "debía fallar");
                Afirmar.EmpiezaCon("No se encontró el servidor en", r.Message);
            }, servidor: "http://127.0.0.1:5999/");

            return e;
        }
    }
}
