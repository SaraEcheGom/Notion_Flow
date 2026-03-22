using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.Views.Course;

[QueryProperty(nameof(CourseId), "courseId")]
[QueryProperty(nameof(CourseName), "courseName")]
[QueryProperty(nameof(Role), "role")]
public partial class CoursePage : ContentPage
{
    private string _courseId = string.Empty;
    private string _courseName = string.Empty;
    private string _role = string.Empty;

    public string CourseId
    {
        get => _courseId;
        set { _courseId = value; TryLoadViewModel(); }
    }

    public string CourseName
    {
        get => _courseName;
        set { _courseName = value; TryLoadViewModel(); }
    }

    public string Role
    {
        get => _role;
        set { _role = value; TryLoadViewModel(); }
    }

    private void TryLoadViewModel()
    {
        if (!string.IsNullOrEmpty(_courseId) &&
            !string.IsNullOrEmpty(_courseName) &&
            !string.IsNullOrEmpty(_role))
        {
            BindingContext = new CourseViewModel(_courseId, _courseName, _role);
        }
    }

    public CoursePage()
    {
        InitializeComponent();
    }
}
