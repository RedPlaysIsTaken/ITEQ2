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
    /// <summary>
    /// Interaction logic for Footer_Control.xaml
    /// </summary>
    public partial class Footer_Control : UserControl
    {
        public static readonly DependencyProperty WorkingDocPathProperty =
            DependencyProperty.Register("WorkingDocPath", typeof(string), typeof(Footer_Control),
                new PropertyMetadata(string.Empty, OnWorkingDocPathChanged));

        public static readonly DependencyProperty FucDocPathProperty =
            DependencyProperty.Register("FucDocPath", typeof(string), typeof(Footer_Control),
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
        // Private strings to hold the file names and last modified dates
        private string _workingDocFileName;
        private string _workingDocLastModified;
        private string _fucDocFileName;
        private string _fucDocLastModified;


        private static void OnWorkingDocPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Footer_Control)d;
            var path = e.NewValue as string;

            control.WorkingDocFileName = System.IO.Path.GetFileName(path);

            if (File.Exists(path))
            {
                control.WorkingDocLastModified = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm");
            }
            else
            {
                control.WorkingDocLastModified = string.Empty;
            }
        }

        private static void OnFucDocPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (Footer_Control)d;
            var path = e.NewValue as string;

            control.FucDocFileName = System.IO.Path.GetFileName(path);

            if (File.Exists(path))
            {
                control.FucDocLastModified = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm");
            }
            else
            {
                control.FucDocLastModified = string.Empty;
            }

        }

        public Footer_Control()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
