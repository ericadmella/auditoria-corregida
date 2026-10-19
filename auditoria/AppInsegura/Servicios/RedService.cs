using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AppInsegura.Servicios
{
    // Envía la puntuación del jugador al servidor.
    public class RedService
    {
        // FIX #4: antes la API key estaba escrita en el código y se podía
        // sacar del .exe/.dll con un descompilador (ILSpy, dnSpy...).
        // Ahora se lee de la variable de entorno APP_API_KEY y no está en el repositorio.
        private static readonly string ApiKey =
            Environment.GetEnvironmentVariable("APP_API_KEY") ?? "";

        // FIX #5: antes http:// (texto plano, vulnerable a Man-in-the-Middle).
        // Ahora https://: la conexión va cifrada con TLS y se verifica el servidor.
        private const string UrlServidor = "https://api.miapp.local/puntuaciones";

        public void EnviarPuntuacion(string nombreUsuario, int puntuacion)
        {
            try
            {
                EnviarPuntuacionAsync(nombreUsuario, puntuacion).GetAwaiter().GetResult();
            }
            catch (Exception)
            {
                // FIX #10: antes se mostraba ex.Message con detalles técnicos.
                // Ahora el usuario solo ve un mensaje genérico.
                Console.WriteLine("No se ha podido conectar con el servidor.");
            }
        }

        private async Task EnviarPuntuacionAsync(string nombreUsuario, int puntuacion)
        {
            using var cliente = new HttpClient();

            // FIX #9: antes los datos y la key iban en la URL (?usuario=...&api_key=...)
            // y quedaban guardados en logs, historiales y proxys.
            // Ahora van en el cuerpo de una petición POST.
            var datos = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("usuario", nombreUsuario),
                new KeyValuePair<string, string>("puntos", puntuacion.ToString())
            });

            // FIX #4/#9: la key viaja en la cabecera Authorization, no en la URL.
            cliente.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", ApiKey);

            // Ya no se imprime la URL completa (antes mostraba la key por pantalla).
            HttpResponseMessage respuesta = await cliente.PostAsync(UrlServidor, datos);
            Console.WriteLine($"Respuesta del servidor: {(int)respuesta.StatusCode}");
        }
    }
}
