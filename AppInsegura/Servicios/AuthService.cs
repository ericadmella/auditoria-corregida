using System;
using System.IO;
using System.Security.Cryptography;
using AppInsegura.Datos;
using AppInsegura.Modelos;

namespace AppInsegura.Servicios
{
    public class AuthService
    {
        private readonly BaseDatosUsuarios baseDatos;

        public AuthService(BaseDatosUsuarios baseDatos)
        {
            this.baseDatos = baseDatos;
        }

        public Usuario Registrar(string nombre, string contrasena, string rol = "jugador")
        {
            var nuevo = new Usuario
            {
                Nombre = nombre,
                ContrasenaHash = CalcularHash(contrasena),
                Rol = rol,
                TokenSesion = ""
            };

            baseDatos.Agregar(nuevo);
            return nuevo;
        }

        public Usuario? IniciarSesion(string nombre, string contrasena)
        {
            Usuario? usuario = baseDatos.BuscarExacto(nombre);
            if (usuario == null)
            {
                return null;
            }

            if (!VerificarContrasena(contrasena, usuario.ContrasenaHash))
            {
                return null;
            }

            usuario.TokenSesion = GenerarTokenSesion();

            Console.WriteLine($"[LOG] Login correcto -> usuario: {usuario.Nombre}, token: {usuario.TokenSesion}");

            GuardarSesionEnDisco(usuario);

            return usuario;
        }

        private const int TamanoSal = 16;
        private const int TamanoHash = 32;
        private const int Iteraciones = 100_000;

        // PBKDF2 con SHA-256 y sal aleatoria por usuario. Se guarda como "sal:hash".
        private string CalcularHash(string contrasena)
        {
            byte[] sal = RandomNumberGenerator.GetBytes(TamanoSal);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
            return $"{Convert.ToHexString(sal)}:{Convert.ToHexString(hash)}";
        }

        private bool VerificarContrasena(string contrasena, string hashGuardado)
        {
            string[] partes = hashGuardado.Split(':');
            if (partes.Length != 2)
            {
                return false;
            }

            byte[] sal = Convert.FromHexString(partes[0]);
            byte[] hashEsperado = Convert.FromHexString(partes[1]);
            byte[] hashIntento = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

            // Comparación en tiempo constante para no filtrar información por tiempos de respuesta.
            return CryptographicOperations.FixedTimeEquals(hashIntento, hashEsperado);
        }

        private string GenerarTokenSesion()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private void GuardarSesionEnDisco(Usuario usuario)
        {
            File.WriteAllText("sesion.txt", $"{usuario.Nombre}:{usuario.TokenSesion}");
        }
    }
}
