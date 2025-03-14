using System.Windows;
using System.Windows.Data;
using ITEQ2.View;
using Microsoft.Win32;
using ITEQ2.CsvHandling;
using ITEQ2.View.UserControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Xml.Linq;
using System.ComponentModel;
using System.IO;


namespace ITEQ2
{
    public partial class MainWindow : Window
    {
        private List<UnifiedModel> UnifiedRecords;  // Original dataset
        private List<UnifiedModel> SearchRecords;   // Filtered dataset (after a search)

        private GridViewColumnHeader _lastHeaderClicked = null; // Keeps track of the last header clicked
        private ListSortDirection _lastDirection = ListSortDirection.Ascending; // Switches between ascending and descending when filtering

        private Dictionary<UnifiedModel, Dictionary<string, object>> _modifiedRecords = new(); // Keeps track of what fields have been changed and what they have been changed to

        public MainWindow() // Main program window
        {
            InitializeComponent(); // Start/open the main window.

            SearchBarControl.SearchPerformed += OnSearchPerformed; // Check if the save event has been called from the SearchBar
            MenuBarControl.SaveRequested += OnSaveRequested; // Check if the save event has been called from the MenuBar
        }

        // Old open window code below

        //private void btnNormal_Click(object sender, RoutedEventArgs e)
        //{
        //    NormalWindow normalWindow = new NormalWindow();
        //    normalWindow.Show(); 
        //}

        // Old open window code below

        //private void btnModal_Click(object sender, RoutedEventArgs e)
        //{
        //    ModalWindow modalWindow = new ModalWindow(this);
        //    Opacity = 0.4;
        //    modalWindow.ShowDialog();
        //    Opacity = 1;

        //    if (modalWindow.Success)
        //    {
        //        //txtInput.Text = modalWindow.Input;
        //    }
        //}

        private void TitleBar_Loaded(object sender, RoutedEventArgs e) // Executes when the titlebar loads (must be here for it to work apparantly)
        {

        }

        private void MenuBar_Loaded(object sender, RoutedEventArgs e) // Executes when the menubar loads (must be here for it to work apparantly)
        {

        }



        public void LoadData(List<UnifiedModel> unifiedData) // Loads the data from the UnifiedModel and generate columns based on the data.
        {
            if (unifiedData == null || !unifiedData.Any()) // If the unifiedData is null, give error message
            {
                MessageBox.Show("No data available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else // if unifiedData has data -->
            {
                UnifiedRecords = unifiedData; // Create UnifiedRecords variable to work with, so we dont change the original unifiedData

                foreach (var record in UnifiedRecords) // loop through and link each record in the UnifiedRecords with a propertyChanged event. THis way we can "notice" when a field is changed.
                {
                    record.PropertyChanged += OnPropertyChanged;
                }

                GenerateDynamicColumns(); // Call the GenerateDynamicColumns() method
                UnifiedListView.ItemsSource = UnifiedRecords; // updates the UI
            }
            
        }

        private void GenerateDynamicColumns() // generates Columns based on the loaded data from LoadData()
        {
            if (UnifiedRecords == null || !UnifiedRecords.Any()) //if there is no data, quit
            {  
                return; 
            }
            else // if there is data -->
            {
                if (UnifiedListView.View == null) // if the listview is empty, create a new one
                {
                    UnifiedListView.View = new GridView();
                }

                GridView gridView = (GridView)UnifiedListView.View; // make GridView accessable inside the Unifiedlistview
                gridView.Columns.Clear(); // clear the grid columns before doing changes to prevent duplications

                PropertyInfo[] properties = typeof(UnifiedModel).GetProperties(); // gets the properties from the unifiedmodel

                foreach (PropertyInfo property in properties) // for each of the properties create a new editable data template
                {
                    
                    GridViewColumnHeader header = new GridViewColumnHeader // create header with sorting
                    {
                        Content = property.Name,
                        Tag = property.Name     // give property name for sorting
                    };

                    header.Click += GridViewColumnHeader_Click; // click handler for the header

                   
                    DataTemplate template = new DataTemplate(); // template to dictate how a cell will look        
                    FrameworkElementFactory textBoxFactory = new FrameworkElementFactory(typeof(TextBox)); // creates dynamically created textboxes
                    textBoxFactory.SetBinding(TextBox.TextProperty, new Binding(property.Name) { Mode = BindingMode.TwoWay }); // bind each textbox to their respective property from the unified model. Additionaly TwoWay makes it update the unified model 
                    template.VisualTree = textBoxFactory; // sets the columns cell content into the textbox

                    
                    GridViewColumn column = new() // create new column
                    {
                        Header = header,  //column header is set to the header with property name tag
                        CellTemplate = template,  //uses the template above
                        Width = 150
                    };

                    gridView.Columns.Add(column);// adds the column into the gridView
                }
            }
        }
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is GridViewColumnHeader header && header.Tag is string propertyName)
            {
                ListSortDirection direction = (_lastHeaderClicked == header && _lastDirection == ListSortDirection.Ascending)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;

                _lastHeaderClicked = header;
                _lastDirection = direction;

                Sort(propertyName, direction);
            }
        }
        private void Sort(string propertyName, ListSortDirection direction)
        {
            if (UnifiedRecords == null || !UnifiedRecords.Any()) return;

            var propertyInfo = typeof(UnifiedModel).GetProperty(propertyName);
            if (propertyInfo == null) return;

            UnifiedRecords = direction == ListSortDirection.Ascending
                ? UnifiedRecords.OrderBy(x => propertyInfo.GetValue(x, null)).ToList()
                : UnifiedRecords.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();

            UnifiedListView.ItemsSource = null;
            UnifiedListView.ItemsSource = UnifiedRecords;
        }

