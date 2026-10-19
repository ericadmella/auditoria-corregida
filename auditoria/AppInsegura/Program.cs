using System;
using System.Text.RegularExpressions;
using AppInsegura.Datos;
using AppInsegura.Modelos;
using AppInsegura.Servicios;

namespace AppInsegura
{
    public class Program
    {
        private static readonly BaseDatosUsuarios baseDatos = new BaseDatosUsuarios();
        private static readonly AuthService auth = new AuthService(baseDatos);
        private static Usuario? usuarioActual = null;

        public static void Main(string[] args)
        {
            CargarUsuariosDeEjemplo();

            Console.WriteLine("=== Gestor de Usuarios y Partidas ===");
            Console.WriteLine("(usuarios de prueba: admin/admin1234, ana/ana2024)");
            Console.WriteLine();

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine() ?? "";

                try
                {
                    switch (opcion)
                    {
                        case "1":
                            Registrar();
                            break;
                        case "2":
                            IniciarSesion();
                            break;
                        case "3":
                            BuscarUsuario();
                            break;
                        case "4":
                            VerPerfil();
                            break;
                        case "5":
                            PanelAdministracion();
                            break;
                        case "6":
                            SincronizarConServidor();
                            break;
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
                }
                catch (Exception)
                {
                    // FIX #10: antes se imprimía ex.ToString() (stack trace con clases,
                    // métodos y rutas internas). Ahora el usuario solo ve un mensaje genérico;
                    // el detalle debería ir a un log interno que solo vean los desarrolladores.
                    Console.WriteLine("Ha ocurrido un error inesperado.");
                }

                Console.WriteLine();
            }

            Console.WriteLine("Hasta luego.");
        }

        private static void CargarUsuariosDeEjemplo()
        {
            auth.Registrar("admin", "admin1234", "admin");
            auth.Registrar("ana", "ana2024", "jugador");
        }

        private static void MostrarMenu()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"Usuario actual: {(usuarioActual != null ? usuarioActual.Nombre : "ninguno")}");
            Console.WriteLine("1. Registrar usuario");
            Console.WriteLine("2. Iniciar sesión");
            Console.WriteLine("3. Buscar usuario por nombre");
            Console.WriteLine("4. Ver mi perfil");
            if (usuarioActual != null && usuarioActual.Rol == "admin")
            {
                Console.WriteLine("5. Panel de administración");
            }
            Console.WriteLine("6. Sincronizar partida con el servidor");
            Console.WriteLine("0. Salir");
            Console.Write("Elige una opción: ");
        }

        private static void Registrar()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Contraseña: ");
            string contrasena = Console.ReadLine() ?? "";

            // FIX #11: validación de entrada (nunca confiar en lo que escribe el usuario).
            // Nombre: solo letras, números y _, entre 3 y 20 caracteres.
            if (!Regex.IsMatch(nombre, "^[a-zA-Z0-9_]{3,20}$"))
            {
                Console.WriteLine("Nombre no válido (3-20: letras, números o _).");
                return;
            }
            // Contraseña: mínimo 8 caracteres.
            if (contrasena.Length < 8)
            {
                Console.WriteLine("La contraseña debe tener al menos 8 caracteres.");
                return;
            }
            // No se permiten nombres repetidos (evita suplantar a otro usuario, p. ej. "admin").
            if (baseDatos.BuscarExacto(nombre) != null)
            {
                Console.WriteLine("Ese nombre de usuario ya existe.");
                return;
            }

            Usuario nuevo = auth.Registrar(nombre, contrasena);
            Console.WriteLine($"Usuario '{nuevo.Nombre}' registrado con rol '{nuevo.Rol}'.");
        }

        private static void IniciarSesion()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            Console.Write("Contraseña: ");
            string contrasena = Console.ReadLine() ?? "";

            Usuario? usuario = auth.IniciarSesion(nombre, contrasena);
            if (usuario == null)
            {
                Console.WriteLine("Usuario o contraseña incorrectos.");
                return;
            }

            usuarioActual = usuario;
            Console.WriteLine($"Bienvenido, {usuario.Nombre}.");
        }

        private static void BuscarUsuario()
        {
            Console.Write("Nombre a buscar: ");
            string nombre = Console.ReadLine() ?? "";

            Usuario? encontrado = baseDatos.BuscarPorNombre(nombre);
            Console.WriteLine(encontrado != null
                ? $"Encontrado: {encontrado.Nombre} (rol: {encontrado.Rol})"
                : "No se ha encontrado ningún usuario con ese nombre.");
        }

        private static void VerPerfil()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            Console.WriteLine($"Nombre: {usuarioActual.Nombre}");
            Console.WriteLine($"Rol: {usuarioActual.Rol}");
            // FIX #7: antes se mostraba el token de sesión por pantalla.
            // Quien lo vea puede suplantar al usuario, así que ya no se muestra.
        }

        private static void PanelAdministracion()
        {
            // FIX #6: antes solo se ocultaba la opción 5 en el menú, pero cualquier
            // jugador podía escribir "5" y entrar. Ocultar un botón no es seguridad:
            // el permiso se comprueba aquí, justo donde se realiza la acción.
            if (usuarioActual == null || usuarioActual.Rol != "admin")
            {
                Console.WriteLine("Acceso denegado.");
                return;
            }
            Console.WriteLine("=== PANEL DE ADMINISTRACIÓN ===");
            Console.WriteLine("Lista de usuarios registrados:");
            foreach (Usuario u in baseDatos.ListarTodos())
            {
                Console.WriteLine($" - {u.Nombre} ({u.Rol})");
            }
        }

        private static void SincronizarConServidor()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            var red = new RedService();
            red.EnviarPuntuacion(usuarioActual.Nombre, 1000);
        }
    }
}
