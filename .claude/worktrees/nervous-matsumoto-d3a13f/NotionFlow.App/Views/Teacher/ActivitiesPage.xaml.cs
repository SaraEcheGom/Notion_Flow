using NotionFlow.App.Models.Auth;
using NotionFlow.App.Services;
using NotionFlow.App.ViewModels.Teacher;
using System.Windows.Input;

namespace NotionFlow.App.Views.Teacher;

public partial class ActivitiesPage : ContentPage
{
    private readonly ActivityViewModel _vm;

    public ICommand CreateActivityCommand { get; }
    public ICommand EditActivityCommand { get; }
    public ICommand AssignActivityCommand { get; }
    public ICommand LoadActivitiesCommand => _vm.LoadActivitiesCommand;
    public ICommand DeleteActivityCommand => _vm.DeleteActivityCommand;
    public string CourseName => _vm.CourseName;
    public System.Collections.ObjectModel.ObservableCollection<ActivityModel> Activities => _vm.Activities;

    public ActivitiesPage(ApiService api, int courseId, string courseName)
    {
        InitializeComponent();
        _vm = new ActivityViewModel(api, courseId, courseName);

        CreateActivityCommand = new Command(async () =>
            await Navigation.PushAsync(new CreateActivityPage(_vm)));

        EditActivityCommand = new Command<ActivityModel>(async (activity) =>
        {
            if (activity == null) return;
            await Navigation.PushAsync(new EditActivityPage(_vm, activity));
        });

        AssignActivityCommand = new Command<ActivityModel>(async (activity) =>
        {
            if (activity == null) return;
            await Navigation.PushAsync(new AssignActivityPage(api, _vm, activity));
        });

        BindingContext = this;
    }
}