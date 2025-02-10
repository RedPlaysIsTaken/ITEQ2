using ITEQ2.CsvHandling;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

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

        private void menuitemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog // Open file dialog for CSV files omly
            {
                Multiselect = true,
                Filter = "Csv Files|*.csv"
            };

            if (openFile.ShowDialog() == true)
            {
                if (openFile.FileNames.Length != 2)  // Make sure 2 CSV files are selected
                {
                    MessageBox.Show("You must select 2 CSV files.", "File Selection Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Create lists to store parsed data
                List<FucModel> fucData = new();
                List<ITEQModel> iteqData = new();


                foreach (string filePath in openFile.FileNames) // Loop through selected files
                {
                    Path path = new Path { FilePath = filePath };
                    CSVHandler csvHandler = new CSVHandler(path);

                    if (filePath.Contains("Fuc")) // Dette er ganske drit og er midlertidig (Ser om filnavnet har "fuc" i seg)
                    {
                        fucData = csvHandler.ReadFile<FucModel>(path);
                    }
                    else
                    {
                        iteqData = csvHandler.ReadFile<ITEQModel>(path);
                    }
                }

                if (fucData.Count == 0 || iteqData.Count == 0) // Check if the files loaded correctly
                {
                    MessageBox.Show("One or both files failed to load correctly. Check if the files have the correct format.", "File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var mergedData = DataMatcher.MatchAndMerge(fucData, iteqData); // Match and merge the data using the DataMatcher class
                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();

                if (mainWindow != null)
                {
                    DataMatcher.LoadData(mergedData, mainWindow);
                }
            }
        }
    }
}
