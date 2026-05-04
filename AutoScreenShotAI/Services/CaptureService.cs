using AForge.Video.DirectShow;
using AutoScreenShotAI.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Threading;

namespace AutoScreenShotAI.Services
{
    public static class CaptureService
    {
        public static Bitmap CaptureScreen()
        {
            int left = Win32Helper.GetSystemMetrics(Win32Helper.SM_XVIRTUALSCREEN);
            int top = Win32Helper.GetSystemMetrics(Win32Helper.SM_YVIRTUALSCREEN);
            int width = Win32Helper.GetSystemMetrics(Win32Helper.SM_CXVIRTUALSCREEN);
            int height = Win32Helper.GetSystemMetrics(Win32Helper.SM_CYVIRTUALSCREEN);

            var bmp = new Bitmap(width, height);
            using var g = Graphics.FromImage(bmp);
            g.CopyFromScreen(left, top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            return bmp;
        }

        public static Bitmap CaptureUser()
        {
            // 枚举所有摄像头设备
            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            if (videoDevices.Count == 0)
                throw new Exception("未检测到摄像头");

            // 使用第一个摄像头
            var device = new VideoCaptureDevice(videoDevices[0].MonikerString);
            device.Start();

            // 等待一帧（最多2秒）
            Bitmap frame = null;
            var waitHandle = new ManualResetEvent(false);

            device.NewFrame += (sender, eventArgs) =>
            {
                // 克隆帧，避免被覆盖
                frame = (Bitmap)eventArgs.Frame.Clone();
                waitHandle.Set();
            };

            waitHandle.WaitOne(2000);
            device.SignalToStop();
            device.WaitForStop();

            if (frame == null)
                throw new Exception("获取摄像头图像超时");

            return frame;
        }
        public static ImageFormat GetImageFormat(int formatIndex)
        {
            return formatIndex switch
            {
                1 => ImageFormat.Png,
                _ => ImageFormat.Jpeg
            };
        }

        public static string GetExtension(int formatIndex) => formatIndex switch
        {
            1 => ".png",
            _ => ".jpg"
        };

        public static string BuildFilePath(string outputDir, string template, int formatIndex)
        {
            string folder = DateTime.Now.ToString("yyyy-MM-dd");
            string outputFolder = Path.Combine(outputDir, folder);
            Directory.CreateDirectory(outputFolder);

            // template 验证在 UI 层已做
            string filename = DateTime.Now.ToString(template);
            string ext = GetExtension(formatIndex);
            if (!filename.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
                filename += ext;

            return Path.Combine(outputFolder, filename);
        }

    }

    public class ScreenshotManager
    {
        private readonly LanguageService _language;
        private readonly DispatcherTimer _screenshotTimer = new();
        private AppSettings _settings;

        public ScreenshotManager(LanguageService language, AppSettings initialSettings)
        {
            _language = language;
            _settings = initialSettings;
            _screenshotTimer.Tick += async (s, e) => await OnTimerTick();

        }

        public void UpdateSettings(AppSettings settings)
        {
            _settings = settings;
        }

        private async Task OnTimerTick()
        {
            await Capture();
        }

        public async Task<string> Capture()
        {
            if (_settings == null) return "设置未初始化";

            string _outputDir = _settings.OutputDir;
            string _filenameTemplate = _settings.FilenameTemplate;
            int _screenshotFormat = _settings.ScreenshotFormat;
            bool _enableScript = _settings.IsScript;
            string _pythonScriptPath = _settings.PythonScript;
            string _prompt = _settings.Prompt;
            try
            {
                // 1. 验证目录
                if (string.IsNullOrWhiteSpace(_outputDir) || !Directory.Exists(_outputDir))
                    return "输出目录无效";

                // 2. 验证文件名模板
                if (string.IsNullOrWhiteSpace(_filenameTemplate) || _filenameTemplate.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                    return "文件名模板无效";

                // 3. 截图并保存
                string pcPath = CaptureService.BuildFilePath(_outputDir, "pc_" + _filenameTemplate, _screenshotFormat);
                string mePath = CaptureService.BuildFilePath(_outputDir, "me_" + _filenameTemplate, _screenshotFormat);
                string recordPath = CaptureService.BuildFilePath(_outputDir, "r_" + _filenameTemplate, _screenshotFormat);

                using (var bmp = CaptureService.CaptureScreen())
                {
                    bmp.Save(pcPath, CaptureService.GetImageFormat(_screenshotFormat));
                }

                if (_settings.IsCamera == true)
                {
                    using (var bmp = CaptureService.CaptureUser())
                    {
                        bmp.Save(mePath, CaptureService.GetImageFormat(_screenshotFormat));
                    }
                }

                // 4. 不启用脚本直接返回
                if (!_enableScript)
                    return $"已保存：{Path.GetFileName(pcPath)}";
                else
                {
                    return await RecordCapture(recordPath, _pythonScriptPath, _prompt);
                }
                // 5. 执行Python分析

            }
            catch (Exception ex)
            {
                return $"错误：{ex.Message}";
            }
        }

        private async Task<string> RecordCapture(string recordPath, string _pythonScriptPath, string _prompt)
        {
            if (!File.Exists(_pythonScriptPath))
                return "未选择Python脚本";

            if (string.IsNullOrWhiteSpace(_prompt))
                _prompt = _language.Texts.DefaultPrompt;
            string result = await PythonService.RunScriptAsync(_pythonScriptPath, recordPath, _prompt);
            string mdPath = Path.ChangeExtension(recordPath, ".md");
            File.WriteAllText(mdPath, result, System.Text.Encoding.UTF8);

            return "分析完成";
        }


        public void StartTimer(int intervalSeconds)
        {
            _screenshotTimer.Stop();
            _screenshotTimer.Interval = TimeSpan.FromSeconds(intervalSeconds);
            _screenshotTimer.Start();
        }

        // 停止
        public void StopTimer()
        {
            _screenshotTimer.Stop();
        }
    }

}