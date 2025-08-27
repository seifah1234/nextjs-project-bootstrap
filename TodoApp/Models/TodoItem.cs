using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TodoApp.Models
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private DateTime _dueDate = DateTime.Today.AddDays(1);
        private string _category = string.Empty;
        private PriorityLevel _priority = PriorityLevel.Medium;
        private bool _isCompleted = false;

        public Guid Id { get; set; } = Guid.NewGuid();

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        public PriorityLevel Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
            }
        }

        public bool IsCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                OnPropertyChanged();
            }
        }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string PriorityText => Priority.ToString();

        public string DueDateText => DueDate.ToString("MMM dd, yyyy");

        public bool IsOverdue => !IsCompleted && DueDate < DateTime.Today;

        public bool IsDueSoon => !IsCompleted && DueDate <= DateTime.Today.AddDays(1) && DueDate >= DateTime.Today;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
