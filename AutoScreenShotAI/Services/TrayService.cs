using AutoScreenShotAI.Helpers;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;

namespace AutoScreenShotAI.Services
{
    public class TrayService : IDisposable
    {
        private readonly Window _window;
        private bool _visible;
        private IntPtr _hwnd;
        private Icon? _icon;

        public event Action? OnRestoreRequested;
        public event Action? OnExitRequested;

        public TrayService(Window window)
        {
            _window = window;

            OnRestoreRequested += RestoreFromTray;

            OnExitRequested += () =>
            {
                Hide();
                Environment.Exit(0);
            };
        }

        private void RestoreFromTray()
        {
            Hide();
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();
        }
        public void Show()
        {
            if (_visible) return;

            _hwnd = new WindowInteropHelper(_window).Handle;
            if (_hwnd == IntPtr.Zero) return;

            HwndSource.FromHwnd(_hwnd)?.AddHook(WndProc);

            var data = new Win32Helper.NOTIFYICONDATA
            {
                cbSize = System.Runtime.InteropServices.Marshal.SizeOf<Win32Helper.NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = 1,
                uFlags = Win32Helper.NIF_MESSAGE | Win32Helper.NIF_ICON | Win32Helper.NIF_TIP,
                uCallbackMessage = Win32Helper.WM_USER + 1,
                szTip = "AutoScreenShotAI"
            };

            // 加载图标（从嵌入资源）
            try
            {
                var uri = new Uri("pack://application:,,,/AutoScreenShotAI;component/ico/camera.ico");
                using var stream = Application.GetResourceStream(uri)?.Stream;
                if (stream != null)
                {
                    _icon = new Icon(stream);
                    data.hIcon = _icon.Handle;
                }
            }
            catch { /* fallback below */ }

            if (data.hIcon == IntPtr.Zero)
            {
                try
                {
                    string exePath = Environment.ProcessPath ?? System.Reflection.Assembly.GetEntryAssembly()?.Location ?? "";
                    _icon = Icon.ExtractAssociatedIcon(exePath);
                    if (_icon != null) data.hIcon = _icon.Handle;
                }
                catch { }
            }

            Win32Helper.Shell_NotifyIcon(Win32Helper.NIM_ADD, ref data);
            _visible = true;
        }

        public void Hide()
        {
            if (!_visible) return;

            var data = new Win32Helper.NOTIFYICONDATA
            {
                cbSize = System.Runtime.InteropServices.Marshal.SizeOf<Win32Helper.NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = 1
            };
            Win32Helper.Shell_NotifyIcon(Win32Helper.NIM_DELETE, ref data);

            _icon?.Dispose();
            _icon = null;
            _visible = false;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_USER + 1)
            {
                uint mouseMsg = (uint)lParam.ToInt32();
                if (mouseMsg == Win32Helper.WM_LBUTTONDBLCLK)
                {
                    OnRestoreRequested?.Invoke();
                    handled = true;
                }
                else if (mouseMsg == Win32Helper.WM_RBUTTONUP)
                {
                    ShowContextMenu();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        private void ShowContextMenu()
        {
            var menu = new System.Windows.Controls.ContextMenu();
            var openItem = new System.Windows.Controls.MenuItem { Header = "Open" };
            openItem.Click += (_, _) => OnRestoreRequested?.Invoke();

            var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
            exitItem.Click += (_, _) => OnExitRequested?.Invoke();

            menu.Items.Add(openItem);
            menu.Items.Add(new System.Windows.Controls.Separator());
            menu.Items.Add(exitItem);
            menu.IsOpen = true;
        }

        public void Dispose()
        {
            Hide();
        }
    }
}