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

        private void menuitemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Csv Files|*.csv"
            };

            if (openFile.ShowDialog() == true)
            {
                if (openFile.FileNames.Length != 2)  // Ensure exactly 2 files are selected
                {
                    MessageBox.Show("You must select exactly 2 files.", "File Selection Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Lists to store parsed data
                List<FucModel> fucData = new();
                List<ITEQModel> iteqData = new();

                foreach (string filePath in openFile.FileNames)
                {
                    Path path = new Path { FilePath = filePath };
                    CSVHandler csvHandler = new CSVHandler(path);

                    // Determine which model to use based on filename pattern
                    if (filePath.Contains("Fuc"))
                        fucData = csvHandler.ReadFile<FucModel>(path);
                    else
                        iteqData = csvHandler.ReadFile<ITEQModel>(path);
                }

                // Ensure both files were correctly loaded
                if (fucData.Count == 0 || iteqData.Count == 0)
                {
                    MessageBox.Show("One or both files failed to load correctly.", "File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Step 1: Match & Merge the Data
                var mergedData = DataMatcher.MatchAndMerge(fucData, iteqData);

                // Step 2: Display in DataGrid
                // CsvDataGrid.ItemsSource = mergedData;

                // Step 3: Save the merged data
                string savePath = DataMatcher.ShowSaveFileDialog();
                if (!string.IsNullOrEmpty(savePath))
                {
                    DataMatcher.SaveToCsv(mergedData, savePath);
                    MessageBox.Show($"CSV file saved to: {savePath}", "Save Successful", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

    }
}
