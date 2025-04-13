using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;

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
        private void Window_StateChanged(object sender, EventArgs e)
        {
            var window = sender as Window;
            if (window == null) return;

            if (window.WindowState == WindowState.Maximized)
            {
                // Prevent actual maximize by restoring immediately
                window.WindowState = WindowState.Normal;

                // Optionally resize manually to simulate a "maximized" look
                var workingArea = SystemParameters.WorkArea;
                window.Left = workingArea.Left;
                window.Top = workingArea.Top;
                window.Width = workingArea.Width;
                window.Height = workingArea.Height;
                _isMaximized = true;
            }
        }

        private Point _startPoint; // Tracks the initial mouse position
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
                    // if the window is maximized, restore it to normal size
                    window.Width = 1280;
                    window.Height = 720;
                    window.Left = (SystemParameters.PrimaryScreenWidth - window.Width) / 2;
                    window.Top = (SystemParameters.PrimaryScreenHeight - window.Height) / 2;
                    _isMaximized = false;
                }
                else
                {
                    // if the window is not maximized, maximize it
                    var workingArea = SystemParameters.WorkArea;
                    window.Left = workingArea.Left;
                    window.Top = workingArea.Top;
                    window.Width = workingArea.Width;
                    window.Height = workingArea.Height;
                    _isMaximized = true; // sets maximized state to true
                }
            }
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


    }
}
