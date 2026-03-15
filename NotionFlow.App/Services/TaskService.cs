using System.Text.Json;
using NotionFlow.App.Models;

namespace NotionFlow.App.Services
{
    /// <summary>
    /// Servicio encargado de guardar y cargar las tareas
    /// </summary>
    public class TaskService
    {
        private const string FileName = "tasks.json";

        public List<TaskItem> Tasks { get; private set; }

        public TaskService()
        {
            Tasks = LoadTasks();
        }

        /// <summary>
        /// Agrega una nueva tarea
        /// </summary>
        public void AddTask(string title)
        {
            Tasks.Add(new TaskItem
            {
                Title = title,
                IsCompleted = false
            });

            SaveTasks();
        }

        /// <summary>
        /// Elimina una tarea
        /// </summary>
        public void DeleteTask(TaskItem task)
        {
            Tasks.Remove(task);
            SaveTasks();
        }

        /// <summary>
        /// Guarda las tareas en JSON
        /// </summary>
        private void SaveTasks()
        {
            string path = GetFilePath();

            var json = JsonSerializer.Serialize(Tasks);

            File.WriteAllText(path, json);
        }

        /// <summary>
        /// Carga las tareas desde JSON
        /// </summary>
        private List<TaskItem> LoadTasks()
        {
            string path = GetFilePath();

            if (!File.Exists(path))
                return new List<TaskItem>();

            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<List<TaskItem>>(json)
                   ?? new List<TaskItem>();
        }

        private string GetFilePath()
        {
            return Path.Combine(FileSystem.AppDataDirectory, FileName);
        }

        /// <summary>
        /// Guarda el estado actual de las tareas
        /// </summary>
        public void UpdateTasks()
        {
            SaveTasks();
        }
    }
}