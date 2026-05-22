using NotionFlow.App.Models;
using System.Diagnostics;
using System.Text.Json;

namespace NotionFlow.App.Services;

public class LocalDataService
{
    private readonly string _dataFolderPath;
    private readonly string _usersFile;
    private readonly string _institutionsFile;
    private readonly string _coursesFile;
    private readonly string _contentsFile;
    private readonly string _evaluationsFile;
    private readonly string _gradesFile;
    private readonly string _courseTeachersFile;
    private readonly string _courseStudentsFile;

    public LocalDataService()
    {
        _dataFolderPath = Path.Combine(FileSystem.AppDataDirectory, "NotionFlowData");
        
        if (!Directory.Exists(_dataFolderPath))
            Directory.CreateDirectory(_dataFolderPath);

        _usersFile = Path.Combine(_dataFolderPath, "users.json");
        _institutionsFile = Path.Combine(_dataFolderPath, "institutions.json");
        _coursesFile = Path.Combine(_dataFolderPath, "courses.json");
        _contentsFile = Path.Combine(_dataFolderPath, "contents.json");
        _evaluationsFile = Path.Combine(_dataFolderPath, "evaluations.json");
        _gradesFile = Path.Combine(_dataFolderPath, "grades.json");
        _courseTeachersFile = Path.Combine(_dataFolderPath, "courseTeachers.json");
        _courseStudentsFile = Path.Combine(_dataFolderPath, "courseStudents.json");
    }

