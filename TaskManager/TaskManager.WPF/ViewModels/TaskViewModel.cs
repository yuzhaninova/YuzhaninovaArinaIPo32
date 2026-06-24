using TaskManager.Core.Models;

namespace TaskManager.WPF.ViewModels
{
    /// <summary>
    /// Обёртка над TaskItem для отображения в ListView
    /// </summary>
    public class TaskViewModel
    {
        private readonly TaskItem _task;
        public TaskViewModel(TaskItem task) => _task = task;
        public TaskItem Task => _task;

        public int    Id          => _task.Id;
        public string Title       => _task.Title;
        public string Description => _task.Description;
        public bool   IsImportant => _task.IsImportant;
        public bool   IsOverdue   => _task.IsOverdue;
        public bool   IsCompleted => _task.Status == WorkStatus.Completed;

        public string ImportantStar => _task.IsImportant ? "★" : "";

        public string PriorityDisplay => _task.Priority switch
        {
            TaskPriority.High   => "🔴 Высокий",
            TaskPriority.Medium => "🟡 Средний",
            TaskPriority.Low    => "🟢 Низкий",
            _                   => _task.Priority.ToString()
        };

        public string StatusDisplay => _task.Status switch
        {
            WorkStatus.New        => "🆕 Новая",
            WorkStatus.InProgress => "⏳ В процессе",
            WorkStatus.Completed  => "✅ Завершена",
            _                     => _task.Status.ToString()
        };

        public string DueDateDisplay => _task.DueDate.ToString("dd.MM.yyyy");
        public string OverdueDisplay  => _task.IsOverdue ? "⚠ Да" : "";
    }
}
