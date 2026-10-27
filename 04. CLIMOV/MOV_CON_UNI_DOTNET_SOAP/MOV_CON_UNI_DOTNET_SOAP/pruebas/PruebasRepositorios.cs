using MOV_CON_UNI_DOTNET_SOAP.Controllers;
using MOV_CON_UNI_DOTNET_SOAP.Models.data;
using MOV_CON_UNI_DOTNET_SOAP.Models.Enums;
using MOV_CON_UNI_DOTNET_SOAP.Models.Repositories;
using MOV_CON_UNI_DOTNET_SOAP.Services;
using System.Net.Sockets;
using static MOV_CON_UNI_DOTNET_SOAP.Pruebas.ManejadorSimulado;

namespace MOV_CON_UNI_DOTNET_SOAP.Pruebas
{
    public static class PruebasRepositorios
    {
        private const string Token = "11111111-2222-3333-4444-555555555555";

        private static readonly string LoginOk = Sobre("Login",
            $"<estado>OK</estado><mensaje>Ingreso de Sesión Exitoso!</mensaje><token>{Token}</token>");
        private static readonly string LoginMala = Sobre("Login",
            "<estado>ERROR</estado><mensaje>Credenciales Incorrectas</mensaje><token />");
        private static readonly string CatalogoTokenInvalido = Sobre("GetCatalog",
            "<estado>ERROR</estado><mensaje>Token Inválido</mensaje><resultado />");
        private static readonly string ConversionTokenInvalido = Sobre("Convert",
            "<estado>ERROR</estado><mensaje>Token Inválido</mensaje><resultado>0</resultado>");

