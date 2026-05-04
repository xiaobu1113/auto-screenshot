using System.IO;
using System.Text.Json;

namespace AutoScreenShotAI.Services
{
    public class AppSettings
    {
        public string OutputDir { get; set; } = string.Empty;
        public string PythonScript { get; set; } = string.Empty;
        public string FilenameTemplate { get; set; } = "yyyy-MM-dd HH-mm-ss";
        public string Prompt { get; set; } = "Please describe the content of this screenshot in detail...";
        public string Interval { get; set; } = "300";
        public string AutoDeletionDays { get; set; } = "60";
        public int Language { get; set; } = 0; // 0=中文,1=英文
        public int ScreenshotFormat { get; set; } = 0; // 0=jpg,1=png
        public bool IsAutoStartup { get; set; } = false;
        public bool IsMinimizeToTray { get; set; } = false;
        public bool IsAutoDelete { get; set; } = false;
        public bool IsMinimizeRunning { get; set; } = false;
        public bool IsScript { get; set; } = false;
        public bool IsCamera { get; set; } = false;
    }

    public class SettingsService
    {

        private AppSettings _current;
        public AppSettings Current => _current;

        private static readonly string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string appFolder = Path.Combine(appData, "AutoScreenShotAI");
        private readonly string _filePath = Path.Combine(appFolder, "settings.json");

        //string exeDir = AppContext.BaseDirectory;
        //string settingsPath = Path.Combine(exeDir, "settings.json");

        public SettingsService()
        {
            _current = Load();
        }

        private AppSettings Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                //System.Diagnostics.Debug.WriteLine($"Settings load error: {ex.Message}");
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_current, options);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                //System.Diagnostics.Debug.WriteLine($"Settings save error: {ex.Message}");
            }
        }

        public void UpdateOutputDir(string dir) => _current.OutputDir = dir;
        public void UpdatePythonScript(string path) => _current.PythonScript = path;
    }
}