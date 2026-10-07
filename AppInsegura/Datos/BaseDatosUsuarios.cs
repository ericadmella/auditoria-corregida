using System;
using System.Collections.Generic;
using System.Linq;
using AppInsegura.Modelos;

namespace AppInsegura.Datos
{
    // Simula una tabla de base de datos (equivalente a una tabla SQLite/sqflite).
    // No usa un motor real para que el proyecto compile sin dependencias externas.
    public class BaseDatosUsuarios
    {
        private readonly List<Usuario> usuarios = new List<Usuario>();

        public void Agregar(Usuario usuario)
        {
            usuarios.Add(usuario);
        }

        public List<Usuario> ListarTodos()
        {
            return usuarios;
        }

        public Usuario? BuscarExacto(string nombre)
        {
            return usuarios.FirstOrDefault(u => u.Nombre == nombre);
        }

        public Usuario? BuscarPorNombre(string nombreBuscado)
        {
            // ERROR CORREGIDO: Inyección SQL.
            // Antes: la consulta se construía concatenando la entrada del usuario
            //   $"SELECT * FROM usuarios WHERE nombre = '{nombreBuscado}'"
            // Un atacante podía escribir  ' OR '1'='1  y la condición pasaba a ser
            // siempre verdadera, obteniendo usuarios sin conocer su nombre.
            // Ahora: consulta parametrizada (@nombre) que trata la entrada solo como dato.
            //
            // Consulta parametrizada: el valor que escribe el usuario viaja separado
            // del SQL y nunca se interpreta como parte de la consulta.
            const string consulta = "SELECT * FROM usuarios WHERE nombre = @nombre";
            return EjecutarConsultaParametrizada(consulta, nombreBuscado);
        }

        // ERROR CORREGIDO: el antiguo EjecutarConsultaSimulada interpretaba la cadena
        // SQL completa (con Regex y comprobando patrones como "' OR '1'='1"), por lo
        // que la entrada del usuario podía alterar la lógica de la consulta.
        // Se ha sustituido por un método que recibe el parámetro por separado.
        //
        // Simulación simplificada de un motor de consultas con parámetros, únicamente
        // para que el ejercicio se pueda ejecutar sin una base de datos real.
        // El parámetro se compara como dato literal, igual que haría un motor SQL
        // con "@nombre" enlazado mediante command.Parameters.
        private Usuario? EjecutarConsultaParametrizada(string consulta, string nombre)
        {
            Console.WriteLine($"[DB] {consulta}");
            return usuarios.FirstOrDefault(u => u.Nombre == nombre);
        }
    }
}
