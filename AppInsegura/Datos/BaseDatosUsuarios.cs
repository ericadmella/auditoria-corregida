using System.Collections.Generic;
using System.Linq;
using AppInsegura.Modelos;

namespace AppInsegura.Datos
{
    // Simula una tabla de usuarios en memoria (sin motor de BD real).
    public class BaseDatosUsuarios
    {
        private readonly List<Usuario> usuarios = new List<Usuario>();

        public void Agregar(Usuario usuario) => usuarios.Add(usuario);

        public List<Usuario> ListarTodos() => usuarios;

        public Usuario? BuscarExacto(string nombre)
            => usuarios.FirstOrDefault(u => u.Nombre == nombre);

        // FIX #1 (inyección SQL):
        // Antes se montaba "SELECT ... WHERE nombre = '" + entrada + "'",
        // y con ' OR '1'='1 la condición era siempre verdadera.
        // Ahora la entrada solo se compara como texto y nunca se interpreta como código.
        // Con una BD real se usaría una consulta parametrizada:
        //   cmd.CommandText = "SELECT * FROM usuarios WHERE nombre = @nombre";
        //   cmd.Parameters.AddWithValue("@nombre", nombreBuscado);
        //
        // FIX #13: se elimina el Console.WriteLine que mostraba la consulta,
        // porque le enseñaba al atacante cómo estaba montada.
        public Usuario? BuscarPorNombre(string nombreBuscado)
            => usuarios.FirstOrDefault(u => u.Nombre == nombreBuscado);
    }
}
