using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Drawing;

namespace ITEQ2.View.UserControls
{
    /// <summary>
    /// Interaction logic for TitleBar.xaml
    /// </summary>
    public partial class TitleBar : UserControl
    {

        public TitleBar()
        {
            InitializeComponent();

            this.Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.StateChanged += Window_StateChanged; // ✅ hook here
                }
            };
        }
        public MenuBar MenuBarControlInstance => MenuBarControl;
        private void Window_StateChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null) return;

            if (window.WindowState == WindowState.Maximized)
            {
                // Prevent the changing of windowstate to maximized
                window.WindowState = WindowState.Normal;

                // Optionally resize manually to simulate a "maximized" look
                var workingArea = GetCurrentMonitorWorkingArea(window);
                window.Left = workingArea.Left;
                window.Top = workingArea.Top;
                window.Width = workingArea.Width;
                window.Height = workingArea.Height;
                _isMaximized = true;
            }
        }

        private System.Windows.Point _startPoint; // Tracks the initial mouse position
        private bool _isDragging;  // Indicates if the drag threshold is met
        private const int DragThreshold = 1; // Minimum distance to activate drag
        private double _relativeX; // Proportional X position
        private double _relativeY; // Proportional Y position

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this); // Get the parent window
            if (window != null)
                window.WindowState = WindowState.Minimized;
        }

        private bool _isMaximized = false;

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                if (_isMaximized)
                {
                    var workingArea = GetCurrentMonitorWorkingArea(window);
                    window.Left = workingArea.Left + workingArea.Width / 6;
                    window.Top = workingArea.Top + workingArea.Height / 6;
                    window.Width = workingArea.Width / 1.5;
                    window.Height = workingArea.Height / 1.5;
                    _isMaximized = false; // sets maximized state to true




                    // if the window is maximized, restore it to normal size
                    /*window.Width = 1280;
                    window.Height = 720;
                    window.Left = (SystemParameters.PrimaryScreenWidth - window.Width) / 2;
                    window.Top = (SystemParameters.PrimaryScreenHeight - window.Height) / 2;
                    _isMaximized = false;*/
                }
                else
                {
                    // if the window is not maximized, maximize it
                    var workingArea = GetCurrentMonitorWorkingArea(window);
                    window.Left = workingArea.Left;
                    window.Top = workingArea.Top;
                    window.Width = workingArea.Width;
                    window.Height = workingArea.Height;
                    _isMaximized = true; // sets maximized state to true
                }
            }
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
            var window = Window.GetWindow(this); // Get the parent window

            if (window != null && e.ChangedButton == System.Windows.Input.MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    btnMaximize_Click(sender, e);
                }
                else
                {
                    // Record the starting position of the mouse
                    _startPoint = e.GetPosition(window);
                    _isDragging = false;

                    if (_isMaximized)
                    {
                        // Calculate proportional position relative to the maximized window
                        _relativeX = e.GetPosition(window).X / window.ActualWidth;  // X ratio
                        _relativeY = e.GetPosition(window).Y / window.ActualHeight; // Y ratio
                    }
                }
            }
        }

        private void titleBar_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var window = Window.GetWindow(this); // Get the parent window

            if (window != null && e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                // Calculate the distance the mouse has moved
                var currentPosition = e.GetPosition(window);
                var deltaX = Math.Abs(currentPosition.X - _startPoint.X);
                var deltaY = Math.Abs(currentPosition.Y - _startPoint.Y);

                // If the mouse has moved beyond the threshold, enable dragging
                if (!_isDragging && (deltaX > DragThreshold || deltaY > DragThreshold))
                {
                    if (_isMaximized)
                    {
                        // Restore the window to Normal state
                        window.Width = 1280;
                        window.Height = 720;

                        // Calculate the new window position proportionally
                        var mousePos = Mouse.GetPosition(window); // Get mouse position relative to the window
                        var screenPos = window.PointToScreen(mousePos); // Convert to screen coordinates

                        window.Left = screenPos.X - (window.Width * _relativeX);
                        window.Top = screenPos.Y - (window.Height * _relativeY);
                        _isMaximized = false;
                    }

                    // Start dragging
                    _isDragging = true;
                    window.DragMove();
                }
            }
        }

        private void titleBar_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Reset drag state on mouse release
            _isDragging = false;
        }

        // logic for dynamic maximize regardless of the monitor size

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);// hwnd is the handle to the window
        // tells the program what monitor the window is on, if on two monitors get the nearest one

        [DllImport("user32.dll")]
        static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
        // gets the monitor information, like the size of the monitor and the working area

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
        private Rect GetCurrentMonitorWorkingArea(Window window)
        {
            var hwnd = new WindowInteropHelper(window).Handle;
            IntPtr monitor = MonitorFromWindow(hwnd, 2); // finds the monitor the window is on or closest to

            MONITORINFO monitorInfo = new MONITORINFO(); //
            monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO)); //Create the monitor info structure and set its size
           
            if (GetMonitorInfo(monitor, ref monitorInfo)) // if getting monitor size is succsesfull sets the rc
            {
                var rc = monitorInfo.rcWork;
                return new Rect(rc.left, rc.top, rc.right - rc.left, rc.bottom - rc.top);
            }
            return new Rect(SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Top, SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);// if we can't get the monitor size, use WPF syspam WorkingArea instead

        }
    }
}
