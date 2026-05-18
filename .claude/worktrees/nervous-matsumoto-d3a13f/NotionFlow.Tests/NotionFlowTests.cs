using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NotionFlow.Api.Controllers;
using NotionFlow.Api.Data;
using NotionFlow.Api.DTOs;
using NotionFlow.Api.Models;
using System.Security.Claims;
using Xunit;

namespace NotionFlow.Tests;

// ============================================================
// HELPERS
// ============================================================

public static class DbHelper
{
    public static AppDbContext CreateInMemoryDb(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }
}

public static class ControllerHelper
{
    // Simula un usuario autenticado en el contexto HTTP del controlador
    public static void SetAuthenticatedUser(ControllerBase controller, string userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }
}

// ============================================================
// PRUEBAS - AuthController
// ============================================================

public class AuthControllerTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly AppDbContext _db;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var store = new Mock<IUserStore<User>>();
        _userManagerMock = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _configMock = new Mock<IConfiguration>();
        _configMock.Setup(c => c["Jwt:Key"]).Returns("superSecretKey123456789012345678");
        _configMock.Setup(c => c["Jwt:Issuer"]).Returns("NotionFlowApi");
        _configMock.Setup(c => c["Jwt:Audience"]).Returns("NotionFlowApp");

        _db = DbHelper.CreateInMemoryDb(Guid.NewGuid().ToString());
        _controller = new AuthController(_userManagerMock.Object, _configMock.Object, _db);

        // Sin usuario autenticado por defecto (registro no requiere auth)
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };
    }

    // ----------------------------------------------------------
    // Register - Happy Path: estudiante se registra correctamente
    // ----------------------------------------------------------
    [Fact]
    public async Task Register_StudentRole_ReturnsOk()
    {
        var dto = new RegisterDto("Ana López", "ana@test.com", "Pass123!", "Student", "");

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);

        _userManagerMock
            .Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Student"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.Register(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("User registered successfully", okResult.Value);
    }

    // ----------------------------------------------------------
    // Register - Flujo alternativo: Admin sin token válido
    // ----------------------------------------------------------
    [Fact]
    public async Task Register_AdminWithoutToken_ReturnsBadRequest()
    {
        var dto = new RegisterDto("Carlos Admin", "admin@test.com", "Pass123!", "Admin", "WRONG_TOKEN");

        var result = await _controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid administrator token", badRequest.Value);
    }

    // ----------------------------------------------------------
    // Register - Flujo alternativo: Profesor sin token de admin
    // ----------------------------------------------------------
    [Fact]
    public async Task Register_ProfessorWithoutAdminToken_ReturnsBadRequest()
    {
        var dto = new RegisterDto("Prof. Gómez", "prof@test.com", "Pass123!", "Professor", "");

        var result = await _controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Only an administrator can create professors", badRequest.Value);
    }

    // ----------------------------------------------------------
    // Register - Flujo alternativo: error en creación de usuario
    // ----------------------------------------------------------
    [Fact]
    public async Task Register_IdentityError_ReturnsBadRequest()
    {
        var dto = new RegisterDto("Ana", "ana@test.com", "weak", "Student", "");
        var identityError = new IdentityError { Description = "Password too weak" };

        _userManagerMock
            .Setup(m => m.CreateAsync(It.IsAny<User>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        var result = await _controller.Register(dto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ----------------------------------------------------------
    // Login - Happy Path: credenciales correctas devuelven token
    // ----------------------------------------------------------
    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var dto = new LoginDto("ana@test.com", "Pass123!");
        var user = new User { Id = "1", Name = "Ana", Email = "ana@test.com", UserName = "ana@test.com", Role = "Student" };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(true);

        var result = await _controller.Login(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<AuthResponseDto>(okResult.Value);
        Assert.Equal("Ana", response.Name);
        Assert.False(string.IsNullOrEmpty(response.Token));
    }

    // ----------------------------------------------------------
    // Login - Flujo alternativo: usuario no existe
    // ----------------------------------------------------------
    [Fact]
    public async Task Login_UserNotFound_ReturnsUnauthorized()
    {
        var dto = new LoginDto("noexiste@test.com", "Pass123!");

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync((User?)null);

        var result = await _controller.Login(dto);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials", unauthorized.Value);
    }

    // ----------------------------------------------------------
    // Login - Flujo alternativo: contraseña incorrecta
    // ----------------------------------------------------------
    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var dto = new LoginDto("ana@test.com", "WrongPass!");
        var user = new User { Id = "1", Name = "Ana", Email = "ana@test.com", Role = "Student" };

        _userManagerMock
            .Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(user);

        _userManagerMock
            .Setup(m => m.CheckPasswordAsync(user, dto.Password))
            .ReturnsAsync(false);

        var result = await _controller.Login(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // ----------------------------------------------------------
    // Register - Happy Path: rol "Teacher" se mapea a "Professor"
    // ----------------------------------------------------------
    [Fact]
    public async Task Register_TeacherRole_MappedToProfessor_RequiresAdminToken()
    {
        var dto = new RegisterDto("Prof", "prof@test.com", "Pass123!", "Teacher", "");

        var result = await _controller.Register(dto);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Only an administrator can create professors", badRequest.Value);
    }
}

// ============================================================
// PRUEBAS - CoursesController
// ============================================================

public class CoursesControllerTests
{
    private AppDbContext CreateDb() =>
        DbHelper.CreateInMemoryDb(Guid.NewGuid().ToString());

    private Mock<UserManager<User>> CreateUserManagerMock() =>
        new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            null!, null!, null!, null!, null!, null!, null!, null!);

    // ----------------------------------------------------------
    // CreateCourse - Happy Path: admin autenticado crea curso
    // ----------------------------------------------------------
    [Fact]
    public async Task CreateCourse_AuthenticatedAdmin_ReturnsOk()
    {
        var db = CreateDb();
        var userManagerMock = CreateUserManagerMock();

        var adminUser = new User { Id = "admin-1", Name = "Admin", Email = "admin@test.com", Role = "Admin", InstitutionId = 1 };
        var teacher = new User { Id = "teacher-1", Name = "Prof. García", Email = "garcia@test.com", Role = "Professor", InstitutionId = 1 };

        userManagerMock.Setup(m => m.FindByIdAsync("admin-1")).ReturnsAsync(adminUser);
        userManagerMock.Setup(m => m.FindByIdAsync("teacher-1")).ReturnsAsync(teacher);

        var controller = new CoursesController(db, userManagerMock.Object);
        ControllerHelper.SetAuthenticatedUser(controller, "admin-1");

        var dto = new CreateCourseDto("Matemáticas", "Álgebra Lineal", "Curso de álgebra", "teacher-1");

        var result = await controller.CreateCourse(dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    // ----------------------------------------------------------
    // CreateCourse - Flujo alternativo: usuario no autenticado
    // ----------------------------------------------------------
    [Fact]
    public async Task CreateCourse_NotAuthenticated_ReturnsUnauthorized()
    {
        var db = CreateDb();
        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);

        // Sin usuario autenticado
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
        };

        var dto = new CreateCourseDto("Matemáticas", "Álgebra", "Desc", "teacher-1");

        var result = await controller.CreateCourse(dto);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    // ----------------------------------------------------------
    // AssignStudent - Happy Path: estudiante asignado correctamente
    // ----------------------------------------------------------
    [Fact]
    public async Task AssignStudent_ValidStudentAndCourse_ReturnsOk()
    {
        var db = CreateDb();
        db.Courses.Add(new Course { Id = 1, Name = "Matemáticas", Subject = "Álgebra", Description = "Curso", InstitutionId = 1 });
        await db.SaveChangesAsync();

        var userManagerMock = CreateUserManagerMock();
        var student = new User { Id = "student-1", Name = "Ana", Email = "ana@test.com", Role = "Student", InstitutionId = 1 };
        userManagerMock.Setup(m => m.FindByIdAsync("student-1")).ReturnsAsync(student);

        var controller = new CoursesController(db, userManagerMock.Object);
        var dto = new AssignStudentDto("student-1");

        var result = await controller.AssignStudent(1, dto);

        Assert.IsType<OkObjectResult>(result);
    }

    // ----------------------------------------------------------
    // AssignStudent - Flujo alternativo: estudiante no existe
    // ----------------------------------------------------------
    [Fact]
    public async Task AssignStudent_StudentNotFound_ReturnsNotFound()
    {
        var db = CreateDb();
        db.Courses.Add(new Course { Id = 2, Name = "Historia", Subject = "Universal", Description = "Curso", InstitutionId = 1 });
        await db.SaveChangesAsync();

        var userManagerMock = CreateUserManagerMock();
        userManagerMock.Setup(m => m.FindByIdAsync("student-inexistente")).ReturnsAsync((User?)null);

        var controller = new CoursesController(db, userManagerMock.Object);
        var dto = new AssignStudentDto("student-inexistente");

        var result = await controller.AssignStudent(2, dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ----------------------------------------------------------
    // AssignStudent - Flujo alternativo: curso no existe
    // ----------------------------------------------------------
    [Fact]
    public async Task AssignStudent_CourseNotFound_ReturnsNotFound()
    {
        var db = CreateDb();
        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);
        var dto = new AssignStudentDto("student-1");

        var result = await controller.AssignStudent(999, dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ----------------------------------------------------------
    // SaveGrade - Happy Path: guardar nota nueva
    // ----------------------------------------------------------
    [Fact]
    public async Task SaveGrade_NewGrade_ReturnsOkWithGrade()
    {
        var db = CreateDb();
        db.Evaluations.Add(new Evaluation { Id = 1, CourseId = 1, Title = "Parcial 1", Description = "Primer parcial", Date = DateTime.UtcNow, PercentageValue = 30 });
        await db.SaveChangesAsync();

        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);
        var dto = new SaveGradeDto("student-1", 4.5);

        var result = await controller.SaveGrade(1, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var grade = Assert.IsType<Grade>(okResult.Value);
        Assert.Equal(4.5, grade.Value);
    }

    // ----------------------------------------------------------
    // SaveGrade - Flujo alternativo: actualizar nota existente
    // ----------------------------------------------------------
    [Fact]
    public async Task SaveGrade_ExistingGrade_UpdatesValue()
    {
        var db = CreateDb();
        db.Evaluations.Add(new Evaluation { Id = 2, CourseId = 1, Title = "Final", Description = "Examen final", Date = DateTime.UtcNow, PercentageValue = 40 });
        db.Grades.Add(new Grade { EvaluationId = 2, StudentId = "student-1", Value = 3.0 });
        await db.SaveChangesAsync();

        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);
        var dto = new SaveGradeDto("student-1", 4.8);

        var result = await controller.SaveGrade(2, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var grade = Assert.IsType<Grade>(okResult.Value);
        Assert.Equal(4.8, grade.Value);
    }

    // ----------------------------------------------------------
    // GetEvaluations - Happy Path: retorna evaluaciones del curso
    // ----------------------------------------------------------
    [Fact]
    public async Task GetEvaluations_ExistingCourse_ReturnsEvaluations()
    {
        var db = CreateDb();
        db.Evaluations.AddRange(
            new Evaluation { Id = 3, CourseId = 5, Title = "Quiz 1", Description = "Primer quiz", Date = DateTime.UtcNow, PercentageValue = 10 },
            new Evaluation { Id = 4, CourseId = 5, Title = "Quiz 2", Description = "Segundo quiz", Date = DateTime.UtcNow, PercentageValue = 10 }
        );
        await db.SaveChangesAsync();

        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);

        var result = await controller.GetEvaluations(5);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var evaluations = Assert.IsAssignableFrom<IEnumerable<Evaluation>>(okResult.Value);
        Assert.Equal(2, evaluations.Count());
    }

    // ----------------------------------------------------------
    // GetEvaluations - Flujo alternativo: curso sin evaluaciones
    // ----------------------------------------------------------
    [Fact]
    public async Task GetEvaluations_NoCourseEvaluations_ReturnsEmptyList()
    {
        var db = CreateDb();
        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);

        var result = await controller.GetEvaluations(999);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var evaluations = Assert.IsAssignableFrom<IEnumerable<Evaluation>>(okResult.Value);
        Assert.Empty(evaluations);
    }

    // ----------------------------------------------------------
    // PublishContent - Happy Path: publicar contenido en un curso
    // ----------------------------------------------------------
    [Fact]
    public async Task PublishContent_ValidData_ReturnsOkWithContent()
    {
        var db = CreateDb();
        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);
        var dto = new PublishContentDto("Clase 1", "Introducción al álgebra", "Video", "https://youtube.com/xyz");

        var result = await controller.PublishContent(10, dto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var content = Assert.IsType<Content>(okResult.Value);
        Assert.Equal("Clase 1", content.Title);
        Assert.Equal(10, content.CourseId);
    }

    // ----------------------------------------------------------
    // GetContents - Flujo alternativo: curso sin contenidos
    // ----------------------------------------------------------
    [Fact]
    public async Task GetContents_NoCourseContents_ReturnsEmptyList()
    {
        var db = CreateDb();
        var userManagerMock = CreateUserManagerMock();
        var controller = new CoursesController(db, userManagerMock.Object);

        var result = await controller.GetContents(999);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var contents = Assert.IsAssignableFrom<IEnumerable<Content>>(okResult.Value);
        Assert.Empty(contents);
    }
}
