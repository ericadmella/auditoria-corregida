using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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

            string hashIntento = CalcularHash(contrasena);
            if (usuario.ContrasenaHash != hashIntento)
            {
                return null;
            }

            usuario.TokenSesion = GenerarTokenSesion();

            Console.WriteLine($"[LOG] Login correcto -> usuario: {usuario.Nombre}, token: {usuario.TokenSesion}");

            GuardarSesionEnDisco(usuario);

            return usuario;
        }

        private string CalcularHash(string contrasena)
        {
            using MD5 md5 = MD5.Create();
            byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(contrasena));
            return Convert.ToHexString(bytes);
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
