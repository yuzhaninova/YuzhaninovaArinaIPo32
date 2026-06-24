using System;

namespace TaskManager.Core.Models
{
    /// <summary>
    /// Приоритет задачи
    /// </summary>
    public enum TaskPriority
    {
        Low,      // Низкий
        Medium,   // Средний
        High      // Высокий
    }

    /// <summary>
    /// Статус задачи
    /// </summary>
    public enum WorkStatus
    {
        New,        // Новая
        InProgress, // В процессе
        Completed   // Завершена
    }

    /// <summary>
    /// Модель задачи
    /// </summary>
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
        public WorkStatus Status { get; set; } = WorkStatus.New;
        public bool IsImportant { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Просрочена ли задача
        /// </summary>
        public bool IsOverdue => Status != WorkStatus.Completed && DueDate < DateTime.Today;

        public override string ToString()
        {
            string star = IsImportant ? "★ " : "";
            return $"[{Id}] {star}{Title} | {Priority} | {Status} | До: {DueDate:dd.MM.yyyy}";
        }
    }
}
