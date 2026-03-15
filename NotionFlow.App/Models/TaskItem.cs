using System.ComponentModel;

namespace NotionFlow.App.Models
{
    /// <summary>
    /// Modelo que representa una tarea dentro de la aplicación
    /// </summary>
    public class TaskItem : INotifyPropertyChanged
    {
        private bool isCompleted;

        public string? Title { get; set; }

        public bool IsCompleted
        {
            get => isCompleted;
            set
            {
                isCompleted = value;
                OnPropertyChanged(nameof(IsCompleted));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}