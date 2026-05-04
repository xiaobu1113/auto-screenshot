using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

}
