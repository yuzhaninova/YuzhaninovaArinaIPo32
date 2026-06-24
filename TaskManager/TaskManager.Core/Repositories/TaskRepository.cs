using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using TaskManager.Core.Interfaces;
using TaskManager.Core.Models;

namespace TaskManager.Core.Repositories
{
    /// <summary>
    /// Репозиторий задач — хранит коллекцию в памяти, поддерживает JSON/XML
    /// </summary>
    public class TaskRepository : ITaskRepository
    {
        private readonly List<TaskItem> _tasks = new();
        private int _nextId = 1;

        // ─── CRUD ────────────────────────────────────────────────────────────

        public void Add(TaskItem task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (string.IsNullOrWhiteSpace(task.Title))
                throw new ArgumentException("Название задачи не может быть пустым.");

            task.Id = _nextId++;
            task.CreatedAt = DateTime.Now;
            _tasks.Add(task);
        }

        public void Update(TaskItem task)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            var existing = _tasks.FirstOrDefault(t => t.Id == task.Id)
                ?? throw new KeyNotFoundException($"Задача с Id={task.Id} не найдена.");

            existing.Title       = task.Title;
            existing.Description = task.Description;
            existing.Priority    = task.Priority;
            existing.DueDate     = task.DueDate;
            existing.Status      = task.Status;
            existing.IsImportant = task.IsImportant;
        }

        public void Delete(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id)
                ?? throw new KeyNotFoundException($"Задача с Id={id} не найдена.");
            _tasks.Remove(task);
        }

        public TaskItem? GetById(int id) =>
            _tasks.FirstOrDefault(t => t.Id == id);

        public IEnumerable<TaskItem> GetAll() =>
            _tasks.OrderBy(t => t.DueDate).ToList();

        // ─── Фильтрация и поиск  ────────────────────────────────────

        public IEnumerable<TaskItem> GetByStatus(WorkStatus status) =>
            _tasks.Where(t => t.Status == status)
                  .OrderBy(t => t.DueDate)
                  .ToList();

        public IEnumerable<TaskItem> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return GetAll();
            string q = query.ToLower();
            return _tasks
                .Where(t => t.Title.ToLower().Contains(q) ||
                            t.Description.ToLower().Contains(q))
                .OrderBy(t => t.DueDate)
                .ToList();
        }

        // ─── Дополнительные (задания 2.x) ─────────────────────────────────

        public IEnumerable<TaskItem> GetSortedByPriority() =>
            _tasks.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate).ToList();

        public IEnumerable<TaskItem> GetSortedByDueDate() =>
            _tasks.OrderBy(t => t.DueDate).ToList();

        public TaskStatistics GetStatistics()
        {
            return new TaskStatistics
            {
                Total      = _tasks.Count,
                Completed  = _tasks.Count(t => t.Status == WorkStatus.Completed),
                InProgress = _tasks.Count(t => t.Status == WorkStatus.InProgress),
                New        = _tasks.Count(t => t.Status == WorkStatus.New),
                Overdue    = _tasks.Count(t => t.IsOverdue),
                Important  = _tasks.Count(t => t.IsImportant)
            };
        }

        // ─── Файловый ввод-вывод ───────────────────────────────────────────

        public void SaveToJson(string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_tasks, options);
            File.WriteAllText(filePath, json);
        }

        public void LoadFromJson(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            var json = File.ReadAllText(filePath);
            var loaded = JsonSerializer.Deserialize<List<TaskItem>>(json)
                ?? throw new InvalidDataException("Не удалось прочитать JSON.");

            _tasks.Clear();
            _tasks.AddRange(loaded);
            _nextId = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
        }

        public void SaveToXml(string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<TaskItem>));
            using var writer = new StreamWriter(filePath);
            serializer.Serialize(writer, _tasks);
        }

        public void LoadFromXml(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Файл не найден: {filePath}");

            var serializer = new XmlSerializer(typeof(List<TaskItem>));
            using var reader = new StreamReader(filePath);
            var loaded = serializer.Deserialize(reader) as List<TaskItem>
                ?? throw new InvalidDataException("Не удалось прочитать XML.");

            _tasks.Clear();
            _tasks.AddRange(loaded);
            _nextId = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
        }
    }

    /// <summary>
    /// Статистика по задачам (задание 2.3)
    /// </summary>
    public class TaskStatistics
    {
        public int Total      { get; set; }
        public int Completed  { get; set; }
        public int InProgress { get; set; }
        public int New        { get; set; }
        public int Overdue    { get; set; }
        public int Important  { get; set; }

        public override string ToString() =>
            $"Всего: {Total} | Новых: {New} | В процессе: {InProgress} | " +
            $"Завершено: {Completed} | Просрочено: {Overdue} | Важных: {Important}";
    }
}