    public async Task InitializeAsync()
    {
        try
        {
            Debug.WriteLine("Inicializando datos locales JSON...");

            if (File.Exists(_usersFile))
            {
                Debug.WriteLine("Datos ya existen");
                return;
            }

            await LoadInitialDataAsync();
            Debug.WriteLine("Datos iniciales cargados");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    private async Task LoadInitialDataAsync()
    {
        try
        {
            var institutions = new List<InstitutionLocal>
            {
                new InstitutionLocal
                {
                    Id = 1,
                    Name = "Instituto Educativo NotionFlow",
                    Email = "info@notionflow.edu.com",
                    Phone = "+1 555 0001",
                    Address = "Calle Principal 123",
                    City = "Ciudad Educativa",
                    Country = "País",
                    RegistrationCode = "NF-2024-001",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new InstitutionLocal
                {
                    Id = 2,
                    Name = "Academia de Tecnología Digital",
                    Email = "contact@techacademy.edu.com",
                    Phone = "+1 555 0002",
                    Address = "Avenida Tecnológica 456",
                    City = "Tech City",
                    Country = "País",
                    RegistrationCode = "TA-2024-002",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };
            await SaveToJsonAsync(_institutionsFile, institutions);

            var users = new List<UserLocal>
            {
                new UserLocal
                {
                    Id = "user-admin-001",
                    Name = "Admin Sistema",
                    Email = "admin@notionflow.com",
                    Role = "Admin",
                    InstitutionId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new UserLocal
                {
                    Id = "user-teacher-001",
                    Name = "María García",
                    Email = "maria.garcia@notionflow.com",
                    Role = "Professor",
                    InstitutionId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new UserLocal
                {
                    Id = "user-student-001",
                    Name = "Ana Rodríguez",
                    Email = "ana.rodriguez@notionflow.com",
                    Role = "Student",
                    InstitutionId = 1,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new UserLocal
                {
                    Id = "user-admin-002",
                    Name = "Director Técnico",
                    Email = "director@techacademy.com",
                    Role = "Admin",
                    InstitutionId = 2,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };
            await SaveToJsonAsync(_usersFile, users);

            var courses = new List<CourseLocal>
            {
                new CourseLocal
                {
                    Id = 1,
                    InstitutionId = 1,
                    Name = "Matemáticas Avanzadas",
                    Subject = "Matemáticas",
                    Description = "Curso avanzado",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                },
                new CourseLocal
                {
                    Id = 2,
                    InstitutionId = 1,
                    Name = "Programación con .NET",
                    Subject = "Programación",
                    Description = "C# y ASP.NET",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                }
            };
            await SaveToJsonAsync(_coursesFile, courses);

            var courseTeachers = new List<CourseTeacherLocal>
            {
                new CourseTeacherLocal
                {
                    Id = 1,
                    CourseId = 1,
                    TeacherId = "user-teacher-001",
                    IsPrimary = true,
                    AssignedAt = DateTime.UtcNow
                }
            };
            await SaveToJsonAsync(_courseTeachersFile, courseTeachers);

            var courseStudents = new List<CourseStudentLocal>
            {
                new CourseStudentLocal
                {
                    Id = 1,
                    CourseId = 1,
                    StudentId = "user-student-001"
                }
            };
            await SaveToJsonAsync(_courseStudentsFile, courseStudents);

            var contents = new List<ContentLocal>
            {
                new ContentLocal
                {
                    Id = 1,
                    CourseId = 1,
                    Title = "Intro Álgebra",
                    Description = "Video inicial",
                    Type = "Video",
                    Url = "http://link.com",
                    PublicationDate = DateTime.UtcNow
                }
            };
            await SaveToJsonAsync(_contentsFile, contents);

            var evaluations = new List<EvaluationLocal>
            {
                new EvaluationLocal
                {
                    Id = 1,
                    CourseId = 1,
                    Title = "Parcial 1",
                    Description = "Examen",
                    Date = DateTime.UtcNow.AddDays(7),
                    PercentageValue = 100.0
                }
            };
            await SaveToJsonAsync(_evaluationsFile, evaluations);

            var grades = new List<GradeLocal>
            {
                new GradeLocal
                {
                    Id = 1,
                    EvaluationId = 1,
                    StudentId = "user-student-001",
                    Value = 18.0
                }
            };
            await SaveToJsonAsync(_gradesFile, grades);

            Debug.WriteLine("Todos los datos cargados");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            throw;
        }
    }

    public async Task<List<CourseLocal>> GetStudentCoursesAsync(string studentId)
    {
        try
        {
            var courseStudents = await LoadFromJsonAsync<CourseStudentLocal>(_courseStudentsFile) ?? new();
            var courses = await LoadFromJsonAsync<CourseLocal>(_coursesFile) ?? new();

            var enrolledCourseIds = courseStudents
                .Where(cs => cs.StudentId == studentId)
                .Select(cs => cs.CourseId)
                .ToList();

            return courses.Where(c => enrolledCourseIds.Contains(c.Id)).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<GradeLocal>> GetStudentGradesAsync(string studentId)
    {
        try
        {
            var grades = await LoadFromJsonAsync<GradeLocal>(_gradesFile) ?? new();
            return grades.Where(g => g.StudentId == studentId).ToList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
            return new();
        }
    }

    public async Task<List<CourseLocal>> GetAllCoursesAsync()
    {
        return await LoadFromJsonAsync<CourseLocal>(_coursesFile) ?? new();
    }

    public async Task<List<UserLocal>> GetAllUsersAsync()
    {
        return await LoadFromJsonAsync<UserLocal>(_usersFile) ?? new();
    }

    public async Task SaveGradeAsync(GradeLocal grade)
    {
        try
        {
            var grades = await LoadFromJsonAsync<GradeLocal>(_gradesFile) ?? new();
            grades.Add(grade);
            await SaveToJsonAsync(_gradesFile, grades);
            Debug.WriteLine("Calificación guardada");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    public async Task SaveCourseAsync(CourseLocal course)
    {
        try
        {
            var courses = await LoadFromJsonAsync<CourseLocal>(_coursesFile) ?? new();
            courses.Add(course);
            await SaveToJsonAsync(_coursesFile, courses);
            Debug.WriteLine("Curso guardado");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    private async Task SaveToJsonAsync<T>(string filePath, List<T> data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(filePath, json);
    }

    private async Task<List<T>> LoadFromJsonAsync<T>(string filePath)
    {
        if (!File.Exists(filePath))
            return new();

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<T>>(json) ?? new();
    }

    public async Task ClearDataAsync()
    {
        try
        {
            if (Directory.Exists(_dataFolderPath))
                Directory.Delete(_dataFolderPath, true);
            
            await InitializeAsync();
            Debug.WriteLine("Datos limpiados");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
    }
}