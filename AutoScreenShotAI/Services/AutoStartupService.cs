using Microsoft.Win32;

namespace AutoScreenShotAI.Services
{
    class AutoStartupService
    {
        public void SetAutoStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                if (key == null) return;
                if (enable)
                    key.SetValue("AutoScreenShotAI", Environment.ProcessPath!);
                else
                    key.DeleteValue("AutoScreenShotAI", false);
            }
            catch { }
        }
    }
}
