using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Course;

namespace NotionFlow.App.Views.Course;

public partial class CourseDetailsPage : ContentPage
{
    public CourseDetailsPage(CourseResponse course, ApiService apiService, AuthService authService)
    {
        InitializeComponent();
        BindingContext = new CourseDetailsViewModel(apiService, authService, course);
    }
}
