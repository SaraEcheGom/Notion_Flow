using System.Collections.Generic;
using System.Threading.Tasks;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Course
{
    public class StudentProgressViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        private readonly int _courseId;
        private readonly string _studentId;

        public string StudentName { get; set; }
        public int TotalActivities { get; set; }
        public int CompletedActivities { get; set; }
        public double AverageScore { get; set; }
        public int TotalPoints { get; set; }

        public List<ActivityProgressDetail> ActivityDetails { get; set; } = new();
        public List<BadgeModel> Badges { get; set; } = new();

        public StudentProgressViewModel(ApiService api, int courseId, string studentId, string studentName)
        {
            _api = api;
            _courseId = courseId;
            _studentId = studentId;
            StudentName = studentName;

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                var res = await _api.GetStudentProgressAsync(_courseId, _studentId);
                StudentName = res.StudentName;
                TotalActivities = res.TotalActivities;
                CompletedActivities = res.CompletedActivities;
                AverageScore = res.AverageScore;
                TotalPoints = res.TotalPoints;
                ActivityDetails = res.ActivityDetails;
                Badges = res.Badges;
            }
            catch (Exception ex)
            {
                CrashLog.Write("StudentProgressViewModel.LoadAsync", ex);
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
