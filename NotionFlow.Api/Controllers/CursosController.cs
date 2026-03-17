using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotionFlow.Api.Data;
using NotionFlow.Api.DTOs;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CursosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Usuario> _userManager;

        public CursosController(AppDbContext db, UserManager<Usuario> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CrearCurso(CrearCursoDto dto)
        {
            var profesor = await _userManager.FindByIdAsync(dto.ProfesorId);
            var curso = new Curso
            {
                Nombre = dto.Nombre,
                Materia = dto.Materia,
                ProfesorId = dto.ProfesorId,
                ProfesorNombre = profesor?.Nombre ?? string.Empty
            };
            _db.Cursos.Add(curso);
            await _db.SaveChangesAsync();
            return Ok(curso);
        }

        [HttpPost("{cursoId}/estudiantes")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AsignarEstudiante(int cursoId, AsignarEstudianteDto dto)
        {
            var existe = await _db.CursoEstudiantes
                .AnyAsync(ce => ce.CursoId == cursoId && ce.EstudianteId == dto.EstudianteId);

            if (existe) return BadRequest("El estudiante ya está en el curso");

            _db.CursoEstudiantes.Add(new CursoEstudiante
            {
                CursoId = cursoId,
                EstudianteId = dto.EstudianteId
            });
            await _db.SaveChangesAsync();
            return Ok("Estudiante asignado");
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ObtenerTodos()
        {
            var cursos = await _db.Cursos
                .Include(c => c.CursoEstudiantes)
                .ToListAsync();

            var usuarios = await _userManager.Users.ToListAsync();

            var resultado = cursos.Select(c => new CursoResponseDto(
                c.Id,
                c.Nombre,
                c.Materia,
                c.ProfesorId,
                usuarios.FirstOrDefault(u => u.Id == c.ProfesorId)?.Nombre ?? "Sin profesor",
                c.CursoEstudiantes.Select(ce => new EstudianteDto(
                    ce.EstudianteId,
                    usuarios.FirstOrDefault(u => u.Id == ce.EstudianteId)?.Nombre ?? "",
                    usuarios.FirstOrDefault(u => u.Id == ce.EstudianteId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(resultado);
        }

        [HttpGet("profesor/{profesorId}")]
        [Authorize(Roles = "Admin,Profesor")]
        public async Task<IActionResult> CursosProfesor(string profesorId)
        {
            var cursos = await _db.Cursos
                .Where(c => c.ProfesorId == profesorId)
                .Include(c => c.CursoEstudiantes)
                .ToListAsync();

            var usuarios = await _userManager.Users.ToListAsync();

            var resultado = cursos.Select(c => new CursoResponseDto(
                c.Id,
                c.Nombre,
                c.Materia,
                c.ProfesorId,
                usuarios.FirstOrDefault(u => u.Id == c.ProfesorId)?.Nombre ?? "Sin profesor",
                c.CursoEstudiantes.Select(ce => new EstudianteDto(
                    ce.EstudianteId,
                    usuarios.FirstOrDefault(u => u.Id == ce.EstudianteId)?.Nombre ?? "",
                    usuarios.FirstOrDefault(u => u.Id == ce.EstudianteId)?.Email ?? ""
                )).ToList()
            ));

            return Ok(resultado);
        }

        [HttpGet("estudiante/{estudianteId}")]
        [Authorize(Roles = "Admin,Estudiante")]
        public async Task<IActionResult> CursosEstudiante(string estudianteId)
        {
            var cursos = await _db.CursoEstudiantes
                .Where(ce => ce.EstudianteId == estudianteId)
                .Include(ce => ce.Curso)
                .Select(ce => ce.Curso)
                .ToListAsync();

            var usuarios = await _userManager.Users.ToListAsync();

            var resultado = cursos.Select(c => new CursoResponseDto(
                c!.Id,
                c.Nombre,
                c.Materia,
                c.ProfesorId,
                usuarios.FirstOrDefault(u => u.Id == c.ProfesorId)?.Nombre ?? "Sin profesor",
                new List<EstudianteDto>()
            ));

            return Ok(resultado);
        }

        [HttpGet("{cursoId}/evaluaciones")]
        public async Task<IActionResult> ObtenerEvaluaciones(int cursoId)
        {
            var evaluaciones = await _db.Evaluaciones
                .Where(e => e.CursoId == cursoId)
                .ToListAsync();
            return Ok(evaluaciones);
        }

        [HttpPost("{cursoId}/evaluaciones")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> CrearEvaluacion(int cursoId, CrearEvaluacionDto dto)
        {
            var evaluacion = new Evaluacion
            {
                CursoId = cursoId,
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Fecha = dto.Fecha,
                PorcentajeValor = dto.PorcentajeValor
            };
            _db.Evaluaciones.Add(evaluacion);
            await _db.SaveChangesAsync();
            return Ok(evaluacion);
        }

        [HttpPost("evaluaciones/{evaluacionId}/notas")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> GuardarNota(int evaluacionId, GuardarNotaDto dto)
        {
            var nota = await _db.Notas
                .FirstOrDefaultAsync(n => n.EvaluacionId == evaluacionId
                    && n.EstudianteId == dto.EstudianteId);

            if (nota == null)
            {
                nota = new Nota
                {
                    EvaluacionId = evaluacionId,
                    EstudianteId = dto.EstudianteId,
                    Valor = dto.Valor
                };
                _db.Notas.Add(nota);
            }
            else
            {
                nota.Valor = dto.Valor;
            }

            await _db.SaveChangesAsync();
            return Ok(nota);
        }

        [HttpGet("{cursoId}/contenidos")]
        public async Task<IActionResult> ObtenerContenidos(int cursoId)
        {
            var contenidos = await _db.Contenidos
                .Where(c => c.CursoId == cursoId)
                .OrderByDescending(c => c.FechaPublicacion)
                .ToListAsync();
            return Ok(contenidos);
        }

        [HttpPost("{cursoId}/contenidos")]
        [Authorize(Roles = "Profesor")]
        public async Task<IActionResult> PublicarContenido(int cursoId, PublicarContenidoDto dto)
        {
            var contenido = new Contenido
            {
                CursoId = cursoId,
                Titulo = dto.Titulo,
                Descripcion = dto.Descripcion,
                Tipo = dto.Tipo,
                Url = dto.Url,
                FechaPublicacion = DateTime.UtcNow
            };
            _db.Contenidos.Add(contenido);
            await _db.SaveChangesAsync();
            return Ok(contenido);
        }
    }
}