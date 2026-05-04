using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Diagnostics;
using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels.Teacher
{
    public class ActivityViewModel : BaseViewModel
    {
        private readonly ApiService _api;
        public int CourseId { get; }
        public string CourseName { get; }
        public ObservableCollection<ActivityModel> Activities { get; } = new();
        public ICommand LoadActivitiesCommand { get; }
        public ICommand DeleteActivityCommand { get; }

        public ActivityViewModel(ApiService api, int courseId, string courseName)
        {
            _api = api; CourseId = courseId; CourseName = courseName;
            LoadActivitiesCommand = new Command(async () => await LoadActivitiesAsync());
            DeleteActivityCommand = new Command<ActivityModel>(async (activity) =>
            {
                if (activity == null) return;
                var confirm = await Shell.Current.DisplayAlert("Confirmar",
                    $"¿Eliminar \"{activity.Title}\"?", "Eliminar", "Cancelar");
                if (!confirm) return;
                try
                {
                    await _api.DeleteActivityAsync(CourseId, activity.Id);
                    Activities.Remove(activity);
                    await Shell.Current.DisplayAlert("Éxito", "Actividad eliminada.", "OK");
                }
                catch (Exception ex) { await Shell.Current.DisplayAlert("Error", ex.Message, "OK"); }
            });
            _ = LoadActivitiesAsync();
        }

        public async Task LoadActivitiesAsync()
        {
            try
            {
                var list = await _api.GetActivitiesAsync(CourseId);
                Activities.Clear();
                foreach (var a in list) Activities.Add(a);
            }
            catch (Exception ex) { await Shell.Current.DisplayAlert("Error", ex.Message, "OK"); }
        }

        public async Task<ActivityModel> CreateActivityAsync(string title, string description,
            DateTime dueDate, double pct, List<QuestionPayload> questions)
        {
            var created = await _api.CreateActivityAsync(CourseId, BuildPayload(title, description, dueDate, pct, questions));
            Activities.Insert(0, created);
            return created;
        }

        public async Task UpdateActivityAsync(int id, string title, string description,
            DateTime dueDate, double pct, List<QuestionPayload> questions)
        {
            var updated = await _api.UpdateActivityAsync(CourseId, id, BuildPayload(title, description, dueDate, pct, questions));
            var existing = Activities.FirstOrDefault(a => a.Id == id);
            if (existing != null) Activities[Activities.IndexOf(existing)] = updated;
        }

        public async Task AssignActivityAsync(int activityId, List<string> studentIds) =>
            await _api.AssignActivityAsync(CourseId, activityId, studentIds);

        private static object BuildPayload(string title, string description, DateTime dueDate,
            double pct, List<QuestionPayload> questions) => new
            {
                title,
                description,
                dueDate,
                percentageValue = pct,
                questions = questions.Select(q => new
                {
                    text = q.Text,
                    questionType = q.QuestionType,
                    options = q.Options.Select(o => new { text = o.Text, isCorrect = o.IsCorrect }).ToList()
                }).ToList()
            };
    }

    public class QuestionPayload
    {
        public string Text { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MultipleChoice";
        public List<OptionPayload> Options { get; set; } = new();
    }

    public class OptionPayload
    {
        public string Text { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}