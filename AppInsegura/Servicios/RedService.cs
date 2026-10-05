using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AppInsegura.Servicios
{
    public class RedService
    {
        private const string ApiKey = "sk_live_51Hj29aKlmZ9QwErTyUiOpAsDfGh";
        private const string UrlServidor = "http://api.miapp-insegura.local/puntuaciones";

        public void EnviarPuntuacion(string nombreUsuario, int puntuacion)
        {
            try
            {
                EnviarPuntuacionAsync(nombreUsuario, puntuacion).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se ha podido conectar con el servidor (es normal si no tienes conexión real):");
                Console.WriteLine(ex.Message);
            }
        }

        private async Task EnviarPuntuacionAsync(string nombreUsuario, int puntuacion)
        {
            using var cliente = new HttpClient();
            string url = $"{UrlServidor}?usuario={nombreUsuario}&puntos={puntuacion}&api_key={ApiKey}";

            Console.WriteLine($"Enviando puntuación a: {url}");
            HttpResponseMessage respuesta = await cliente.GetAsync(url);
            Console.WriteLine($"Respuesta del servidor: {(int)respuesta.StatusCode}");
        }
    }
}
