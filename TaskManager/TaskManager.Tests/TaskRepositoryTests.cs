using System;
using System.IO;
using System.Linq;
using TaskManager.Core.Models;
using TaskManager.Core.Repositories;
using Xunit;

namespace TaskManager.Tests
{
    public class TaskRepositoryTests
    {
        private TaskRepository CreateRepo() => new TaskRepository();

        private TaskItem SampleTask(string title = "Тест", WorkStatus status = WorkStatus.New) =>
            new TaskItem
            {
                Title       = title,
                Description = "Описание",
                Priority    = TaskPriority.Medium,
                DueDate     = DateTime.Today.AddDays(3),
                Status      = status
            };

        // ── Add ──────────────────────────────────────────────────────────────

        [Fact]
        public void Add_ValidTask_AssignsId()
        {
            var repo = CreateRepo();
            var task = SampleTask();
            repo.Add(task);
            Assert.Equal(1, task.Id);
        }

        [Fact]
        public void Add_EmptyTitle_ThrowsArgumentException()
        {
            var repo = CreateRepo();
            var task = SampleTask("");
            Assert.Throws<ArgumentException>(() => repo.Add(task));
        }

        [Fact]
        public void Add_NullTask_ThrowsArgumentNullException()
        {
            var repo = CreateRepo();
            Assert.Throws<ArgumentNullException>(() => repo.Add(null!));
        }

        // ── GetAll ───────────────────────────────────────────────────────────

        [Fact]
        public void GetAll_ReturnsAllTasks()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("A"));
            repo.Add(SampleTask("B"));
            Assert.Equal(2, repo.GetAll().Count());
        }

        // ── GetById ──────────────────────────────────────────────────────────

        [Fact]
        public void GetById_ExistingId_ReturnsTask()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("A"));
            var found = repo.GetById(1);
            Assert.NotNull(found);
            Assert.Equal("A", found!.Title);
        }

        [Fact]
        public void GetById_NonExistingId_ReturnsNull()
        {
            var repo = CreateRepo();
            Assert.Null(repo.GetById(99));
        }

        // ── Update ───────────────────────────────────────────────────────────

        [Fact]
        public void Update_ExistingTask_ChangesTitle()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("Старое"));
            var task = repo.GetById(1)!;
            task.Title = "Новое";
            repo.Update(task);
            Assert.Equal("Новое", repo.GetById(1)!.Title);
        }

        [Fact]
        public void Update_NonExistingId_ThrowsKeyNotFoundException()
        {
            var repo = CreateRepo();
            Assert.Throws<KeyNotFoundException>(() => repo.Update(new TaskItem { Id = 99, Title = "X" }));
        }

        // ── Delete ───────────────────────────────────────────────────────────

        [Fact]
        public void Delete_ExistingTask_RemovesIt()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask());
            repo.Delete(1);
            Assert.Empty(repo.GetAll());
        }

        [Fact]
        public void Delete_NonExistingId_ThrowsKeyNotFoundException()
        {
            var repo = CreateRepo();
            Assert.Throws<KeyNotFoundException>(() => repo.Delete(99));
        }

        // ── GetByStatus ──────────────────────────────────────────────────────

        [Fact]
        public void GetByStatus_ReturnsOnlyMatchingTasks()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("A", WorkStatus.New));
            repo.Add(SampleTask("B", WorkStatus.Completed));
            repo.Add(SampleTask("C", WorkStatus.New));

            var newTasks = repo.GetByStatus(WorkStatus.New).ToList();
            Assert.Equal(2, newTasks.Count);
            Assert.All(newTasks, t => Assert.Equal(WorkStatus.New, t.Status));
        }

        // ── Search ───────────────────────────────────────────────────────────

        [Fact]
        public void Search_ByTitle_ReturnsMatchingTasks()
        {
            var repo = CreateRepo();
            repo.Add(new TaskItem { Title = "Купить молоко", Description = "" });
            repo.Add(new TaskItem { Title = "Сходить в магазин", Description = "" });
            var result = repo.Search("молоко").ToList();
            Assert.Single(result);
        }

        [Fact]
        public void Search_ByDescription_ReturnsMatchingTasks()
        {
            var repo = CreateRepo();
            repo.Add(new TaskItem { Title = "Задача 1", Description = "важный проект" });
            repo.Add(new TaskItem { Title = "Задача 2", Description = "обычное дело" });
            Assert.Single(repo.Search("важный"));
        }

        [Fact]
        public void Search_EmptyQuery_ReturnsAllTasks()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("A"));
            repo.Add(SampleTask("B"));
            Assert.Equal(2, repo.Search("").Count());
        }

        // ── Статистика ───────────────────────────────────────────────────────

        [Fact]
        public void GetStatistics_ReturnsCorrectCounts()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("A", WorkStatus.New));
            repo.Add(SampleTask("B", WorkStatus.Completed));
            repo.Add(SampleTask("C", WorkStatus.InProgress));

            var stats = repo.GetStatistics();
            Assert.Equal(3, stats.Total);
            Assert.Equal(1, stats.Completed);
            Assert.Equal(1, stats.InProgress);
            Assert.Equal(1, stats.New);
        }

        // ── JSON сохранение/загрузка ─────────────────────────────────────────

        [Fact]
        public void SaveAndLoadJson_PreservesData()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("Задача JSON"));
            var path = Path.GetTempFileName();
            try
            {
                repo.SaveToJson(path);
                var repo2 = CreateRepo();
                repo2.LoadFromJson(path);
                Assert.Single(repo2.GetAll());
                Assert.Equal("Задача JSON", repo2.GetAll().First().Title);
            }
            finally { File.Delete(path); }
        }

        // ── XML сохранение/загрузка ──────────────────────────────────────────

        [Fact]
        public void SaveAndLoadXml_PreservesData()
        {
            var repo = CreateRepo();
            repo.Add(SampleTask("Задача XML"));
            var path = Path.GetTempFileName();
            try
            {
                repo.SaveToXml(path);
                var repo2 = CreateRepo();
                repo2.LoadFromXml(path);
                Assert.Single(repo2.GetAll());
                Assert.Equal("Задача XML", repo2.GetAll().First().Title);
            }
            finally { File.Delete(path); }
        }

        // ── IsOverdue ────────────────────────────────────────────────────────

        [Fact]
        public void IsOverdue_PastDueNotCompleted_ReturnsTrue()
        {
            var task = new TaskItem
            {
                Title   = "X",
                DueDate = DateTime.Today.AddDays(-1),
                Status  = WorkStatus.New
            };
            Assert.True(task.IsOverdue);
        }

        [Fact]
        public void IsOverdue_Completed_ReturnsFalse()
        {
            var task = new TaskItem
            {
                Title   = "X",
                DueDate = DateTime.Today.AddDays(-1),
                Status  = WorkStatus.Completed
            };
            Assert.False(task.IsOverdue);
        }
    }
}
