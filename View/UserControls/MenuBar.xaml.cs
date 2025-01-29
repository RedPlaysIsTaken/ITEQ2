using ITEQ2.CsvHandling;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace ITEQ2.View.UserControls
{
    /// <summary>
    /// Interaction logic for MenuBar.xaml
    /// </summary>
    public partial class MenuBar : UserControl
    {
        public MenuBar()
        {
            InitializeComponent();
        }

        private void menuitemOpenITEQ_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "Csv Files| *.csv";
            if (openFile.ShowDialog() == true)
            {
                string filePath = openFile.FileName;
                ITEQPath iteqPath = new ITEQPath
                {
                    FilePath = filePath
                };

                ITEQCSVHandler csvHandler = new ITEQCSVHandler(iteqPath);
                // Now you can use csvHandler to handle CSV operations
            }
        }

        private void menuitemOpenFuc_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "Csv Files| *.csv";
            if (openFile.ShowDialog() == true)
            {
                string filePath = openFile.FileName;
                FucPath fucPath = new FucPath
                {
                    FilePath = filePath
                };

                FucCSVHandler csvHandler = new FucCSVHandler(fucPath);
                // Now you can use csvHandler to handle CSV operations
            }
        }
    }
}
