using System;
using System.Security.Cryptography;
using AppInsegura.Datos;
using AppInsegura.Modelos;

namespace AppInsegura.Servicios
{
    // Gestiona el registro y el inicio de sesión.
    public class AuthService
    {
        private readonly BaseDatosUsuarios baseDatos;

        // Nº de repeticiones de PBKDF2: cuantas más, más lento es cada intento de fuerza bruta.
        private const int Iteraciones = 100_000;

        public AuthService(BaseDatosUsuarios baseDatos) => this.baseDatos = baseDatos;

        public Usuario Registrar(string nombre, string contrasena, string rol = "jugador")
        {
            // FIX #2: sal de 16 bytes generada con un generador criptográfico.
            byte[] sal = RandomNumberGenerator.GetBytes(16);

            var nuevo = new Usuario
            {
                Nombre = nombre,
                Sal = Convert.ToHexString(sal),           // la sal se guarda para poder verificar luego
                ContrasenaHash = CalcularHash(contrasena, sal),
                Rol = rol,
                TokenSesion = ""
            };
            baseDatos.Agregar(nuevo);
            return nuevo;
        }

        public Usuario? IniciarSesion(string nombre, string contrasena)
        {
            Usuario? usuario = baseDatos.BuscarExacto(nombre);
            if (usuario == null) return null;

            // Se recalcula el hash con la MISMA sal del usuario y se compara.
            byte[] sal = Convert.FromHexString(usuario.Sal);
            string hashIntento = CalcularHash(contrasena, sal);

            // FIX #2: comparación en tiempo constante.
            // Una comparación normal se para en el primer byte distinto, y midiendo
            // el tiempo se podría deducir información. Esta tarda siempre lo mismo.
            bool ok = CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(hashIntento),
                Convert.FromHexString(usuario.ContrasenaHash));
            if (!ok) return null;

            // FIX #3: token nuevo e impredecible en cada inicio de sesión.
            usuario.TokenSesion = GenerarTokenSesion();

            // FIX #7: antes se imprimía "[LOG] ... token: ..." y el token se filtraba.
            // FIX #8: antes se escribía "usuario:token" en sesion.txt en texto plano.
            // Ahora no se hace ninguna de las dos cosas.
            return usuario;
        }

        // FIX #2: antes MD5 sin sal (muy rápido y vulnerable a diccionario/rainbow tables).
        // Ahora PBKDF2 con SHA-256, sal y 100.000 iteraciones: un hash lento a propósito.
        private string CalcularHash(string contrasena, byte[] sal)
        {
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, 32);
            return Convert.ToHexString(hash);
        }

        // FIX #3: antes new Random() con 6 dígitos (predecible y solo 1.000.000 de opciones).
        // Ahora 32 bytes de RandomNumberGenerator: 2^256 combinaciones, imposible de adivinar.
        private string GenerarTokenSesion()
            => Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
}
