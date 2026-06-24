using System;
using System.Windows;
using TaskManager.Core.Models;

namespace TaskManager.WPF.Views
{
    public partial class TaskEditWindow : Window
    {
        public TaskItem? EditedTask { get; private set; }
        private readonly TaskItem? _existing;

        // Режим «добавить»
        public TaskEditWindow()
        {
            InitializeComponent();
            DueDatePicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        // Режим «редактировать»
        public TaskEditWindow(TaskItem task) : this()
        {
            _existing = task;
            WindowTitle.Text          = "Редактировать задачу";
            TitleBox.Text             = task.Title;
            DescriptionBox.Text       = task.Description;
            DueDatePicker.SelectedDate = task.DueDate;
            ImportantCheckBox.IsChecked = task.IsImportant;

            PriorityComboBox.SelectedIndex = task.Priority switch
            {
                TaskPriority.Low    => 0,
                TaskPriority.Medium => 1,
                TaskPriority.High   => 2,
                _                   => 1
            };

            StatusComboBox.SelectedIndex = task.Status switch
            {
                WorkStatus.New        => 0,
                WorkStatus.InProgress => 1,
                WorkStatus.Completed  => 2,
                _                     => 0
            };
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название задачи.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var priority = PriorityComboBox.SelectedIndex switch
            {
                0 => TaskPriority.Low,
                2 => TaskPriority.High,
                _ => TaskPriority.Medium
            };

            var status = StatusComboBox.SelectedIndex switch
            {
                1 => WorkStatus.InProgress,
                2 => WorkStatus.Completed,
                _ => WorkStatus.New
            };

            EditedTask = new TaskItem
            {
                Id          = _existing?.Id ?? 0,
                Title       = TitleBox.Text.Trim(),
                Description = DescriptionBox.Text.Trim(),
                Priority    = priority,
                Status      = status,
                DueDate     = DueDatePicker.SelectedDate ?? DateTime.Today.AddDays(7),
                IsImportant = ImportantCheckBox.IsChecked == true,
                CreatedAt   = _existing?.CreatedAt ?? DateTime.Now
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) =>
            DialogResult = false;
    }
}
