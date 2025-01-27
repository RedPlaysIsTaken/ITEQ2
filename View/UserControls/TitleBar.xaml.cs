using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

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

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
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
                    // Handle double-click to maximize/restore
                    window.WindowState = window.WindowState == WindowState.Maximized
                        ? WindowState.Normal
                        : WindowState.Maximized;
                }
                else
                {
                    // Record the starting position of the mouse
                    _startPoint = e.GetPosition(window);
                    _isDragging = false;

                    if (window.WindowState == WindowState.Maximized)
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
                    if (window.WindowState == WindowState.Maximized)
                    {
                        // Restore the window to Normal state
                        window.WindowState = WindowState.Normal;

                        // Calculate the new window position proportionally
                        var screenMousePosition = e.GetPosition(window);
                        window.Left = screenMousePosition.X - (window.ActualWidth * _relativeX);
                        window.Top = screenMousePosition.Y - (window.ActualHeight * _relativeY);
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
