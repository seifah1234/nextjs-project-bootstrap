using System.Windows;

namespace TodoApp
{
    public partial class App : Application
    {
        private bool _isDarkTheme = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Load theme preference from settings (could be expanded to use registry/config file)
            LoadThemePreference();
        }

        public void SwitchTheme()
        {
            _isDarkTheme = !_isDarkTheme;
            ApplyTheme(_isDarkTheme);
            SaveThemePreference();
        }

        public void ApplyTheme(bool isDarkTheme)
        {
            _isDarkTheme = isDarkTheme;
            
            // Clear existing theme resources
            var existingTheme = Resources.MergedDictionaries.FirstOrDefault(d => 
                d.Source?.OriginalString.Contains("Theme.xaml") == true);
            
            if (existingTheme != null)
            {
                Resources.MergedDictionaries.Remove(existingTheme);
            }

            // Add new theme
            var themeUri = _isDarkTheme ? "Styles/DarkTheme.xaml" : "Styles/LightTheme.xaml";
            var newTheme = new ResourceDictionary()
            {
                Source = new Uri(themeUri, UriKind.Relative)
            };
            
            Resources.MergedDictionaries.Insert(0, newTheme);
        }

        public bool IsDarkTheme => _isDarkTheme;

        private void LoadThemePreference()
        {
            // Simple implementation - could be expanded to use proper settings storage
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var settingsPath = System.IO.Path.Combine(appDataPath, "ModernTodoApp", "settings.txt");
                
                if (System.IO.File.Exists(settingsPath))
                {
                    var content = System.IO.File.ReadAllText(settingsPath);
                    if (bool.TryParse(content, out bool isDark))
                    {
                        ApplyTheme(isDark);
                    }
                }
            }
            catch
            {
                // If loading fails, use default light theme
                ApplyTheme(false);
            }
        }

        private void SaveThemePreference()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = System.IO.Path.Combine(appDataPath, "ModernTodoApp");
                
                if (!System.IO.Directory.Exists(appFolder))
                {
                    System.IO.Directory.CreateDirectory(appFolder);
                }
                
                var settingsPath = System.IO.Path.Combine(appFolder, "settings.txt");
                System.IO.File.WriteAllText(settingsPath, _isDarkTheme.ToString());
            }
            catch
            {
                // Silently fail if saving theme preference fails
            }
        }
    }
}
