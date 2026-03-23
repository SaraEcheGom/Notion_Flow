using Microsoft.AspNetCore.Identity;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Data
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DataSeeder(AppDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task SeedAsync()
        {
            try
            {
                // 1. Crear roles
                await SeedRolesAsync();

                // 2. Crear usuarios
                await SeedUsersAsync();

                // 3. Crear cursos
                await SeedCoursesAsync();

                // 4. Asignar estudiantes a cursos
                await SeedCourseStudentsAsync();

                // 5. Crear contenido
                await SeedContentsAsync();

                // 6. Crear evaluaciones
                await SeedEvaluationsAsync();

                // 7. Crear calificaciones
                await SeedGradesAsync();

                await _context.SaveChangesAsync();
                Console.WriteLine("✓ Seed completado exitosamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error en seed: {ex.Message}");
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "Professor", "Student" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    Console.WriteLine($"✓ Role '{role}' created.");
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            var users = new[]
            {
                new { Id = "user-admin-001", Name = "Administrador del Sistema", Email = "admin@notionflow.com", Role = "Admin", UserName = "admin@notionflow.com", Password = "Admin123!" },
                new { Id = "user-teacher-001", Name = "María García López", Email = "maria.garcia@notionflow.com", Role = "Professor", UserName = "maria.garcia@notionflow.com", Password = "Profesor123!" },
                new { Id = "user-teacher-002", Name = "Juan Carlos Martínez", Email = "juan.martinez@notionflow.com", Role = "Professor", UserName = "juan.martinez@notionflow.com", Password = "Profesor123!" },
                new { Id = "user-student-001", Name = "Ana Rodríguez Pérez", Email = "ana.rodriguez@notionflow.com", Role = "Student", UserName = "ana.rodriguez@notionflow.com", Password = "Student123!" },
                new { Id = "user-student-002", Name = "Carlos López Fernández", Email = "carlos.lopez@notionflow.com", Role = "Student", UserName = "carlos.lopez@notionflow.com", Password = "Student123!" },
                new { Id = "user-student-003", Name = "Laura Sánchez Gómez", Email = "laura.sanchez@notionflow.com", Role = "Student", UserName = "laura.sanchez@notionflow.com", Password = "Student123!" }
            };

            foreach (var userData in users)
            {
                var user = await _userManager.FindByIdAsync(userData.Id);
                if (user == null)
                {
                    user = new User
                    {
                        Id = userData.Id,
                        Name = userData.Name,
                        Email = userData.Email,
                        UserName = userData.UserName,
                        NormalizedEmail = userData.Email.ToUpper(),
                        NormalizedUserName = userData.UserName.ToUpper(),
                        EmailConfirmed = true,
                        Role = userData.Role,
                        LockoutEnabled = true
                    };

                    var result = await _userManager.CreateAsync(user, userData.Password);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, userData.Role);
                        Console.WriteLine($"✓ Usuario '{userData.Email}' creado.");
                    }
                    else
                    {
                        Console.WriteLine($"✗ Error creando usuario: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private async Task SeedCoursesAsync()
        {
            if (_context.Courses!.Any())
                return;

            var courses = new[]
            {
                new Course { Id = 1, Name = "Matemáticas Avanzadas", Subject = "Matemáticas", TeacherId = "user-teacher-001", TeacherName = "María García López" },
                new Course { Id = 2, Name = "Programación con .NET", Subject = "Programación", TeacherId = "user-teacher-002", TeacherName = "Juan Carlos Martínez" },
                new Course { Id = 3, Name = "Base de Datos PostgreSQL", Subject = "Bases de Datos", TeacherId = "user-teacher-001", TeacherName = "María García López" },
                new Course { Id = 4, Name = "Desarrollo de Aplicaciones Móviles", Subject = "Móvil", TeacherId = "user-teacher-002", TeacherName = "Juan Carlos Martínez" }
            };

            _context.Courses!.AddRange(courses);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✓ {courses.Length} cursos creados.");
        }

        private async Task SeedCourseStudentsAsync()
        {
            if (_context.CourseStudents!.Any())
                return;

            var assignments = new[]
            {
                new CourseStudent { CourseId = 1, StudentId = "user-student-001" },
                new CourseStudent { CourseId = 1, StudentId = "user-student-002" },
                new CourseStudent { CourseId = 2, StudentId = "user-student-001" },
                new CourseStudent { CourseId = 2, StudentId = "user-student-003" },
                new CourseStudent { CourseId = 3, StudentId = "user-student-002" },
                new CourseStudent { CourseId = 3, StudentId = "user-student-003" },
                new CourseStudent { CourseId = 4, StudentId = "user-student-001" },
                new CourseStudent { CourseId = 4, StudentId = "user-student-002" },
                new CourseStudent { CourseId = 4, StudentId = "user-student-003" }
            };

            _context.CourseStudents!.AddRange(assignments);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✓ {assignments.Length} asignaciones de estudiantes creadas.");
        }

        private async Task SeedContentsAsync()
        {
            if (_context.Contents!.Any())
                return;

            var contents = new[]
            {
                new Content { Id = 1, CourseId = 1, Title = "Álgebra Lineal - Introducción", Description = "Introducción a conceptos fundamentales de álgebra lineal", Type = "Video", Url = "https://example.com/algebra-lineal-intro", PublicationDate = DateTime.UtcNow },
                new Content { Id = 2, CourseId = 1, Title = "Cálculo Diferencial - Ejercicios", Description = "Conjunto de ejercicios prácticos de cálculo diferencial", Type = "Document", Url = "https://example.com/calculo-ejercicios", PublicationDate = DateTime.UtcNow },
                new Content { Id = 3, CourseId = 2, Title = "C# Básico - Variables y Tipos", Description = "Introducción a variables y tipos en C#", Type = "Video", Url = "https://example.com/csharp-basics", PublicationDate = DateTime.UtcNow },
                new Content { Id = 4, CourseId = 2, Title = "ASP.NET Core - Primeros Pasos", Description = "Cómo crear tu primer proyecto en ASP.NET Core", Type = "Tutorial", Url = "https://example.com/aspnet-core-first-steps", PublicationDate = DateTime.UtcNow },
                new Content { Id = 5, CourseId = 3, Title = "SQL - Consultas Básicas", Description = "Aprender las consultas básicas en SQL", Type = "Document", Url = "https://example.com/sql-basics", PublicationDate = DateTime.UtcNow },
                new Content { Id = 6, CourseId = 3, Title = "PostgreSQL - Instalación y Configuración", Description = "Guía completa de instalación de PostgreSQL", Type = "Guide", Url = "https://example.com/postgresql-setup", PublicationDate = DateTime.UtcNow },
                new Content { Id = 7, CourseId = 4, Title = ".NET MAUI - Desarrollo Multiplataforma", Description = "Construir aplicaciones multiplataforma con .NET MAUI", Type = "Video", Url = "https://example.com/maui-intro", PublicationDate = DateTime.UtcNow },
                new Content { Id = 8, CourseId = 4, Title = "Flutter vs .NET MAUI - Comparación", Description = "Análisis comparativo entre frameworks móviles", Type = "Article", Url = "https://example.com/maui-flutter-comparison", PublicationDate = DateTime.UtcNow }
            };

            _context.Contents!.AddRange(contents);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✓ {contents.Length} contenidos creados.");
        }

        private async Task SeedEvaluationsAsync()
        {
            if (_context.Evaluations!.Any())
                return;

            var evaluations = new[]
            {
                new Evaluation { Id = 1, CourseId = 1, Title = "Quiz 1 - Álgebra Lineal", Description = "Primer cuestionario de álgebra lineal", Date = DateTime.UtcNow.AddDays(7), PercentageValue = 20.0 },
                new Evaluation { Id = 2, CourseId = 1, Title = "Examen Parcial", Description = "Examen parcial de la unidad 1", Date = DateTime.UtcNow.AddDays(14), PercentageValue = 40.0 },
                new Evaluation { Id = 3, CourseId = 1, Title = "Examen Final", Description = "Examen final del curso", Date = DateTime.UtcNow.AddDays(30), PercentageValue = 40.0 },
                new Evaluation { Id = 4, CourseId = 2, Title = "Proyecto 1 - Aplicación Console", Description = "Desarrollo de aplicación console en C#", Date = DateTime.UtcNow.AddDays(10), PercentageValue = 30.0 },
                new Evaluation { Id = 5, CourseId = 2, Title = "Proyecto 2 - API REST", Description = "Desarrollo de API REST con ASP.NET Core", Date = DateTime.UtcNow.AddDays(20), PercentageValue = 70.0 },
                new Evaluation { Id = 6, CourseId = 3, Title = "Tarea 1 - Consultas SQL", Description = "Realizar consultas SQL en PostgreSQL", Date = DateTime.UtcNow.AddDays(7), PercentageValue = 25.0 },
                new Evaluation { Id = 7, CourseId = 3, Title = "Tarea 2 - Procedimientos Almacenados", Description = "Crear procedimientos almacenados", Date = DateTime.UtcNow.AddDays(14), PercentageValue = 25.0 },
                new Evaluation { Id = 8, CourseId = 3, Title = "Examen Final", Description = "Examen final de bases de datos", Date = DateTime.UtcNow.AddDays(21), PercentageValue = 50.0 },
                new Evaluation { Id = 9, CourseId = 4, Title = "Proyecto Final - App MAUI", Description = "Desarrollar aplicación móvil con .NET MAUI", Date = DateTime.UtcNow.AddDays(30), PercentageValue = 100.0 }
            };

            _context.Evaluations!.AddRange(evaluations);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✓ {evaluations.Length} evaluaciones creadas.");
        }

        private async Task SeedGradesAsync()
        {
            if (_context.Grades!.Any())
                return;

            var grades = new[]
            {
                new Grade { Id = 1, EvaluationId = 1, StudentId = "user-student-001", Value = 18.5 },
                new Grade { Id = 2, EvaluationId = 1, StudentId = "user-student-002", Value = 19.0 },
                new Grade { Id = 3, EvaluationId = 2, StudentId = "user-student-001", Value = 35.5 },
                new Grade { Id = 4, EvaluationId = 2, StudentId = "user-student-002", Value = 38.0 },
                new Grade { Id = 5, EvaluationId = 3, StudentId = "user-student-001", Value = 38.0 },
                new Grade { Id = 6, EvaluationId = 3, StudentId = "user-student-002", Value = 39.5 },
                new Grade { Id = 7, EvaluationId = 4, StudentId = "user-student-001", Value = 28.5 },
                new Grade { Id = 8, EvaluationId = 4, StudentId = "user-student-003", Value = 27.0 },
                new Grade { Id = 9, EvaluationId = 5, StudentId = "user-student-001", Value = 65.0 },
                new Grade { Id = 10, EvaluationId = 5, StudentId = "user-student-003", Value = 63.5 },
                new Grade { Id = 11, EvaluationId = 6, StudentId = "user-student-002", Value = 23.0 },
                new Grade { Id = 12, EvaluationId = 6, StudentId = "user-student-003", Value = 24.5 },
                new Grade { Id = 13, EvaluationId = 7, StudentId = "user-student-002", Value = 24.0 },
                new Grade { Id = 14, EvaluationId = 7, StudentId = "user-student-003", Value = 25.0 },
                new Grade { Id = 15, EvaluationId = 8, StudentId = "user-student-002", Value = 47.5 },
                new Grade { Id = 16, EvaluationId = 8, StudentId = "user-student-003", Value = 49.0 }
            };

            _context.Grades!.AddRange(grades);
            await _context.SaveChangesAsync();
            Console.WriteLine($"✓ {grades.Length} calificaciones creadas.");
        }
    }
}
