using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TaskManager.Core.Models;
using TaskManager.Core.Repositories;
using TaskManager.WPF.ViewModels;

namespace TaskManager.WPF.Views
{
    public partial class MainWindow : Window
    {
        private readonly TaskRepository _repo = new();
        private string _searchQuery  = "";
        private string _currentFilter = "Все";
        private string _currentSort   = "По сроку";
        private bool _initialized = false;

        public MainWindow()
        {
            InitializeComponent();
            LoadDemoData();
            _initialized = true;
            RefreshList();
        }

        // ─── Демо-данные ──────────────────────────────────────────────────────

        private void LoadDemoData()
        {
            _repo.Add(new TaskItem
            {
                Title       = "Написать отчёт",
                Description = "Летняя практика — итоговый отчёт",
                Priority    = TaskPriority.High,
                Status      = WorkStatus.InProgress,
                DueDate     = DateTime.Today.AddDays(2),
                IsImportant = true
            });
            _repo.Add(new TaskItem
            {
                Title       = "Купить продукты",
                Description = "Молоко, хлеб, яйца",
                Priority    = TaskPriority.Low,
                Status      = WorkStatus.New,
                DueDate     = DateTime.Today.AddDays(1)
            });
            _repo.Add(new TaskItem
            {
                Title       = "Сдать задание",
                Description = "Техникум, 3 курс",
                Priority    = TaskPriority.High,
                Status      = WorkStatus.New,
                DueDate     = DateTime.Today.AddDays(-1)
            });
        }

        // ─── Обновление списка ────────────────────────────────────────────────

        private void RefreshList()
        {
            if (!_initialized) return;

            IEnumerable<TaskItem> tasks;

            tasks = _currentFilter switch
            {
                "Новая"      => _repo.GetByStatus(WorkStatus.New),
                "В процессе" => _repo.GetByStatus(WorkStatus.InProgress),
                "Завершена"  => _repo.GetByStatus(WorkStatus.Completed),
                _            => _repo.GetAll()
            };

            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                string q = _searchQuery.ToLower();
                tasks = tasks.Where(t =>
                    t.Title.ToLower().Contains(q) ||
                    t.Description.ToLower().Contains(q));
            }

            tasks = _currentSort == "По приоритету"
                ? tasks.OrderByDescending(t => t.Priority).ThenBy(t => t.DueDate)
                : tasks.OrderBy(t => t.DueDate);

            TaskListView.ItemsSource = tasks.Select(t => new TaskViewModel(t)).ToList();
            UpdateStats();
        }

        private void UpdateStats()
        {
            var s = _repo.GetStatistics();
            StatsText.Text =
                $"Всего: {s.Total}   |   Новых: {s.New}   |   " +
                $"В процессе: {s.InProgress}   |   Завершено: {s.Completed}   |   " +
                $"Просрочено: {s.Overdue}   |   Важных: {s.Important}";
        }

        // ─── Добавить задачу ──────────────────────────────────────────────────

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TaskEditWindow();
            if (dialog.ShowDialog() == true && dialog.EditedTask != null)
            {
                _repo.Add(dialog.EditedTask);
                RefreshList();
            }
        }

        // ─── Редактировать задачу ─────────────────────────────────────────────

        private void EditTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is not TaskViewModel vm)
            {
                MessageBox.Show("Выберите задачу для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var dialog = new TaskEditWindow(vm.Task);
            if (dialog.ShowDialog() == true && dialog.EditedTask != null)
            {
                _repo.Update(dialog.EditedTask);
                RefreshList();
            }
        }

        // ─── Удалить задачу ───────────────────────────────────────────────────

        private void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (TaskListView.SelectedItem is not TaskViewModel vm)
            {
                MessageBox.Show("Выберите задачу для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var result = MessageBox.Show(
                $"Удалить задачу «{vm.Title}»?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                _repo.Delete(vm.Id);
                RefreshList();
            }
        }

        // ─── Фильтр / Сортировка / Поиск ─────────────────────────────────────

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            if (FilterComboBox.SelectedItem is ComboBoxItem item)
                _currentFilter = item.Content.ToString()!;
            RefreshList();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized) return;
            if (SortComboBox.SelectedItem is ComboBoxItem item)
                _currentSort = item.Content.ToString()!;
            RefreshList();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_initialized) return;
            _searchQuery = SearchBox.Text == "Поиск..." ? "" : SearchBox.Text;
            RefreshList();
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void TaskListView_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // ─── JSON ─────────────────────────────────────────────────────────────

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "JSON файлы|*.json", FileName = "tasks.json" };
            if (dlg.ShowDialog() == true)
            {
                _repo.SaveToJson(dlg.FileName);
                MessageBox.Show("Задачи сохранены в JSON.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadJson_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "JSON файлы|*.json" };
            if (dlg.ShowDialog() == true)
            {
                _repo.LoadFromJson(dlg.FileName);
                RefreshList();
                MessageBox.Show("Задачи загружены из JSON.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ─── XML ──────────────────────────────────────────────────────────────

        private void SaveXml_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "XML файлы|*.xml", FileName = "tasks.xml" };
            if (dlg.ShowDialog() == true)
            {
                _repo.SaveToXml(dlg.FileName);
                MessageBox.Show("Задачи сохранены в XML.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadXml_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "XML файлы|*.xml" };
            if (dlg.ShowDialog() == true)
            {
                _repo.LoadFromXml(dlg.FileName);
                RefreshList();
                MessageBox.Show("Задачи загружены из XML.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
