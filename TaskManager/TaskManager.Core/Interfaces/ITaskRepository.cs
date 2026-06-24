using System.Collections.Generic;
using TaskManager.Core.Models;

namespace TaskManager.Core.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория задач
    /// </summary>
    public interface ITaskRepository
    {
        void Add(TaskItem task);
        void Update(TaskItem task);
        void Delete(int id);
        TaskItem? GetById(int id);
        IEnumerable<TaskItem> GetAll();
        IEnumerable<TaskItem> GetByStatus(WorkStatus status);
        IEnumerable<TaskItem> Search(string query);
        void SaveToJson(string filePath);
        void LoadFromJson(string filePath);
        void SaveToXml(string filePath);
        void LoadFromXml(string filePath);
    }
}
