using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Data
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(
            AppDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DataSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("Iniciando proceso de seeding...");

                    await CleanAndResetDatabaseAsync();

                    await SeedRolesAsync();
                    await SeedInstitutionsAsync();
                    await SeedUsersAsync();

                    await SeedInstitutionAdministratorsAsync();
                    await SeedCoursesAsync();
                    await SeedCourseTeachersAsync();
                    await SeedCourseStudentsAsync();

                    await SeedContentsAsync();
                    await SeedEvaluationsAsync();
                    await SeedGradesAsync();

                    await ResetSequencesAsync();

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Seed completado exitosamente");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error crítico durante el seed");
                    throw;
                }
            });
        }

        private async Task CleanAndResetDatabaseAsync()
        {
            _logger.LogDebug("Limpiando tablas...");
            await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'replica'");

            var tables = new[]
            {
                "Grades", "Evaluations", "Contents", "CourseStudents", "CourseTeachers",
                "Courses", "InstitutionAdministrators", "AspNetUserRoles", "AspNetUsers", "Institutions"
            };

            foreach (var table in tables)
            {
#pragma warning disable EF1002
                await _context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{table}\" RESTART IDENTITY CASCADE");
#pragma warning restore EF1002
            }

            await _context.Database.ExecuteSqlRawAsync("SET session_replication_role = 'origin'");
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "Professor", "Student" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));
            }
            _logger.LogDebug("Roles creados");
        }

        private async Task SeedInstitutionsAsync()
        {
            var institutions = new[]
            {
                new Institution { Id = 1, Name = "Instituto Educativo NotionFlow", Email = "info@notionflow.edu.com", Phone = "+1 555 0001", Address = "Calle Principal 123", City = "Ciudad Educativa", Country = "País", RegistrationCode = "NF-2024-001", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Institution { Id = 2, Name = "Academia de Tecnología Digital", Email = "contact@techacademy.edu.com", Phone = "+1 555 0002", Address = "Avenida Tecnológica 456", City = "Tech City", Country = "País", RegistrationCode = "TA-2024-002", CreatedAt = DateTime.UtcNow, IsActive = true }
            };

            _context.Institutions!.AddRange(institutions);
            await _context.SaveChangesAsync();
            _logger.LogDebug("Instituciones creadas");
        }

        private async Task SeedUsersAsync()
        {
            var users = new[]
            {
                new { Id = "user-admin-001", Name = "Admin Sistema", Email = "admin@notionflow.com", Role = "Admin", InstitutionId = 1 },
                new { Id = "user-teacher-001", Name = "María García", Email = "maria.garcia@notionflow.com", Role = "Professor", InstitutionId = 1 },
                new { Id = "user-student-001", Name = "Ana Rodríguez", Email = "ana.rodriguez@notionflow.com", Role = "Student", InstitutionId = 1 },
                new { Id = "user-admin-002", Name = "Director Técnico", Email = "director@techacademy.com", Role = "Admin", InstitutionId = 2 }
            };

            foreach (var u in users)
            {
                var existingUser = await _userManager.FindByEmailAsync(u.Email);
                if (existingUser != null)
                {
                    _logger.LogDebug("Usuario {Email} ya existe, omitiendo", u.Email);
                    continue;
                }

                var user = new User
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    UserName = u.Email,
                    NormalizedEmail = u.Email.ToUpper(),
                    NormalizedUserName = u.Email.ToUpper(),
                    EmailConfirmed = true,
                    Role = u.Role,
                    InstitutionId = u.InstitutionId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, "NotionFlow123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, u.Role);
                    _logger.LogDebug("Usuario creado: {Email} ({Role})", u.Email, u.Role);
                }
                else
                {
                    _logger.LogWarning("Error creando {Email}: {Errors}", u.Email,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            _logger.LogDebug("Usuarios e Identity configurados");
        }

        private async Task SeedInstitutionAdministratorsAsync()
        {
            _context.InstitutionAdministrators!.AddRange(
                new InstitutionAdministrator { Id = 1, InstitutionId = 1, UserId = "user-admin-001", IsOwner = true, AssignedAt = DateTime.UtcNow },
                new InstitutionAdministrator { Id = 2, InstitutionId = 2, UserId = "user-admin-002", IsOwner = true, AssignedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedCoursesAsync()
        {
            _context.Courses!.AddRange(
                new Course { Id = 1, InstitutionId = 1, Name = "Matemáticas Avanzadas", Subject = "Matemáticas", Description = "Curso avanzado", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Course { Id = 2, InstitutionId = 1, Name = "Programación con .NET", Subject = "Programación", Description = "C# y ASP.NET", CreatedAt = DateTime.UtcNow, IsActive = true }
            );
            await _context.SaveChangesAsync();
            _logger.LogDebug("Cursos creados");
        }

        private async Task SeedCourseTeachersAsync()
        {
            _context.CourseTeachers!.AddRange(
                new CourseTeacher { Id = 1, CourseId = 1, TeacherId = "user-teacher-001", IsPrimary = true, AssignedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedCourseStudentsAsync()
        {
            _context.CourseStudents!.AddRange(
                new CourseStudent { CourseId = 1, StudentId = "user-student-001" }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedContentsAsync()
        {
            _context.Contents!.AddRange(
                new Content { Id = 1, CourseId = 1, Title = "Intro Álgebra", Description = "Video inicial", Type = "Video", Url = "http://link.com", PublicationDate = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedEvaluationsAsync()
        {
            _context.Evaluations!.AddRange(
                new Evaluation { Id = 1, CourseId = 1, Title = "Parcial 1", Description = "Examen", Date = DateTime.UtcNow.AddDays(7), PercentageValue = 100.0 }
            );
            await _context.SaveChangesAsync();
        }

        private async Task SeedGradesAsync()
        {
            _context.Grades!.AddRange(
                new Grade { Id = 1, EvaluationId = 1, StudentId = "user-student-001", Value = 18.0 }
            );
            await _context.SaveChangesAsync();
        }

        private async Task ResetSequencesAsync()
        {
            _logger.LogDebug("Sincronizando secuencias de PostgreSQL...");
            var tables = new[] { "Institutions", "Courses", "Contents", "CourseTeachers", "Evaluations", "Grades", "InstitutionAdministrators" };

            foreach (var table in tables)
            {
                try
                {
                    var sql = $@"
                        DO $$
                        DECLARE
                            v_max_id INTEGER;
                            v_seq_name TEXT;
                        BEGIN
                            v_seq_name := pg_get_serial_sequence('""{table}""', 'Id');
                            IF v_seq_name IS NOT NULL THEN
                                v_max_id := COALESCE((SELECT MAX(""Id"") FROM ""{table}""), 0);
                                PERFORM setval(v_seq_name, v_max_id + 1);
                            END IF;
                        END $$;";

                    await _context.Database.ExecuteSqlRawAsync(sql);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo sincronizar secuencia de {Table}", table);
                }
            }
            _logger.LogDebug("Secuencias sincronizadas");
        }
    }
}
