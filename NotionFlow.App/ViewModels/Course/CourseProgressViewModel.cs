using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Course
{
    public class CourseProgressViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly int _courseId;

        private string _courseName = string.Empty;
        private int _totalStudents;
        private int _totalActivities;
        private double _averageCourseScore;
        private double _overallCompletionFraction;
        private string _overallCompletionText = string.Empty;

        public string CourseName
        {
            get => _courseName;
            set => SetProperty(ref _courseName, value);
        }

        public int TotalStudents
        {
            get => _totalStudents;
            set => SetProperty(ref _totalStudents, value);
        }

        public int TotalActivities
        {
            get => _totalActivities;
            set => SetProperty(ref _totalActivities, value);
        }

        public double AverageCourseScore
        {
            get => _averageCourseScore;
            set => SetProperty(ref _averageCourseScore, value);
        }

        public double OverallCompletionFraction
        {
            get => _overallCompletionFraction;
            set => SetProperty(ref _overallCompletionFraction, value);
        }

        public string OverallCompletionText
        {
            get => _overallCompletionText;
            set => SetProperty(ref _overallCompletionText, value);
        }

        private string _courseCompletedText = string.Empty;
        public string CourseCompletedText
        {
            get => _courseCompletedText;
            set => SetProperty(ref _courseCompletedText, value);
        }

        private string _coursePoints = string.Empty;
        public string CoursePoints
        {
            get => _coursePoints;
            set => SetProperty(ref _coursePoints, value);
        }

        public ObservableCollection<StudentSummaryDisplayItem> StudentSummaries { get; } = new();

        public ICommand ViewStudentProgressCommand { get; }

        public CourseProgressViewModel(ApiService api, int courseId, string courseName)
        {
            _api = api;
            _courseId = courseId;
            CourseName = courseName;

            ViewStudentProgressCommand = new Command<StudentSummaryDisplayItem>(async (student) =>
            {
                if (student == null) return;
                await Shell.Current.Navigation.PushAsync(
                    new NotionFlow.App.Views.Student.StudentProgressPage(_api, _courseId, student.StudentId));
            });

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var report = await _api.GetCourseReportAsync(_courseId);
                CourseName = report.CourseName;
                TotalStudents = report.TotalStudents;
                TotalActivities = report.TotalActivities;
                AverageCourseScore = report.AverageCourseScore;

                StudentSummaries.Clear();
                int totalCompletedActivities = 0;
                int totalAssignedActivities = 0;
                int totalPoints = 0;
                int studentCount = report.StudentSummaries.Count;

                foreach (var s in report.StudentSummaries)
                {
                    StudentSummaries.Add(new StudentSummaryDisplayItem(s));
                    totalCompletedActivities += s.CompletedActivities;
                    totalAssignedActivities += s.TotalActivities;
                    totalPoints += s.TotalPoints;
                }

                if (totalAssignedActivities > 0)
                {
                    OverallCompletionFraction = (double)totalCompletedActivities / totalAssignedActivities;
                    OverallCompletionText = $"{OverallCompletionFraction * 100:F0}% completado en promedio";
                }
                else
                {
                    OverallCompletionFraction = 0;
                    OverallCompletionText = "Sin actividades asignadas todavía";
                }

                CourseCompletedText = totalAssignedActivities > 0
                    ? $"{totalCompletedActivities}/{totalAssignedActivities}"
                    : "0/0";

                CoursePoints = studentCount > 0
                    ? $"{Math.Round((double)totalPoints / studentCount):F0}"
                    : "0";
            }
            catch (Exception ex)
            {
                CrashLog.Write("CourseProgressViewModel.LoadAsync", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    public class StudentSummaryDisplayItem
    {
        private readonly StudentSummaryItem _source;

        public StudentSummaryDisplayItem(StudentSummaryItem source)
        {
            _source = source;
        }

        public string StudentId => _source.StudentId;
        public string StudentName => _source.StudentName;
        public int Rank => _source.Rank;
        public int TotalActivities => _source.TotalActivities;
        public int CompletedActivities => _source.CompletedActivities;
        public double AverageScore => _source.AverageScore;
        public int TotalPoints => _source.TotalPoints;
        public string LevelName => _source.LevelName;
        public string LevelEmoji => _source.LevelEmoji;
        public int BadgeCount => _source.BadgeCount;

        public double ProgressFraction => TotalActivities > 0 ? (double)CompletedActivities / TotalActivities : 0;
        public string ProgressText => TotalActivities > 0 ? $"{CompletedActivities}/{TotalActivities} completadas" : "Sin actividades";
        public string LevelDisplay => $"{LevelEmoji} {LevelName}";
        public string SummaryText => $"{LevelEmoji} {LevelName} · {AverageScore:F0}% promedio";
    }
}
