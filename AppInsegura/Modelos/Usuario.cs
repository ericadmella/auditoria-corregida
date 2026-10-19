namespace AppInsegura.Modelos
{
    // Representa a un usuario de la aplicación.
    public class Usuario
    {
        public string Nombre { get; set; } = "";

        // Nunca se guarda la contraseña: solo su hash.
        public string ContrasenaHash { get; set; } = "";

        // FIX #2: sal aleatoria y única para cada usuario (en hexadecimal).
        // Hace que dos contraseñas iguales tengan hashes distintos
        // y que las rainbow tables no sirvan.
        public string Sal { get; set; } = "";

        public string Rol { get; set; } = "jugador";

        // Token de la sesión activa. Es un dato sensible: no se muestra ni se guarda.
        public string TokenSesion { get; set; } = "";
    }
}
