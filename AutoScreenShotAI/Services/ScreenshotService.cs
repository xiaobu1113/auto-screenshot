using AutoScreenShotAI.Helpers;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace AutoScreenShotAI.Services
{
    public static class ScreenshotService
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
}