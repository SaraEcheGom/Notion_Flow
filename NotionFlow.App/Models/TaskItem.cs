namespace NotionFlow.App.Models
{
    /// <summary>
    /// Represents a task in the application.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Unique identifier of the task.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Title of the task.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the task is completed.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Date when the task was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}