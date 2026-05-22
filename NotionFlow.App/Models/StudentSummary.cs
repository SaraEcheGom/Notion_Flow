namespace NotionFlow.App.Models;

public class StudentSummary
{
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public double Score { get; set; }
    public int TotalActivities { get; set; }
    public int CompletedActivities { get; set; }
    public int Rank { get; set; }
    public string LevelEmoji { get; set; } = string.Empty;
    public string LevelName { get; set; } = string.Empty;
    public double AverageScore { get; set; }
    public List<string> Badges { get; set; } = new();
    public int BadgeCount { get; set; }
    public int TotalPoints { get; set; }
}