using NotionFlow.App.ViewModels;
using NotionFlow.App.ViewModels.Student;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Student;

[QueryProperty(nameof(StudentId), "id")]
public partial class StudentPage : ContentPage
{
    private string _studentId = string.Empty;

    public string StudentId
    {
        get => _studentId;
        set
        {
            _studentId = value;
            if (!string.IsNullOrEmpty(_studentId))
            {
                var apiService = new ApiService();
                BindingContext = new StudentViewModel(apiService, _studentId);
            }
        }
    }

    public StudentPage()
    {
        InitializeComponent();
    }
}
