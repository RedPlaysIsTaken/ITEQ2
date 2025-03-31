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
        public event Action SaveRequested;

        public MenuBar()
        {
            InitializeComponent();
        }


        public void openAndIdentifyFiles(string[] filePaths)
        {
            // Create lists to store parsed data
            List<FucModel> fucData = new();
            List<ITEQModel> iteqData = new();


            foreach (string filePath in filePaths) // Loop for each file chosen
            {
                Path path = new Path { FilePath = filePath }; // determine path
                CSVHandler csvHandler = new CSVHandler(path); // put path in the CSVHandler class


                var tempFucData = csvHandler.ReadFile<FucModel>(path); // try using the FucModel template first

                if (tempFucData.Count > 0) // if FucModel succeeds
                {
                    fucData = tempFucData; // assign the tempFucData to the List<FucModel> fucData
                    System.Diagnostics.Debug.WriteLine($"File {filePath} identified as FucModel.");
                }
                else // If the FucModel fails, try the ITEQModel
                {

                    var tempIteqData = csvHandler.ReadFile<ITEQModel>(path); // try using the ITEQModel template

                    if (tempIteqData.Count > 0) // if ITEQModel succeeds
                    {
                        iteqData = tempIteqData; // assign the tempITEQData to the List<IEQModel> ITEQData
                        System.Diagnostics.Debug.WriteLine($"File {filePath} identified as ITEQModel.");
                    }
                    else // if BOTH fail, headers are the problem
                    {
                        MessageBox.Show($"Failed to determine format for {filePath}. Ensure it has valid headers.", "File Format Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            if (fucData.Count == 0 || iteqData.Count == 0)
            {
                MessageBox.Show("One or both files failed to load correctly. Check if the files have the correct format.", "File Load Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var mergedData = DataMatcher.MatchAndMerge(fucData, iteqData);

            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                DataMatcher.LoadData(mergedData, mainWindow);
            }
        }

        private void menuitemOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog // Open file dialog for CSV files omly
            {
                Multiselect = true,
                Filter = "Csv Files|*.csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                if (openFileDialog.FileNames.Length != 2)  // Make sure 2 CSV files are selected
                {
                    MessageBox.Show("You must select 2 CSV files.", "File Selection Limit", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                openAndIdentifyFiles(openFileDialog.FileNames);
            }
        }

        private void menuitemSave_Click(object sender, RoutedEventArgs e)
        {
            SaveRequested?.Invoke();
        }

        private void menuitemOpenCombined_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog // Open file dialog for CSV files omly
            {
                Multiselect = false,
                Filter = "Csv Files|*.csv"
            };

            // Create list to store parsed data
            List<EquipmentObject> unifiedData = new();

            Path path = new Path(); // determine path
            CSVHandler csvHandler = new CSVHandler(path); // put path in the CSVHandler class
        }
    }
}