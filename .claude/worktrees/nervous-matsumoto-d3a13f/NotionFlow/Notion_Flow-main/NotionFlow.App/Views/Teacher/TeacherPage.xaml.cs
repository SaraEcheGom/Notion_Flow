using NotionFlow.App.ViewModels;
using NotionFlow.App.ViewModels.Teacher;
using NotionFlow.App.Services;

namespace NotionFlow.App.Views.Teacher;

[QueryProperty(nameof(TeacherId), "id")]
public partial class TeacherPage : ContentPage
{
    private string _teacherId = string.Empty;

    public string TeacherId
    {
        get => _teacherId;
        set
        {
            _teacherId = value;
            if (!string.IsNullOrEmpty(_teacherId))
            {
                var apiService = new ApiService();
                BindingContext = new TeacherViewModel(apiService, _teacherId);
            }
        }
    }

    public TeacherPage()
    {
        InitializeComponent();
    }
}
