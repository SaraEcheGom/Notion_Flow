using NotionFlow.App.Models;

namespace NotionFlow.App.Services
{
    public class SchoolService
    {
        public static List<Profesor> Profesores = new();

        public static List<Estudiante> Estudiantes = new();

        public Profesor CrearProfesor(string nombre, string correo, string materia)
        {
            var profesor = new Profesor
            {
                Id = Profesores.Count + 1,
                Nombre = nombre,
                Correo = correo,
                Materia = materia
            };

            Profesores.Add(profesor);

            return profesor;
        }

        public Estudiante CrearEstudiante(string nombre, string grado, string profesorCorreo)
        {
            var estudiante = new Estudiante
            {
                Id = Estudiantes.Count + 1,
                Nombre = nombre,
                Grado = grado,
                ProfesorCorreo = profesorCorreo
            };

            Estudiantes.Add(estudiante);

            var profesor = Profesores.FirstOrDefault(p => p.Correo == profesorCorreo);

            if (profesor != null)
            {
                profesor.Estudiantes.Add(estudiante);
            }

            return estudiante;
        }

        public List<Estudiante> ObtenerEstudiantesProfesor(string correoProfesor)
        {
            return Estudiantes
                .Where(e => e.ProfesorCorreo == correoProfesor)
                .ToList();
        }
    }
}