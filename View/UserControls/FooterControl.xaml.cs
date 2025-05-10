using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.ComponentModel;

namespace ITEQ2.View.UserControls
{
    public partial class FooterControl : UserControl
    {

        private string _workingDocFileName;
        private string _workingDocLastModified;
        private string _fucDocFileName;
        private string _fucDocLastModified;

        private double LastZoomValue = 1.0;
        public event Action ZoomResetRequested;
        public event Action<double> ZoomChangedByWheel;
        public event Action<double> ZoomChanged;


        public FooterControl()
        {
            InitializeComponent();
            this.DataContext = this;
            ZoomSlider.ValueChanged += (s, e) => ZoomChanged?.Invoke(e.NewValue);
        }



        public static readonly DependencyProperty WorkingDocPathProperty =
            DependencyProperty.Register("WorkingDocPath", typeof(string), typeof(FooterControl),
                new PropertyMetadata(string.Empty, OnWorkingDocPathChanged));

        public static readonly DependencyProperty FucDocPathProperty =
            DependencyProperty.Register("FucDocPath", typeof(string), typeof(FooterControl),
                new PropertyMetadata(string.Empty, OnFucDocPathChanged));

        public string WorkingDocPath
        {
            get { return (string)GetValue(WorkingDocPathProperty); }
            set { SetValue(WorkingDocPathProperty, value); }
        }
        public string FucDocPath
        {   
            get { return (string)GetValue(FucDocPathProperty); }
            set { SetValue(FucDocPathProperty, value); }
        }

        public string WorkingDocFileName
        {
            get => _workingDocFileName;
            private set
            {
                _workingDocFileName = value;
                OnPropertyChanged(nameof(WorkingDocFileName));
            }
        }
        
        public string WorkingDocLastModified
        {
            get => _workingDocLastModified;
            private set
            {
                _workingDocLastModified = value;
                OnPropertyChanged(nameof(WorkingDocLastModified));
            }
        }
        
        public string FucDocFileName
        {
            get => _fucDocFileName;
            private set
            {
                _fucDocFileName = value;
                OnPropertyChanged(nameof(FucDocFileName));
            }
        }
        
        public string FucDocLastModified
        {
            get => _fucDocLastModified;
            private set
            {
                _fucDocLastModified = value;
                OnPropertyChanged(nameof(FucDocLastModified));
            }
        }



        private static void OnWorkingDocPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooterControl)d;
            var path = e.NewValue as string;

            control.WorkingDocFileName = System.IO.Path.GetFileName(path);

            if (File.Exists(path))
            {
                control.WorkingDocLastModified = File.GetLastWriteTime(path).ToString("dd-MM-yyyy HH:mm");
            }
            else
            {
                control.WorkingDocLastModified = string.Empty;
            }
        }

        private static void OnFucDocPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FooterControl)d;
            var path = e.NewValue as string;

            control.FucDocFileName = System.IO.Path.GetFileName(path);

            if (File.Exists(path))
            {
                control.FucDocLastModified = File.GetLastWriteTime(path).ToString("dd-MM-yyyy HH:mm");
            }
            else
            {
                control.FucDocLastModified = string.Empty;
            }

        }

        

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            double zoom = e.NewValue;

            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.GridZoomTransform.ScaleX = zoom;
                mainWindow.GridZoomTransform.ScaleY = zoom;
            }

        }
        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = 1.0;
        }
    }
}
