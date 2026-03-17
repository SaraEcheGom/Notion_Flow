using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Linq;
using NotionFlow.App.Models;
using NotionFlow.App.Services;

namespace NotionFlow.App.ViewModels
{
    public class TaskViewModel : BaseViewModel
    {
        private readonly TaskService _taskService;

        public ObservableCollection<TaskItem> Tasks { get; set; }

        private string newTaskTitle = string.Empty;

        public string NewTaskTitle
        {
            get => newTaskTitle;
            set
            {
                newTaskTitle = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddTaskCommand { get; }

        public ICommand CompleteTaskCommand { get; }

        public ICommand DeleteTaskCommand { get; }

        public TaskViewModel()
        {
            _taskService = new TaskService();

            Tasks = new ObservableCollection<TaskItem>(_taskService.GetTasks());

            AddTaskCommand = new Command(AddTask);

            CompleteTaskCommand = new Command<TaskItem>(CompleteTask);

            DeleteTaskCommand = new Command<TaskItem>(DeleteTask);

            UpdateCounts();
        }

        private void AddTask()
        {
            if (string.IsNullOrWhiteSpace(NewTaskTitle))
                return;

            var task = new TaskItem
            {
                Title = NewTaskTitle,
                IsCompleted = false
            };

            Tasks.Add(task);

            _taskService.SaveTasks();

            NewTaskTitle = string.Empty;

            OnPropertyChanged(nameof(NewTaskTitle));

            UpdateCounts();
        }

        private void CompleteTask(TaskItem task)
        {
            if (task == null) return;

            task.IsCompleted = !task.IsCompleted;

            _taskService.SaveTasks();

            UpdateCounts();
        }

        private void DeleteTask(TaskItem task)
        {
            if (task == null) return;

            Tasks.Remove(task);

            _taskService.SaveTasks();

            UpdateCounts();
        }

        private int totalTasks;

        public int TotalTasks
        {
            get => totalTasks;
            set
            {
                totalTasks = value;
                OnPropertyChanged();
            }
        }

        private int completedTasks;

        public int CompletedTasks
        {
            get => completedTasks;
            set
            {
                completedTasks = value;
                OnPropertyChanged();
            }
        }

        private int pendingTasks;

        public int PendingTasks
        {
            get => pendingTasks;
            set
            {
                pendingTasks = value;
                OnPropertyChanged();
            }
        }

        private void UpdateCounts()
        {
            TotalTasks = Tasks.Count;

            CompletedTasks = Tasks.Count(t => t.IsCompleted);

            PendingTasks = Tasks.Count(t => !t.IsCompleted);
        }
    }
}