using Microsoft.EntityFrameworkCore;
using NotionFlow.App.Models;

namespace NotionFlow.App.Data;

public class LocalDbContext : DbContext
{
    private readonly string _dbPath;

    public LocalDbContext()
    {
        _dbPath = Path.Combine(
            FileSystem.AppDataDirectory,
            "notionflow_local.db"
        );


        var dir = Path.GetDirectoryName(_dbPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    public DbSet<UserLocal> Users { get; set; }
    public DbSet<InstitutionLocal> Institutions { get; set; }
    public DbSet<CourseLocal> Courses { get; set; }
    public DbSet<ContentLocal> Contents { get; set; }
    public DbSet<EvaluationLocal> Evaluations { get; set; }
    public DbSet<GradeLocal> Grades { get; set; }
    public DbSet<CourseTeacherLocal> CourseTeachers { get; set; }
    public DbSet<CourseStudentLocal> CourseStudents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite($"Filename={_dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

    
        modelBuilder.Entity<UserLocal>().HasKey(u => u.Id);
        modelBuilder.Entity<CourseStudentLocal>().HasKey(cs => new { cs.CourseId, cs.StudentId });
        modelBuilder.Entity<CourseTeacherLocal>().HasKey(ct => new { ct.CourseId, ct.TeacherId });
    }
}