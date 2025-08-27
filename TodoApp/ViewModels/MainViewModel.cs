using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TodoApp.Models;
using TodoApp.Services;
using TodoApp.Views;

namespace TodoApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private readonly DispatcherTimer _reminderTimer;
        private TodoItem? _selectedTodo;
        private string _searchText = string.Empty;
        private string _selectedCategory = "All";
        private PriorityLevel? _selectedPriority;

        public MainViewModel()
        {
            _dataService = new DataService();
            Todos = _dataService.LoadTodos();
            
            // Initialize commands
            AddTodoCommand = new RelayCommand(AddTodo);
            EditTodoCommand = new RelayCommand(EditTodo, () => SelectedTodo != null);
            DeleteTodoCommand = new RelayCommand(DeleteTodo, () => SelectedTodo != null);
            ToggleCompleteCommand = new RelayCommand(ToggleComplete, () => SelectedTodo != null);
            ClearCompletedCommand = new RelayCommand(ClearCompleted, () => Todos.Any(t => t.IsCompleted));
            
            // Setup reminder timer (check every 30 minutes)
            _reminderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(30)
            };
            _reminderTimer.Tick += CheckReminders;
            _reminderTimer.Start();
            
            // Initial reminder check
            CheckReminders(null, EventArgs.Empty);
            
            UpdateCategories();
        }

        public ObservableCollection<TodoItem> Todos { get; }
        
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

        public TodoItem? SelectedTodo
        {
            get => _selectedTodo;
            set
            {
                _selectedTodo = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredTodos));
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredTodos));
            }
        }

        public PriorityLevel? SelectedPriority
        {
            get => _selectedPriority;
            set
            {
                _selectedPriority = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredTodos));
            }
        }

        public ObservableCollection<TodoItem> FilteredTodos
        {
            get
            {
                var filtered = Todos.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filtered = filtered.Where(t => 
                        t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        t.Description.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                }

                if (SelectedCategory != "All" && !string.IsNullOrEmpty(SelectedCategory))
                {
                    filtered = filtered.Where(t => t.Category == SelectedCategory);
                }

                if (SelectedPriority.HasValue)
                {
                    filtered = filtered.Where(t => t.Priority == SelectedPriority.Value);
                }

                return new ObservableCollection<TodoItem>(filtered.OrderBy(t => t.IsCompleted)
                    .ThenByDescending(t => t.Priority)
                    .ThenBy(t => t.DueDate));
            }
        }

        public int TotalTodos => Todos.Count;
        public int CompletedTodos => Todos.Count(t => t.IsCompleted);
        public int PendingTodos => Todos.Count(t => !t.IsCompleted);
        public int OverdueTodos => Todos.Count(t => t.IsOverdue);

        // Commands
        public ICommand AddTodoCommand { get; }
        public ICommand EditTodoCommand { get; }
        public ICommand DeleteTodoCommand { get; }
        public ICommand ToggleCompleteCommand { get; }
        public ICommand ClearCompletedCommand { get; }

        private void AddTodo()
        {
            var newTodoWindow = new NewTodoWindow();
            if (newTodoWindow.ShowDialog() == true && newTodoWindow.TodoItem != null)
            {
                Todos.Add(newTodoWindow.TodoItem);
                SaveTodos();
                UpdateCategories();
                UpdateStats();
            }
        }

        private void EditTodo()
        {
            if (SelectedTodo == null) return;

            var editTodoWindow = new NewTodoWindow(SelectedTodo);
            if (editTodoWindow.ShowDialog() == true)
            {
                SaveTodos();
                UpdateCategories();
                UpdateStats();
            }
        }

        private void DeleteTodo()
        {
            if (SelectedTodo == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{SelectedTodo.Title}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Todos.Remove(SelectedTodo);
                SaveTodos();
                UpdateCategories();
                UpdateStats();
            }
        }

        private void ToggleComplete()
        {
            if (SelectedTodo == null) return;

            SelectedTodo.IsCompleted = !SelectedTodo.IsCompleted;
            SaveTodos();
            UpdateStats();
            OnPropertyChanged(nameof(FilteredTodos));
        }

        private void ClearCompleted()
        {
            var completedTodos = Todos.Where(t => t.IsCompleted).ToList();
            
            var result = MessageBox.Show(
                $"Are you sure you want to delete {completedTodos.Count} completed todo(s)?",
                "Confirm Clear Completed",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                foreach (var todo in completedTodos)
                {
                    Todos.Remove(todo);
                }
                SaveTodos();
                UpdateCategories();
                UpdateStats();
            }
        }

        private void CheckReminders(object? sender, EventArgs e)
        {
            var dueSoonTodos = Todos.Where(t => t.IsDueSoon && !t.IsCompleted).ToList();
            var overdueTodos = Todos.Where(t => t.IsOverdue).ToList();

            if (dueSoonTodos.Any() || overdueTodos.Any())
            {
                var message = "";
                
                if (overdueTodos.Any())
                {
                    message += $"⚠️ {overdueTodos.Count} overdue task(s):\n";
                    message += string.Join("\n", overdueTodos.Take(3).Select(t => $"• {t.Title}"));
                    if (overdueTodos.Count > 3) message += $"\n... and {overdueTodos.Count - 3} more";
                    message += "\n\n";
                }

                if (dueSoonTodos.Any())
                {
                    message += $"📅 {dueSoonTodos.Count} task(s) due soon:\n";
                    message += string.Join("\n", dueSoonTodos.Take(3).Select(t => $"• {t.Title} (Due: {t.DueDateText})"));
                    if (dueSoonTodos.Count > 3) message += $"\n... and {dueSoonTodos.Count - 3} more";
                }

                MessageBox.Show(message, "Task Reminders", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SaveTodos()
        {
            _dataService.SaveTodos(Todos);
        }

        private void UpdateCategories()
        {
            var categories = Todos.Select(t => t.Category)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            Categories.Clear();
            Categories.Add("All");
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

        private void UpdateStats()
        {
            OnPropertyChanged(nameof(TotalTodos));
            OnPropertyChanged(nameof(CompletedTodos));
            OnPropertyChanged(nameof(PendingTodos));
            OnPropertyChanged(nameof(OverdueTodos));
            OnPropertyChanged(nameof(FilteredTodos));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
