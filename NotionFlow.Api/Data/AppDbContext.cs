using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Data
{
    public class AppDbContext : IdentityDbContext<Usuario>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Curso> Cursos => Set<Curso>();
        public DbSet<CursoEstudiante> CursoEstudiantes => Set<CursoEstudiante>();
        public DbSet<Evaluacion> Evaluaciones => Set<Evaluacion>();
        public DbSet<Nota> Notas => Set<Nota>();
        public DbSet<Contenido> Contenidos => Set<Contenido>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CursoEstudiante>()
                .HasKey(ce => new { ce.CursoId, ce.EstudianteId });

            builder.Entity<CursoEstudiante>()
                .HasOne(ce => ce.Curso)
                .WithMany(c => c.CursoEstudiantes)
                .HasForeignKey(ce => ce.CursoId);
        }
    }
}