using AutoScreenShotAI.Services;
using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

//dotnet publish -c Release -f net8.0-windows -r win-x64 --self-contained false -p:PublishSingleFile=true
namespace AutoScreenShotAI
{
    public partial class MainWindow : Window
    {
        private readonly TrayService _tray;
        private readonly SettingsService _settings = new();
        private readonly LanguageService _language = new();
        private readonly AutoStartupService _autoStartupService = new();
        private readonly AutoDeleteService _autoDeleteService = new();
        private readonly ScreenshotManager _screenshotManager;
        public MainWindow()
        {
            InitializeComponent();
            _tray = new TrayService(this);
            _screenshotManager = new ScreenshotManager(_language, _settings.Current);


            // UI 绑定
            this.StateChanged += OnStateChanged;
            this.Loaded += (s, e) =>
            {
                LoadSettingsToUI();
                ApplyLanguage();
            };

            LoadIcon();
        }

        private void LoadIcon()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/AutoScreenShotAI;component/ico/camera.ico");
                Icon = System.Windows.Media.Imaging.BitmapFrame.Create(uri);
            }
            catch { }
        }



        #region Settings ↔ UI

        private void LoadSettingsToUI()
        {
            var s = _settings.Current;
            TxtOutputDir.Text = s.OutputDir;
            TxtPythonPath.Text = s.PythonScript;
            TxtFilenameTemplate.Text = s.FilenameTemplate;
            TxtInterval.Text = s.Interval;
            TxtPrompt.Text = s.Prompt;
            TxtAutoDeletionDays.Text = s.AutoDeletionDays;
            ChkAutoStartup.IsChecked = s.IsAutoStartup;
            ChkMinimizeToTray.IsChecked = s.IsMinimizeToTray;
            ChkAutoDelete.IsChecked = s.IsAutoDelete;
            ChkMinimizeRunning.IsChecked = s.IsMinimizeRunning;
            ChkEnableScript.IsChecked = s.IsScript;
            ChkEnableCamera.IsChecked = s.IsCamera;
            CombLanguage.SelectedIndex = s.Language;
            CombScreenshotFormat.SelectedIndex = s.ScreenshotFormat;

            _autoStartupService.SetAutoStartup(s.IsAutoStartup);
            _autoDeleteService.AutoDelete(s.IsAutoDelete, s.OutputDir, s.AutoDeletionDays);
            if (s.IsMinimizeRunning) RunMinimizeRunning();
            _screenshotManager.UpdateSettings(s);
        }

        private void SaveSettingsFromUI()
        {
            var s = _settings.Current;
            s.OutputDir = TxtOutputDir.Text;
            s.PythonScript = TxtPythonPath.Text;
            s.FilenameTemplate = TxtFilenameTemplate.Text;
            s.Interval = TxtInterval.Text;
            s.Prompt = TxtPrompt.Text;
            s.AutoDeletionDays = TxtAutoDeletionDays.Text;
            s.IsAutoStartup = ChkAutoStartup.IsChecked == true;
            s.IsMinimizeToTray = ChkMinimizeToTray.IsChecked == true;
            s.IsAutoDelete = ChkAutoDelete.IsChecked == true;
            s.IsMinimizeRunning = ChkMinimizeRunning.IsChecked == true;
            s.IsScript = ChkEnableScript.IsChecked == true;
            s.IsCamera = ChkEnableCamera.IsChecked == true;
            s.Language = CombLanguage.SelectedIndex;
            s.ScreenshotFormat = CombScreenshotFormat.SelectedIndex;
            _settings.Save();
            _screenshotManager.UpdateSettings(s);
        }

        #endregion

        #region Language

        private void ApplyLanguage()
        {
            bool isChinese = CombLanguage.SelectedIndex == 0;
            _language.SetLanguage(isChinese);
            UpdateUITexts();
        }

        private void UpdateUITexts()
        {
            var t = _language.Texts;
            txtOutputDir.Text = t.OutputDir;
            txtPythonScript.Text = t.PythonScript;
            txtFileNameTemplate.Text = t.FilenameTemplate;
            txtShotInterval.Text = t.ShotInterval;
            txtFormat.Text = t.Format;
            txtPrompt.Text = t.Prompt;
            txtDays.Text = t.Days;
            txtLanguage.Text = t.Language;
            ChkAutoStartup.Content = t.AutoStartup;
            ChkMinimizeRunning.Content = t.MinRunning;
            ChkMinimizeToTray.Content = t.MinToTray;
            ChkEnableScript.Content = t.EnableScript;
            ChkAutoDelete.Content = t.AutoDelete;
            ChkEnableCamera.Content = t.EnableCamera;
            BtnTakeShotText.Text = t.TakeShot;
            RdoRunText.Text = t.Run;
            RdoStopText.Text = t.Stop;
            TxtStatus.Text = t.StatusReady;
            if (string.IsNullOrWhiteSpace(TxtPrompt.Text) || TxtPrompt.Text == _language.Texts.DefaultPrompt)
                TxtPrompt.Text = t.DefaultPrompt;
            // Tips
            TxtOutputDir.ToolTip = t.OutputDirTip;
            TxtPythonPath.ToolTip = t.PythonPathTip;
            TxtFilenameTemplate.ToolTip = t.TemplateTip;
            TxtInterval.ToolTip = t.IntervalTip;
            TxtPrompt.ToolTip = t.PromptTip;
            BtnBrowseOutput.ToolTip = t.BrowseOutputTip;
            BtnOpenOutput.ToolTip = t.OpenOutputTip;
            BtnBrowsePython.ToolTip = t.BrowsePythonTip;
            BtnOpenScript.ToolTip = t.OpenScriptTip;
            BtnResetFileName.ToolTip = t.ResetTemplateTip;
        }

        #endregion

        #region Core Actions

        private async System.Threading.Tasks.Task TakeScreenshotAndAnalyze()
        {
            SetStatus("分析中");
            var status = await _screenshotManager.Capture();
            SetStatus(status);
        }

        private void SetStatus(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                TxtStatus.Text = $"[{DateTime.Now:HH:mm:ss}] {msg}";
            });
        }



        private void RunMinimizeRunning()
        {
            if (ChkMinimizeRunning.IsChecked == true)
            {
                _tray.Show();
                Hide();
                RdoRun.IsChecked = true;
            }
        }

        #endregion

        #region Tray events

        private void OnStateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized && ChkMinimizeToTray.IsChecked == true)
            {
                _tray.Show();
                Hide();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettingsFromUI();
            if (ChkMinimizeToTray.IsChecked == true)
            {
                e.Cancel = true;
                _tray.Show();
                Hide();
            }
            else
            {
                _tray.Hide();
                _screenshotManager.StopTimer();
                Environment.Exit(0);
            }
        }

        #endregion

        #region UI Event Handlers (simple bridges)

        private void BtnBrowseOutput_Click(object s, RoutedEventArgs e) { var dlg = new OpenFolderDialog(); if (dlg.ShowDialog() == true) TxtOutputDir.Text = dlg.FolderName; }
        private void BtnBrowsePython_Click(object s, RoutedEventArgs e) { var dlg = new OpenFileDialog { Filter = "Python Files (*.py)|*.py|All Files (*.*)|*.*" }; if (dlg.ShowDialog() == true) TxtPythonPath.Text = dlg.FileName; }
        private void BtnTakeShot_Click(object s, RoutedEventArgs e) => _ = TakeScreenshotAndAnalyze();
        private void TxtCheckInt_LostFocus(object s, RoutedEventArgs e) { if (!int.TryParse(TxtInterval.Text, out int sec) || sec <= 0) { MessageBox.Show("Positive integer required"); RdoStop.IsChecked = true; } }
        private void TxtAutoDeletionDays_LostFocus(object s, RoutedEventArgs e) { if (!int.TryParse(TxtAutoDeletionDays.Text, out int d) || d <= 0) MessageBox.Show("Positive integer required"); }
        private void RdoRun_Checked(object s, RoutedEventArgs e) { if (!int.TryParse(TxtInterval.Text, out int sec) || sec <= 0) { MessageBox.Show("Positive integer required"); RdoStop.IsChecked = true; return; } _screenshotManager.StartTimer(sec); SetStatus("Interval started"); }
        private void RdoRun_Unchecked(object s, RoutedEventArgs e) { _screenshotManager?.StopTimer(); SetStatus("Interval stopped"); }
        private void ChkAutoStartup_Checked(object s, RoutedEventArgs e) => _autoStartupService.SetAutoStartup(true);
        private void ChkAutoStartup_Unchecked(object s, RoutedEventArgs e) => _autoStartupService.SetAutoStartup(false);
        private void CombLanguage_SelectionChanged(object s, SelectionChangedEventArgs e) { if (IsLoaded) ApplyLanguage(); }
        private void BtnOpenOutput_Click(object s, RoutedEventArgs e) { if (Directory.Exists(TxtOutputDir.Text)) Process.Start("explorer.exe", TxtOutputDir.Text); }
        private void BtnOpenScript_Click(object s, RoutedEventArgs e) { string p = TxtPythonPath.Text; if (File.Exists(p)) Process.Start("explorer.exe", Path.GetDirectoryName(p)!); }
        private void BtnResetFileName_Click(object s, RoutedEventArgs e) => TxtFilenameTemplate.Text = "yyyy-MM-dd HH-mm-ss";

        // 无用事件保留空白，以防 XAML 引用
        private void CombScreenshotFormat_SelectionChanged(object s, SelectionChangedEventArgs e) { }
        private void ChkMinimizeToTray_Checked(object s, RoutedEventArgs e) { }
        private void ChkMinimizeToTray_Unchecked(object s, RoutedEventArgs e) { }
        private void ChkAutoDelete_Checked(object s, RoutedEventArgs e) { }
        private void ChkAutoDelete_Unchecked(object s, RoutedEventArgs e) { }
        private void ChkMinimizeRunning_Checked(object s, RoutedEventArgs e) { }
        private void ChkMinimizeRunning_Unchecked(object s, RoutedEventArgs e) { }
        private void ChkEnableScript_Checked(object s, RoutedEventArgs e) { }
        private void ChkEnableScript_Unchecked(object s, RoutedEventArgs e) { }
        private void TxtPrompt_LostFocus(object s, RoutedEventArgs e) { }
        private void ChkEnableCamera_Checked(object sender, RoutedEventArgs e) { }
        private void ChkEnableCamera_Unchecked(object sender, RoutedEventArgs e) { }
        #endregion

    }
}