        public static Ejecutor Crear()
        {
            var e = new Ejecutor();

            Caso(e, "Login: envía Login(username, password) a LoginWebService.svc", Responde(200, LoginOk), async red =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.Verdadero(r.Success, r.Message);
                Afirmar.Igual(Token, r.Token);
                Peticion p = red.Peticiones.Single();
                Afirmar.Igual("/LoginWebService.svc", p.Ruta);
                Afirmar.Igual("http://tempuri.org/ILoginWebService/Login", p.Accion);
                Afirmar.Igual("user", p.Valor("username"));
                Afirmar.Igual("1234", p.Valor("password"));
            });
            Caso(e, "Login: ERROR «Credenciales Incorrectas» → mensaje del servidor", Responde(200, LoginMala), async _ =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "x");
                Afirmar.Verdadero(!r.Success, "debía fallar");
                Afirmar.Igual("Credenciales Incorrectas", r.Message);
            });
            Caso(e, "Login: OK sin token → respuesta incompleta",
                Responde(200, Sobre("Login", "<estado>OK</estado><mensaje>ok</mensaje><token />")), async _ =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.Igual(SoapErrores.RespuestaIncompleta, r.Message);
            });
            Caso(e, "Login: respuesta sin LoginResult → respuesta incompleta", Responde(200, SobreSinResultado("Login")), async _ =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.Igual(SoapErrores.RespuestaIncompleta, r.Message);
            });
            Caso(e, "Login: sin red → no llama al servidor", Responde(200, LoginOk), async red =>
            {
                SoapErrores.SimularSinRed = true;
                try
                {
                    LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                    Afirmar.Igual(SoapErrores.SinConexion, r.Message);
                    Afirmar.Igual(0, red.Peticiones.Count, "peticiones");
                }
                finally
                {
                    SoapErrores.SimularSinRed = false;
                }
            });
            Caso(e, "Login: conexión rechazada → no lanza, «No se encontró el servidor»",
                Lanza(new HttpRequestException("x", new SocketException((int)SocketError.ConnectionRefused))), async _ =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.Verdadero(!r.Success, "debía fallar");
                Afirmar.EmpiezaCon("No se encontró el servidor en", r.Message);
            });
            Caso(e, "Login: 404 (servicio no publicado) → «No se encontró el servidor»", Segun(_ => SinCuerpo(404)), async _ =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.EmpiezaCon("No se encontró el servidor en", r.Message);
            });
            Caso(e, "Login: SOAP Fault 500 → «El servidor no pudo procesar la solicitud»",
                Responde(500, Falla("s:Server", "Object reference not set to an instance of an object.")), async _ =>
            {
                LoginResult r = await new LoginRepository().LoginAsync("user", "1234");
                Afirmar.EmpiezaCon("El servidor no pudo procesar la solicitud: Object reference not set to an instance of an object.", r.Message);
            });

            Caso(e, "Catálogo: envía GetCatalog(type, token) y conserva el orden",
                Responde(200, Sobre("GetCatalog",
                    "<estado>OK</estado><mensaje>Obtención de Catálogo Exitosa</mensaje><resultado><string>°C</string><string>°F</string><string>K</string><string>°N</string><string>°Ré</string></resultado>")),
                async red =>
            {
                CatalogResult r = await new CatalogRepository().GetCatalogAsync(Token, Magnitud.Temperatura);
                Afirmar.Igual("°C,°F,K,°N,°Ré", string.Join(",", r.Unidades));
                Peticion p = red.Peticiones.Single();
                Afirmar.Igual("/CatalogWebService.svc", p.Ruta);
                Afirmar.Igual("http://tempuri.org/ICatalogWebService/GetCatalog", p.Accion);
                Afirmar.Igual("Temperatura", p.Valor("type"));
                Afirmar.Igual(Token, p.Valor("token"));
            });
            Caso(e, "Catálogo: «Token Inválido» → SesionExpirada", Responde(200, CatalogoTokenInvalido), async _ =>
            {
                CatalogResult r = await new CatalogRepository().GetCatalogAsync(Token, Magnitud.Area);
                Afirmar.Verdadero(r.SesionExpirada && !r.Success, "debía marcar sesión expirada");
            });
            Caso(e, "Catálogo: «Tipo de Catálogo Inexistente» → error, no sesión",
                Responde(200, Sobre("GetCatalog", "<estado>ERROR</estado><mensaje>Tipo de Catálogo Inexistente</mensaje><resultado />")), async _ =>
            {
                CatalogResult r = await new CatalogRepository().GetCatalogAsync(Token, Magnitud.Area);
                Afirmar.Igual("Tipo de Catálogo Inexistente", r.Message);
                Afirmar.Verdadero(!r.SesionExpirada, "no era sesión");
            });
            Caso(e, "Catálogo: OK sin unidades → mensaje con la magnitud",
                Responde(200, Sobre("GetCatalog", "<estado>OK</estado><mensaje>ok</mensaje><resultado />")), async _ =>
            {
                CatalogResult r = await new CatalogRepository().GetCatalogAsync(Token, Magnitud.Area);
                Afirmar.Igual("El servidor no devolvió unidades para Área.", r.Message);
            });
            Caso(e, "Catálogo: unidades repetidas o vacías se descartan",
                Responde(200, Sobre("GetCatalog",
                    "<estado>OK</estado><mensaje>ok</mensaje><resultado><string>m</string><string> </string><string>m</string><string>cm</string></resultado>")),
                async _ =>
            {
                CatalogResult r = await new CatalogRepository().GetCatalogAsync(Token, Magnitud.Longitud);
                Afirmar.Igual("m,cm", string.Join(",", r.Unidades));
            });

            Caso(e, "Conversión: envía Convert(token, type, value, from, to) con unidades Unicode intactas",
                Responde(200, Sobre("Convert", "<estado>OK</estado><mensaje>Conversión Exitosa</mensaje><resultado>80</resultado>")),
                async red =>
            {
                ConvertResult r = await new ConvertRepository().ConvertAsync(Token, new Conversion(Magnitud.Temperatura, 100, "°C", "°Ré"));
                Afirmar.Cerca(80, r.Value);
                Peticion p = red.Peticiones.Single();
                Afirmar.Igual("/ConverterWebService.svc", p.Ruta);
                Afirmar.Igual("http://tempuri.org/IConverterWebService/Convert", p.Accion);
                Afirmar.Igual(Token, p.Valor("token"));
                Afirmar.Igual("Temperatura", p.Valor("type"));
                Afirmar.Igual("100", p.Valor("value"));
                Afirmar.Igual("°C", p.Valor("from"));
                Afirmar.Igual("°Ré", p.Valor("to"));
            });
            Caso(e, "Conversión: «Token Inválido» → SesionExpirada", Responde(200, ConversionTokenInvalido), async _ =>
            {
                ConvertResult r = await new ConvertRepository().ConvertAsync(Token, new Conversion(Magnitud.Area, 1, "m2", "ha"));
                Afirmar.Verdadero(r.SesionExpirada && !r.Success, "debía marcar sesión expirada");
            });
            Caso(e, "Conversión: «No se pudo hacer la conversión» → error, NO sesión expirada",
                Responde(200, Sobre("Convert", "<estado>ERROR</estado><mensaje>No se pudo hacer la conversión</mensaje><resultado>0</resultado>")),
                async _ =>
            {
                ConvertResult r = await new ConvertRepository().ConvertAsync(Token, new Conversion(Magnitud.Area, 1, "m2", "ha"));
                Afirmar.Igual("No se pudo hacer la conversión", r.Message);
                Afirmar.Verdadero(!r.SesionExpirada && !r.Success, "no debía ser sesión expirada");
            });
            Caso(e, "Conversión: resultado infinito → resultado no válido",
                Responde(200, Sobre("Convert", "<estado>OK</estado><mensaje>ok</mensaje><resultado>INF</resultado>")), async _ =>
            {
                ConvertResult r = await new ConvertRepository().ConvertAsync(Token, new Conversion(Magnitud.Area, 1, "m2", "ha"));
                Afirmar.Igual("El servidor devolvió un resultado no válido.", r.Message);
            });
            Caso(e, "Conversión: servidor lento → mensaje de 15 segundos",
                Demora(TimeSpan.FromSeconds(5), Sobre("Convert", "<estado>OK</estado><resultado>1</resultado>")),
                async _ =>
            {
                ConvertResult r = await new ConvertRepository().ConvertAsync(Token, new Conversion(Magnitud.Area, 1, "m2", "ha"));
                Afirmar.EmpiezaCon("El servidor no respondió en 15 segundos. Intente de nuevo.", r.Message);
            }, espera: TimeSpan.FromSeconds(1));

            (string? Usuario, string? Clave, string Mensaje)[] login =
            [
                (null, "1234", "Ingrese el usuario"),
                ("", "1234", "Ingrese el usuario"),
                ("   ", "1234", "Ingrese el usuario"),
                ("user", null, "Ingrese la contraseña"),
                ("user", "   ", "Ingrese la contraseña"),
                (new string('u', 51), "1234", "El usuario no puede superar 50 caracteres"),
                ("user", new string('c', 65), "La contraseña no puede superar 64 caracteres"),
            ];
            foreach (var (usuario, clave, mensaje) in login)
                Caso(e, $"LoginController: «{Recortar(usuario)}»/«{Recortar(clave)}» → «{mensaje}» sin llamar a la red",
                    Responde(200, LoginOk), async red =>
                {
                    LoginResult r = await new LoginController(new LoginRepository(), new SessionService()).LoginAsync(usuario, clave);
                    Afirmar.Igual(mensaje, r.Message);
                    Afirmar.Igual(0, red.Peticiones.Count, "peticiones");
                });
            Caso(e, "LoginController: 50 y 64 caracteres sí se aceptan", Responde(200, LoginMala), async _ =>
            {
                LoginResult r = await new LoginController(new LoginRepository(), new SessionService())
                    .LoginAsync(new string('u', 50), new string('c', 64));
                Afirmar.Igual("Credenciales Incorrectas", r.Message);
            });
            Caso(e, "LoginController: aplica Trim y guarda la sesión", Responde(200, LoginOk), async red =>
            {
                var sesion = new SessionService();
                LoginResult r = await new LoginController(new LoginRepository(), sesion).LoginAsync("  user ", " 1234  ");
                Afirmar.Verdadero(r.Success, r.Message);
                Peticion p = red.Peticiones.Single();
                Afirmar.Igual("user", p.Valor("username"));
                Afirmar.Igual("1234", p.Valor("password"));
                Afirmar.Igual(Token, sesion.Token);
                Afirmar.Igual("user", sesion.Usuario);
            });

            Caso(e, "ConversionController: origen == destino no llama al servidor", Segun(_ => SinCuerpo(500)), async red =>
            {
                ConvertResult r = await ControlConversion(ConSesion()).ConvertAsync(Magnitud.Longitud, "2,5", "m", "m");
                Afirmar.Verdadero(r.Success, r.Message);
                Afirmar.Igual("Origen y destino son la misma unidad; el valor no cambia.", r.Message);
                Afirmar.Igual(2.5, r.Value);
                Afirmar.Igual(0, red.Peticiones.Count, "peticiones");
            });
            Caso(e, "ConversionController: origen == destino igual valida antes (negativo en Masa)", Segun(_ => SinCuerpo(500)), async _ =>
            {
                ConvertResult r = await ControlConversion(ConSesion()).ConvertAsync(Magnitud.Masa, "-1", "kg", "kg");
                Afirmar.Igual("No se permiten valores negativos en Masa", r.Message);
            });
            Caso(e, "ConversionController: token rechazado → limpia la sesión y avisa", Responde(200, ConversionTokenInvalido), async _ =>
            {
                SessionService sesion = ConSesion();
                ConvertResult r = await ControlConversion(sesion).ConvertAsync(Magnitud.Area, "1", "km2", "ac");
                Afirmar.Verdadero(r.SesionExpirada, "debía marcar sesión expirada");
                Afirmar.Igual(ControladorConSesion.MensajeSesionExpirada, r.Message);
                Afirmar.Verdadero(!sesion.IsAuthenticated, "la sesión debía quedar vacía");
            });
            Caso(e, "ConversionController: sin sesión → expirada sin llamar a la red", Responde(200, LoginOk), async red =>
            {
                ConvertResult r = await ControlConversion(new SessionService()).ConvertAsync(Magnitud.Area, "1", "km2", "ac");
                Afirmar.Verdadero(r.SesionExpirada, "debía marcar sesión expirada");
                Afirmar.Igual(0, red.Peticiones.Count, "peticiones");
            });
            Caso(e, "ConversionController: conversión OK trae el detalle",
                Responde(200, Sobre("Convert", "<estado>OK</estado><mensaje>Conversión Exitosa</mensaje><resultado>247.10516301527602</resultado>")),
                async _ =>
            {
                ConvertResult r = await ControlConversion(ConSesion()).ConvertAsync(Magnitud.Area, "1", "km2", "ac");
                Afirmar.Verdadero(r.Conversion is { From: "km2", To: "ac" }, "faltaba el detalle");
                Afirmar.Cerca(247.10516301527602, r.Conversion!.Result);
            });

            string catalogoOk = Sobre("GetCatalog", "<estado>OK</estado><mensaje>ok</mensaje><resultado><string>m</string><string>cm</string></resultado>");
            Caso(e, "CatalogController: las 6 magnitudes en paralelo con nombres en español", Responde(200, catalogoOk), async red =>
            {
                CatalogoCompletoResult r = await new CatalogController(new CatalogRepository(), ConSesion()).ObtenerCatalogoAsync();
                Afirmar.Verdadero(r.Success, r.Message);
                Afirmar.Igual(6, r.Grupos.Count, "grupos");
                Afirmar.Igual(6, red.Peticiones.Count, "peticiones");
                Afirmar.Igual("Metro", r.Grupos[0][0].Nombre);
            });
            Caso(e, "CatalogController: si una magnitud falla no hay catálogo a medias",
                Segun(p => p.Valor("type") == "Masa"
                    ? Respuesta(200, Sobre("GetCatalog", "<estado>ERROR</estado><mensaje>Tipo de Catálogo Inexistente</mensaje><resultado />"))
                    : Respuesta(200, catalogoOk)),
                async _ =>
            {
                CatalogoCompletoResult r = await new CatalogController(new CatalogRepository(), ConSesion()).ObtenerCatalogoAsync();
                Afirmar.Verdadero(!r.Success, "debía fallar");
                Afirmar.Igual(0, r.Grupos.Count, "grupos");
                Afirmar.Igual("Tipo de Catálogo Inexistente", r.Message);
            });
            Caso(e, "CatalogController: token rechazado → limpia la sesión", Responde(200, CatalogoTokenInvalido), async _ =>
            {
                SessionService sesion = ConSesion();
                CatalogoCompletoResult r = await new CatalogController(new CatalogRepository(), sesion).ObtenerCatalogoAsync();
                Afirmar.Verdadero(r.SesionExpirada && !sesion.IsAuthenticated, "debía expirar la sesión");
                Afirmar.Igual(ControladorConSesion.MensajeSesionExpirada, r.Message);
            });

            return e;
        }

        private static void Caso(Ejecutor e, string nombre, ManejadorSimulado red, Func<ManejadorSimulado, Task> prueba, TimeSpan? espera = null)
            => e.Caso(nombre, async () =>
            {
                using (RedSoapSimulada.Usar(red, espera))
                    await prueba(red);
            });

        private static SessionService ConSesion()
        {
            var sesion = new SessionService();
            sesion.Iniciar("user", Token);
            return sesion;
        }

        private static ConversionController ControlConversion(SessionService sesion)
            => new(new ConvertRepository(), new CatalogRepository(), sesion);

        private static string Recortar(string? texto) => texto is { Length: > 10 } ? $"{texto[..3]}…({texto.Length})" : texto ?? "null";
    }
}
