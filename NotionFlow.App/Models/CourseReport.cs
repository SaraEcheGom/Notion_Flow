namespace NotionFlow.App.Models;

public class CourseReport
{
    public int TotalStudents { get; set; }
    public int TotalActivities { get; set; }
    public double AverageCourseScore { get; set; }
    public List<StudentSummary> StudentSummaries { get; set; } = new();
}