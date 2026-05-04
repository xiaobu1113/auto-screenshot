using System.IO;
using System.Text.Json;

namespace AutoScreenShotAI.Services
{
    public class SettingsService
    {
        private readonly string _filePath;
        private AppSettings _current;

        public AppSettings Current => _current;

        public SettingsService(string filePath)
        {
            _filePath = filePath;
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