using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TodoApp.Models;

namespace TodoApp.Services
{
    public class DataService
    {
        private readonly string _dataFilePath;

        public DataService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "ModernTodoApp");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _dataFilePath = Path.Combine(appFolder, "todos.json");
        }

        public ObservableCollection<TodoItem> LoadTodos()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return new ObservableCollection<TodoItem>();
                }

                var json = File.ReadAllText(_dataFilePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new ObservableCollection<TodoItem>();
                }

                var todos = JsonConvert.DeserializeObject<TodoItem[]>(json);
                return new ObservableCollection<TodoItem>(todos ?? Array.Empty<TodoItem>());
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error loading todos: {ex.Message}", 
                    "Load Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
                
                return new ObservableCollection<TodoItem>();
            }
        }

        public void SaveTodos(ObservableCollection<TodoItem> todos)
        {
            try
            {
                var json = JsonConvert.SerializeObject(todos.ToArray(), Formatting.Indented);
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error saving todos: {ex.Message}", 
                    "Save Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public void BackupData()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var backupPath = _dataFilePath.Replace(".json", $"_backup_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                    File.Copy(_dataFilePath, backupPath);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Error creating backup: {ex.Message}", 
                    "Backup Error", 
                    System.Windows.MessageBoxButton.OK, 
                    System.Windows.MessageBoxImage.Warning);
            }
        }
    }
}
