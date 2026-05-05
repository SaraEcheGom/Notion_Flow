using NotionFlow.App.ViewModels;
using NotionFlow.App.ViewModels.Teacher;
using NotionFlow.App.Services;
using System.Diagnostics;
using NotionFlow.App.Models.Auth;
using Microsoft.Maui.Controls.Shapes;

namespace NotionFlow.App.Views.Teacher;

[QueryProperty(nameof(TeacherId), "id")]
public partial class TeacherPage : ContentPage
{
    private string _teacherId = string.Empty;
    private TeacherViewModel? _viewModel;
    private bool _isInitialized = false;

    public string TeacherId
    {
        get => _teacherId;
        set
        {
            Debug.WriteLine($"📍 [TeacherPage] TeacherId setter called with value: {value}");
            _teacherId = value;
            if (!string.IsNullOrEmpty(_teacherId) && !_isInitialized)
            {
                Debug.WriteLine($"📍 [TeacherPage] Initializing ViewModel with teacherId: {_teacherId}");
                var apiService = new ApiService();
                _viewModel = new TeacherViewModel(apiService, _teacherId);
                BindingContext = _viewModel;

                // Subscribe to collection changes
                if (_viewModel.Courses != null)
                {
                    _viewModel.Courses.CollectionChanged += (s, e) =>
                    {
                        Debug.WriteLine($"📍 [TeacherPage] Courses collection changed. Count: {_viewModel.Courses.Count}");
                        RenderCourses();
                    };
                }

                _isInitialized = true;
                Debug.WriteLine($"📍 [TeacherPage] ViewModel initialized and BindingContext set");
            }
        }
    }

    public TeacherPage()
    {
        InitializeComponent();
        Debug.WriteLine($"📍 [TeacherPage] Constructor called");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Debug.WriteLine($"📍 [TeacherPage] OnAppearing called. IsInitialized: {_isInitialized}, BindingContext type: {BindingContext?.GetType().Name}");

        if (_viewModel != null)
        {
            Debug.WriteLine($"📍 [TeacherPage] Executing LoadCoursesCommand");
            _viewModel.LoadCoursesCommand.Execute(null);
        }
        else
        {
            Debug.WriteLine($"⚠️ [TeacherPage] WARNING: ViewModel is null in OnAppearing!");
        }
    }

    private void RenderCourses()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Debug.WriteLine($"📍 [TeacherPage] RenderCourses called. Courses count: {_viewModel?.Courses.Count ?? 0}");

            var container = this.FindByName<VerticalStackLayout>("CoursesContainer");
            if (container == null)
            {
                Debug.WriteLine($"⚠️ [TeacherPage] Container not found!");
                return;
            }

            container.Clear();

            if (_viewModel?.Courses == null || _viewModel.Courses.Count == 0)
            {
                Debug.WriteLine($"📍 [TeacherPage] No courses to render");
                container.Add(new Label
                {
                    Text = "No courses available",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    Margin = new Thickness(0, 20, 0, 0)
                });
                return;
            }

            foreach (var course in _viewModel.Courses)
            {
                Debug.WriteLine($"📍 [TeacherPage] Rendering course: {course.Name}");
                var courseCard = CreateCourseCard(course);
                container.Add(courseCard);
            }
        });
    }

    private Border CreateCourseCard(CourseResponse course)
    {
        var border = new Border
        {
            Margin = new Thickness(0, 8, 0, 0),
            Padding = new Thickness(16),
            StrokeThickness = 1.5,
            Stroke = (Color)Application.Current.Resources["Gray300"],
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(12) },
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = course.Name,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 16,
                        TextColor = (Color)Application.Current.Resources["TextDark"]
                    },
                    new Label
                    {
                        Text = course.Subject,
                        FontSize = 13,
                        TextColor = (Color)Application.Current.Resources["Gray400"],
                        Opacity = 0.9
                    },
                    new Button
                    {
                        Text = "Open course",
                        Margin = new Thickness(0, 8, 0, 0),
                        FontSize = 12,
                        Padding = new Thickness(12, 8),
                        Command = _viewModel?.GoToCourseCommand,
                        CommandParameter = course
                    }
                }
            }
        };

        return border;
    }
}
