using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseStudent> CourseStudents => Set<CourseStudent>();
        public DbSet<Evaluation> Evaluations => Set<Evaluation>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Content> Contents => Set<Content>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<CourseStudent>()
                .HasKey(ce => new { ce.CourseId, ce.StudentId });

            builder.Entity<CourseStudent>()
                .HasOne(ce => ce.Course)
                .WithMany(c => c.CourseStudents)
                .HasForeignKey(ce => ce.CourseId);
        }
    }
}