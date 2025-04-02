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

namespace ITEQ2.View.UserControls
{
    /// <summary>
    /// Interaction logic for Footer_Control.xaml
    /// </summary>
    public partial class Footer_Control : UserControl
    {
        public static readonly DependencyProperty WorkingDocPathProperty =
            DependencyProperty.Register("WorkingDocPath", typeof(string), typeof(Footer_Control), new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty FucDocPathProperty =
            DependencyProperty.Register("FucDocPath", typeof(string), typeof(Footer_Control), new PropertyMetadata(string.Empty));

        public string WorkingDocPath
        {
            get { return (string)GetValue(WorkingDocPathProperty); }
            set { SetValue(WorkingDocPathProperty, value); }
        }
        public string FucDocPath
        {             get { return (string)GetValue(FucDocPathProperty); }
            set { SetValue(FucDocPathProperty, value); }
        }
        public Footer_Control()
        {
            InitializeComponent();
        }
    }
}
