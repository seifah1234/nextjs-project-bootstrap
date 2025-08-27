using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TodoApp.ViewModels;

namespace TodoApp.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            
            // Set initial theme toggle state
            var app = (App)Application.Current;
            ThemeToggle.IsChecked = app.IsDarkTheme;
            
            // Setup search textbox placeholder behavior
            SetupSearchPlaceholder();
        }

        private void SetupSearchPlaceholder()
        {
            SearchTextBox.GotFocus += (s, e) =>
            {
                if (SearchTextBox.Text == "Search todos...")
                {
                    SearchTextBox.Text = "";
                    SearchTextBox.Foreground = (System.Windows.Media.Brush)FindResource("PrimaryTextBrush");
                }
            };

            SearchTextBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
                {
                    SearchTextBox.Text = "Search todos...";
                    SearchTextBox.Foreground = (System.Windows.Media.Brush)FindResource("SecondaryTextBrush");
                }
            };

            // Initial placeholder setup
            if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
            {
                SearchTextBox.Text = "Search todos...";
                SearchTextBox.Foreground = (System.Windows.Media.Brush)FindResource("SecondaryTextBrush");
            }
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.SwitchTheme();
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.SwitchTheme();
        }

        private void TodoItem_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.DataContext != null)
            {
                _viewModel.EditTodoCommand.Execute(null);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Save data when window closes
            base.OnClosed(e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard shortcuts
            if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
            {
                _viewModel.AddTodoCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Delete && _viewModel.SelectedTodo != null)
            {
                _viewModel.DeleteTodoCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.F3)
            {
                SearchTextBox.Focus();
                e.Handled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus search box on startup
            SearchTextBox.Focus();
        }
    }
}
