using System.Windows;
using System.Windows.Data;
using ITEQ2.View;
using Microsoft.Win32;


namespace ITEQ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnNormal_Click(object sender, RoutedEventArgs e)
        {
            NormalWindow normalWindow = new NormalWindow();
            normalWindow.Show(); 
        }

        private void btnModal_Click(object sender, RoutedEventArgs e)
        {
            ModalWindow modalWindow = new ModalWindow(this);
            Opacity = 0.4;
            modalWindow.ShowDialog();
            Opacity = 1;

            if (modalWindow.Success)
            {
                //txtInput.Text = modalWindow.Input;
            }
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MenuBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void btnImportCSV_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();

            openFile.Filter = "Csv Files| *.csv";
            if (openFile.ShowDialog() == true)
            {
                var csvData = CSVData.GetCsvData(openFile.FileName);

                CsvDataGrid.ItemsSource = csvData;
            }
        }
    }
}