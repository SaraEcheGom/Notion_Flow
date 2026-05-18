using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Teacher;

namespace NotionFlow.App.Views.Teacher;

public partial class AssignActivityPage : ContentPage
{
    private readonly ActivityViewModel _vm;
    private readonly ActivityModel _activity;

    public string ActivityTitle => _activity.Title;
    public List<SelectableStudent> SelectableStudents { get; private set; } = new();

    public AssignActivityPage(ApiService api, ActivityViewModel vm, ActivityModel activity)
    {
        InitializeComponent();
        _vm = vm; _activity = activity;
        BindingContext = this;
        _ = LoadStudentsAsync(api);
    }

    private async Task LoadStudentsAsync(ApiService api)
    {
        try
        {
            var courses = await api.GetCoursesByProfessorAsync(AuthService.CurrentUser?.Id ?? string.Empty);
            var course = courses.FirstOrDefault(c => c.Id == _vm.CourseId);
            if (course == null) return;
            var assignedIds = _activity.Assignments.Select(a => a.StudentId).ToHashSet();
            SelectableStudents = course.Students.Select(s => new SelectableStudent
            {
                Id = s.Id,
                Name = s.Name,
                Email = s.Email,
                AlreadyAssigned = assignedIds.Contains(s.Id),
                IsSelected = assignedIds.Contains(s.Id)
            }).ToList();
            OnPropertyChanged(nameof(SelectableStudents));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar estudiantes: {ex.Message}", "OK");
        }
    }

    private void OnSelectAll(object sender, EventArgs e)
    {
        foreach (var s in SelectableStudents) s.IsSelected = true;
        OnPropertyChanged(nameof(SelectableStudents));
    }

    private void OnDeselectAll(object sender, EventArgs e)
    {
        foreach (var s in SelectableStudents) s.IsSelected = false;
        OnPropertyChanged(nameof(SelectableStudents));
    }

    private async void OnAssign(object sender, EventArgs e)
    {
        var ids = SelectableStudents.Where(s => s.IsSelected && !s.AlreadyAssigned).Select(s => s.Id).ToList();
        if (!ids.Any()) { await DisplayAlert("Aviso", "No hay estudiantes nuevos seleccionados.", "OK"); return; }
        try
        {
            await _vm.AssignActivityAsync(_activity.Id, ids);
            await DisplayAlert("Éxito", $"Actividad asignada a {ids.Count} estudiante(s).", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex) { await DisplayAlert("Error", ex.Message, "OK"); }
    }
}

public class SelectableStudent : Microsoft.Maui.Controls.BindableObject
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool AlreadyAssigned { get; set; }
    private bool _isSelected;
    public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }
}