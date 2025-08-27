using System.Windows;
using System.Windows.Controls;
using TodoApp.Models;

namespace TodoApp.Views
{
    public partial class NewTodoWindow : Window
    {
        private TodoItem? _editingTodo;
        private bool _isEditMode;

        public TodoItem? TodoItem { get; private set; }

        public NewTodoWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        public NewTodoWindow(TodoItem todoToEdit)
        {
            InitializeComponent();
            _editingTodo = todoToEdit;
            _isEditMode = true;
            InitializeWindow();
            LoadTodoData();
        }

        private void InitializeWindow()
        {
            if (_isEditMode)
            {
                Title = "Edit Todo";
                HeaderLabel.Content = "Edit Todo";
                SaveButton.Content = "Update Todo";
            }
            else
            {
                DueDatePicker.SelectedDate = DateTime.Today.AddDays(1);
            }

            // Set focus to title textbox
            Loaded += (s, e) => TitleTextBox.Focus();
        }

        private void LoadTodoData()
        {
            if (_editingTodo == null) return;

            TitleTextBox.Text = _editingTodo.Title;
            DescriptionTextBox.Text = _editingTodo.Description;
            DueDatePicker.SelectedDate = _editingTodo.DueDate;
            CategoryComboBox.Text = _editingTodo.Category;

            // Set priority
            foreach (ComboBoxItem item in PriorityComboBox.Items)
            {
                if (item.Tag?.ToString() == _editingTodo.Priority.ToString())
                {
                    PriorityComboBox.SelectedItem = item;
                    break;
                }
            }

            // Set additional options based on priority and other factors
            HighPriorityCheckBox.IsChecked = _editingTodo.Priority == PriorityLevel.High;
            SetReminderCheckBox.IsChecked = _editingTodo.IsDueSoon || _editingTodo.Priority == PriorityLevel.High;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                if (_isEditMode && _editingTodo != null)
                {
                    UpdateExistingTodo();
                }
                else
                {
                    CreateNewTodo();
                }

                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInput()
        {
            bool isValid = true;
            var errors = new List<string>();

            // Reset error displays
            TitleError.Visibility = Visibility.Collapsed;
            DueDateError.Visibility = Visibility.Collapsed;
            ValidationSummary.Visibility = Visibility.Collapsed;

            // Validate title
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                TitleError.Text = "Title is required";
                TitleError.Visibility = Visibility.Visible;
                errors.Add("Title is required");
                isValid = false;
            }
            else if (TitleTextBox.Text.Length > 100)
            {
                TitleError.Text = "Title must be 100 characters or less";
                TitleError.Visibility = Visibility.Visible;
                errors.Add("Title is too long");
                isValid = false;
            }

            // Validate due date
            if (!DueDatePicker.SelectedDate.HasValue)
            {
                DueDateError.Text = "Due date is required";
                DueDateError.Visibility = Visibility.Visible;
                errors.Add("Due date is required");
                isValid = false;
            }
            else if (DueDatePicker.SelectedDate.Value < DateTime.Today && !_isEditMode)
            {
                DueDateError.Text = "Due date cannot be in the past";
                DueDateError.Visibility = Visibility.Visible;
                errors.Add("Due date cannot be in the past");
                isValid = false;
            }

            // Validate description length
            if (DescriptionTextBox.Text.Length > 500)
            {
                errors.Add("Description must be 500 characters or less");
                isValid = false;
            }

            // Show validation summary if there are errors
            if (!isValid && errors.Any())
            {
                ValidationSummary.Text = string.Join(", ", errors);
                ValidationSummary.Visibility = Visibility.Visible;
            }

            return isValid;
        }

        private void CreateNewTodo()
        {
            var priority = GetSelectedPriority();
            
            // Override priority if marked as urgent
            if (HighPriorityCheckBox.IsChecked == true)
            {
                priority = PriorityLevel.High;
            }

            TodoItem = new TodoItem
            {
                Title = TitleTextBox.Text.Trim(),
                Description = DescriptionTextBox.Text.Trim(),
                DueDate = DueDatePicker.SelectedDate!.Value,
                Category = CategoryComboBox.Text.Trim(),
                Priority = priority,
                IsCompleted = false
            };
        }

        private void UpdateExistingTodo()
        {
            if (_editingTodo == null) return;

            var priority = GetSelectedPriority();
            
            // Override priority if marked as urgent
            if (HighPriorityCheckBox.IsChecked == true)
            {
                priority = PriorityLevel.High;
            }

            _editingTodo.Title = TitleTextBox.Text.Trim();
            _editingTodo.Description = DescriptionTextBox.Text.Trim();
            _editingTodo.DueDate = DueDatePicker.SelectedDate!.Value;
            _editingTodo.Category = CategoryComboBox.Text.Trim();
            _editingTodo.Priority = priority;

            TodoItem = _editingTodo;
        }

        private PriorityLevel GetSelectedPriority()
        {
            if (PriorityComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Tag?.ToString() switch
                {
                    "High" => PriorityLevel.High,
                    "Medium" => PriorityLevel.Medium,
                    "Low" => PriorityLevel.Low,
                    _ => PriorityLevel.Medium
                };
            }
            return PriorityLevel.Medium;
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Handle keyboard shortcuts
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                CancelButton_Click(sender, e);
            }
            else if (e.Key == System.Windows.Input.Key.Enter && 
                     System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                SaveButton_Click(sender, e);
            }
        }

        private void TitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Clear title error when user starts typing
            if (TitleError.Visibility == Visibility.Visible && !string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                TitleError.Visibility = Visibility.Collapsed;
            }
        }

        private void DueDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            // Clear due date error when user selects a date
            if (DueDateError.Visibility == Visibility.Visible && DueDatePicker.SelectedDate.HasValue)
            {
                DueDateError.Visibility = Visibility.Collapsed;
            }
        }

        private void HighPriorityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Auto-select high priority when urgent is checked
            foreach (ComboBoxItem item in PriorityComboBox.Items)
            {
                if (item.Tag?.ToString() == "High")
                {
                    PriorityComboBox.SelectedItem = item;
                    break;
                }
            }
        }
    }
}
