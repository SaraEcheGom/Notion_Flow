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
        set { _courseId = Uri.UnescapeDataString(value ?? string.Empty); TryLoadViewModel(); }
    }

    public string CourseName
    {
        get => _courseName;
        set { _courseName = Uri.UnescapeDataString(value ?? string.Empty); TryLoadViewModel(); }
    }

    public string Role
    {
        get => _role;
        set { _role = Uri.UnescapeDataString(value ?? string.Empty); TryLoadViewModel(); }
    }

    private bool _vmLoaded = false;

    private void TryLoadViewModel()
    {
        // If BindingContext was set externally (e.g. TeacherViewModel push) don't overwrite it
        if (_vmLoaded || BindingContext is CourseViewModel) return;

        if (!string.IsNullOrEmpty(_courseId) &&
            !string.IsNullOrEmpty(_courseName) &&
            !string.IsNullOrEmpty(_role))
        {
            BindingContext = new CourseViewModel(_courseId, _courseName, _role);
            _vmLoaded = true;
        }
    }

    public CoursePage()
    {
        InitializeComponent();
    }
}
