using System.Text.Json;
using NotionFlow.App.Models;

namespace NotionFlow.App.Services
{
    public class TaskService
    {
        private const string FileName = "tasks.json";

        public List<TaskItem> Tasks { get; set; }

        public TaskService()
        {
            Tasks = LoadTasks();
        }

        /// <summary>
        /// Obtiene todas las tareas
        /// </summary>
        public List<TaskItem> GetTasks()
        {
            return Tasks;
        }

        /// <summary>
        /// Agrega una tarea
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
        public void SaveTasks()
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

        /// <summary>
        /// Obtiene la ruta del archivo
        /// </summary>
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