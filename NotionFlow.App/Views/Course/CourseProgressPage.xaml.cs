using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.Views.Course;

public partial class CourseProgressPage : ContentPage
{
    private readonly ApiService _api;
    private readonly int _courseId;

    public CourseProgressPage(ApiService api, int courseId, string courseName)
    {
        InitializeComponent();
        _api = api;
        _courseId = courseId;
        BindingContext = new CourseProgressViewModel(_api, _courseId, courseName);
    }
}
