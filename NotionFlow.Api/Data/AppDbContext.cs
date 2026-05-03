using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NotionFlow.Api.Models;

namespace NotionFlow.Api.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Nuevas entidades para arquitectura multi-institución
        public DbSet<Institution> Institutions => Set<Institution>();
        public DbSet<InstitutionAdministrator> InstitutionAdministrators => Set<InstitutionAdministrator>();
        public DbSet<CourseTeacher> CourseTeachers => Set<CourseTeacher>();

        // Entidades existentes
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseStudent> CourseStudents => Set<CourseStudent>();
        public DbSet<Evaluation> Evaluations => Set<Evaluation>();
        public DbSet<Grade> Grades => Set<Grade>();
        public DbSet<Content> Contents => Set<Content>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<ActivityQuestion> ActivityQuestions => Set<ActivityQuestion>();
        public DbSet<ActivityOption> ActivityOptions => Set<ActivityOption>();
        public DbSet<ActivityAssignment> ActivityAssignments => Set<ActivityAssignment>();
        public DbSet<StudentAnswer> StudentAnswers => Set<StudentAnswer>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ========== CONFIGURACIÓN: Institution ==========
            builder.Entity<Institution>()
                .HasMany(i => i.Users)
                .WithOne(u => u.Institution)
                .HasForeignKey(u => u.InstitutionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Institution>()
                .HasMany(i => i.Courses)
                .WithOne(c => c.Institution)
                .HasForeignKey(c => c.InstitutionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Institution>()
                .HasMany(i => i.Administrators)
                .WithOne(ia => ia.Institution)
                .HasForeignKey(ia => ia.InstitutionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Institution>()
                .HasIndex(i => i.RegistrationCode)
                .IsUnique();

            // ========== CONFIGURACIÓN: InstitutionAdministrator ==========
            builder.Entity<InstitutionAdministrator>()
                .HasOne(ia => ia.User)
                .WithMany(u => u.AdministratorOf)
                .HasForeignKey(ia => ia.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== CONFIGURACIÓN: CourseTeacher ==========
            builder.Entity<CourseTeacher>()
                .HasOne(ct => ct.Course)
                .WithMany(c => c.Teachers)
                .HasForeignKey(ct => ct.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CourseTeacher>()
                .HasOne(ct => ct.Teacher)
                .WithMany(u => u.TaughtCourses)
                .HasForeignKey(ct => ct.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== CONFIGURACIÓN: CourseStudent ==========
            builder.Entity<CourseStudent>()
                .HasKey(cs => new { cs.CourseId, cs.StudentId });

            builder.Entity<CourseStudent>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseStudents)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== CONFIGURACIÓN: Evaluation ==========
            builder.Entity<Evaluation>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Evaluations)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== CONFIGURACIÓN: Content ==========
            builder.Entity<Content>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Contents)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ========== CONFIGURACIÓN: Activity ==========
            builder.Entity<Activity>()
                .HasOne(a => a.Course)
                .WithMany()
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ActivityQuestion>()
                .HasOne(q => q.Activity)
                .WithMany(a => a.Questions)
                .HasForeignKey(q => q.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ActivityOption>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ActivityAssignment>()
                .HasOne(a => a.Activity)
                .WithMany(act => act.Assignments)
                .HasForeignKey(a => a.ActivityId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Assignment)
                .WithMany()
                .HasForeignKey(sa => sa.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Question)
                .WithMany()
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // ========== CONFIGURACIÓN: Grade ==========
            builder.Entity<Grade>()
                .HasOne(g => g.Evaluation)
                .WithMany(e => e.Grades)
                .HasForeignKey(g => g.EvaluationId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