        private void SearchBar_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void OnSearchPerformed(string query)
        {
            if (UnifiedRecords == null) return;

            if (string.IsNullOrWhiteSpace(query))
            {
                UnifiedListView.ItemsSource = UnifiedRecords; // Restore original data
            }
            else
            {
                SearchRecords = UnifiedRecords
                    .Where(item => item.GetType().GetProperties()
                        .Any(prop => prop.GetValue(item)?.ToString()
                            .IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();

                UnifiedListView.ItemsSource = SearchRecords;
            }
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is UnifiedModel model)
            {
                if (!_modifiedRecords.ContainsKey(model))
                    _modifiedRecords[model] = new Dictionary<string, object>();

                PropertyInfo property = model.GetType().GetProperty(e.PropertyName);
                if (property != null)
                {
                    object newValue = property.GetValue(model);
                    _modifiedRecords[model][e.PropertyName] = newValue;
                }
            }
        }
        private string _currentFilePath = "C:\\Users\\123st\\source\\repos\\BAC3030\\Temp\\Combined.csv";
        private void OnSaveRequested()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show("No file loaded. Cannot save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveChangesToCsv(_currentFilePath); // Call the existing save method
        }
        public void SaveChangesToCsv(string filePath)
        {
            if (UnifiedRecords == null || !UnifiedRecords.Any())
            {
                MessageBox.Show("No records to save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var lines = new List<string>
    {
        "GgLabel,User,Type,Make,Model,SerialNo,SecurityId,Site,Status,PurchaseDate,Received,ShortComment,PC,Username,Date,ReportDate,PCLocation,EmplMailAdresse"
    };

            foreach (var record in UnifiedRecords)
            {
                string line = $"{record.GgLabel},{record.User},{record.Type},{record.Make},{record.Model},{record.SerialNo},{record.SecurityId},{record.Site},{record.Status}," +
                              $"{record.PurchaseDate?.ToString("yyyy-MM-dd")},{record.Received?.ToString("yyyy-MM-dd")},{record.ShortComment},{record.PC},{record.Username}," +
                              $"{record.Date?.ToString("yyyy-MM-dd")},{record.ReportDate?.ToString("yyyy-MM-dd")},{record.PCLocation},{record.EmplMailAdresse}";

                lines.Add(line);
            }

            try
            {
                File.WriteAllLines(filePath, lines);
                MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}