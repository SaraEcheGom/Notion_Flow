using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using NotionFlow.App.Models;
using NotionFlow.App.Services;
using System.ComponentModel;
using System.Linq;

namespace NotionFlow.App.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        private TaskService taskService;

        public ObservableCollection<TaskItem> Tasks { get; set; }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        public ICommand ShowAllCommand { get; }
        public ICommand ShowPendingCommand { get; }
        public ICommand ShowCompletedCommand { get; }

        public string NewTaskTitle { get; set; }

        private int totalTasks;
        private int pendingTasks;
        private int completedTasks;

        public int TotalTasks
        {
            get => totalTasks;
            set
            {
                totalTasks = value;
                OnPropertyChanged(nameof(TotalTasks));
            }
        }

        public int PendingTasks
        {
            get => pendingTasks;
            set
            {
                pendingTasks = value;
                OnPropertyChanged(nameof(PendingTasks));
            }
        }

        public int CompletedTasks
        {
            get => completedTasks;
            set
            {
                completedTasks = value;
                OnPropertyChanged(nameof(CompletedTasks));
            }
        }

        public TaskViewModel()
        {
            taskService = new TaskService();

            Tasks = new ObservableCollection<TaskItem>();

            NewTaskTitle = "";

            LoadAllTasks();

            AddTaskCommand = new Command(AddTask);
            DeleteTaskCommand = new Command<TaskItem>(DeleteTask);

            ShowAllCommand = new Command(LoadAllTasks);
            ShowPendingCommand = new Command(LoadPendingTasks);
            ShowCompletedCommand = new Command(LoadCompletedTasks);
        }

        private void AddTask()
        {
            if (!string.IsNullOrWhiteSpace(NewTaskTitle))
            {
                taskService.AddTask(NewTaskTitle);
                LoadAllTasks();
                NewTaskTitle = "";
            }
        }

        private void DeleteTask(TaskItem task)
        {
            if (task == null)
                return;

            taskService.DeleteTask(task);
            Tasks.Remove(task);

            UpdateCounters();
        }

        private void LoadAllTasks()
        {
            Tasks.Clear();

            foreach (var task in taskService.Tasks)
            {
                Tasks.Add(task);
            }

            SubscribeToTaskChanges();
            UpdateCounters();
        }

        private void LoadPendingTasks()
        {
            Tasks.Clear();

            foreach (var task in taskService.Tasks)
            {
                if (!task.IsCompleted)
                    Tasks.Add(task);
            }
        }

        private void LoadCompletedTasks()
        {
            Tasks.Clear();

            foreach (var task in taskService.Tasks)
            {
                if (task.IsCompleted)
                    Tasks.Add(task);
            }
        }

        private void UpdateCounters()
        {
            TotalTasks = taskService.Tasks.Count;
            PendingTasks = taskService.Tasks.Count(t => !t.IsCompleted);
            CompletedTasks = taskService.Tasks.Count(t => t.IsCompleted);
        }

        private void SubscribeToTaskChanges()
        {
            foreach (var task in Tasks)
            {
                task.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(task.IsCompleted))
                    {
                        taskService.UpdateTasks();
                        UpdateCounters();
                    }
                };
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}