using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing;
using System.Windows.Media;

namespace ITEQ2.View.UserControls
{
    public partial class TitleBar : UserControl
    {
        private bool _isMaximized = false;
        private Rect _restoreBounds;
        public MenuBar MenuBarControlInstance => MenuBarControl;

        public TitleBar()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.StateChanged += Window_StateChanged;
                }
            };
        }
        
        private void Window_StateChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null) return;

            if (window.WindowState == WindowState.Maximized)
            {
                window.WindowState = WindowState.Normal;

                var workingArea = GetCurrentMonitorWorkingArea(window);
                window.Left = workingArea.Left;
                window.Top = workingArea.Top;
                window.Width = workingArea.Width;
                window.Height = workingArea.Height;
                _isMaximized = true;
            }
        }

        private System.Windows.Point _startPoint; // initial mouse position
        private bool _isDragging;  
        private const int DragThreshold = 1; // distance to activate drag
        private double _relativeX;
        private double _relativeY;

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
                window.WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (_isMaximized)
                {
                    window.Left = _restoreBounds.Left;
                    window.Top = _restoreBounds.Top;
                    window.Width = _restoreBounds.Width;
                    window.Height = _restoreBounds.Height;
                    _isMaximized = false;
                }
                else
                {
                    _restoreBounds = new Rect(window.Left, window.Top, window.Width, window.Height);
                    var workingArea = GetScreenFromWindowCenter(window);

                    window.Left = workingArea.Left;
                    window.Top = workingArea.Top;
                    window.Width = workingArea.Width;
                    window.Height = workingArea.Height;
                    _isMaximized = true;
                }
            }
        }
        private Rect GetScreenFromWindowCenter(Window window)
        {
            var centerPoint = new POINT((int)(window.Left + window.Width / 2), (int)(window.Top + window.Height / 2));
            IntPtr monitor = MonitorFromPoint(centerPoint, 2);

            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            if (GetMonitorInfo(monitor, ref monitorInfo))
            {
                var rc = monitorInfo.rcWork;
                return new Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
            }

            return new Rect(SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Top, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
        }

        private void MenuBar_Loaded(object sender, RoutedEventArgs e)
        {
            //
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);

            if (window != null)
                window.Close();

            //Application.Current.Shutdown();
        }

        private void titleBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);

            if (window != null && e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    btnMaximize_Click(sender, e);
                }
                else
                {
                    // starting position of the mouse
                    _startPoint = e.GetPosition(window);
                    _isDragging = false;

                    if (_isMaximized)
                    {
                        _relativeX = e.GetPosition(window).X / window.ActualWidth;
                        _relativeY = e.GetPosition(window).Y / window.ActualHeight;
                    }
                }
            }
        }

        private void titleBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var window = Window.GetWindow(this);

            if (window != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(window);
                var deltaX = Math.Abs(currentPosition.X - _startPoint.X);
                var deltaY = Math.Abs(currentPosition.Y - _startPoint.Y);

                if (!_isDragging && (deltaX > DragThreshold || deltaY > DragThreshold))
                {
                    if (_isMaximized)
                    {
                        window.Width = 1280;
                        window.Height = 720;

                        var mousePos = Mouse.GetPosition(window);
                        var screenPos = window.PointToScreen(mousePos);

                        window.Left = screenPos.X - (window.Width * _relativeX);
                        window.Top = screenPos.Y - (window.Height * _relativeY);
                        _isMaximized = false;
                    }

                    _isDragging = true;
                    window.DragMove();
                }
            }
        }

        private void titleBar_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _isDragging = false;
        }

        // logic for dynamic maximize regardless of the monitor size

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);// hwnd is the handle to the window
        // tells the program what monitor the window is on, if on two monitors get the nearest one

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        // gets the monitor information, like the size of the monitor and the working area

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct MONITORINFO // fils out the monitor information
        {
            public int cbSize; 
            public RECT rcMonitor; // the size of the monitor
            public RECT rcWork;// the working area of the monitor
            public uint dwFlags;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
        private Rect GetCurrentMonitorWorkingArea(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            IntPtr monitor = MonitorFromWindow(hwnd, 2); 

            MONITORINFO monitorInfo = new MONITORINFO();
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));

            if (GetMonitorInfo(monitor, ref monitorInfo))
            {
                var rc = monitorInfo.rcWork;

                var source = HwndSource.FromHwnd(hwnd);
                double dpiX = 1.0, dpiY = 1.0;

                if (source?.CompositionTarget != null)
                {
                    dpiX = source.CompositionTarget.TransformToDevice.M11;
                    dpiY = source.CompositionTarget.TransformToDevice.M22;
                }

                return new Rect(
                    rc.left / dpiX,
                    rc.top / dpiY,
                    (rc.right - rc.left) / dpiX,
                    (rc.bottom - rc.top) / dpiY
                );
            }

            var fallbackDpi = VisualTreeHelper.GetDpi(window);
            return new Rect(
                SystemParameters.WorkArea.Left / fallbackDpi.DpiScaleX,
                SystemParameters.WorkArea.Top / fallbackDpi.DpiScaleY,
                SystemParameters.WorkArea.Width / fallbackDpi.DpiScaleX,
                SystemParameters.WorkArea.Height / fallbackDpi.DpiScaleY
            );
        }
    }
}
