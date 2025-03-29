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
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;


namespace ITEQ2
{
    public partial class MainWindow : Window
    {
        private List<UnifiedModel> UnifiedModel;  // Original dataset
        private List<UnifiedModel> SearchRecords;   // Filtered dataset (after a search)

        private GridViewColumnHeader _lastHeaderClicked = null; // Keeps track of the last header clicked
        private ListSortDirection _lastDirection = ListSortDirection.Ascending; // Switches between ascending and descending when filtering

        private Dictionary<UnifiedModel, Dictionary<string, object>> _modifiedRecords = new(); // Keeps track of what fields have been changed and what they have been changed to

        private string _workingDocPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workingDoc.csv"); // local variable for the path of the working document
        private string _fucDocPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fucReportExampleData.csv"); // local variable for the path of the fucreport

        public MainWindow() // Main program window
        {
            InitializeComponent(); // Start/open the main window.

            SearchBarControl.SearchPerformed += OnSearchPerformed; // Check if the save event has been called from the SearchBar
            MenuBarControl.SaveRequested += OnSaveRequested; // Check if the save event has been called from the MenuBar
        }

        private void TitleBar_Loaded(object sender, RoutedEventArgs e) // Executes when the titlebar loads (must be here for it to work apparantly)
        {

        }
        private void SearchBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void MenuBar_Loaded(object sender, RoutedEventArgs e) // Executes when the menubar loads (must be here for it to work apparantly)
        {
            String[] filePaths = {_workingDocPath, _fucDocPath};

            MenuBar menuBarInstance = this.FindName("MenuBarControl") as MenuBar;

            if (menuBarInstance != null)
            {
                menuBarInstance.openAndIdentifyFiles(filePaths);
                LoadData(UnifiedModel);
            }
            else
            {
                MessageBox.Show("MenuBar instance not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            LoadData(UnifiedModel);
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
                UnifiedModel = unifiedData; // Create UnifiedModel variable to work with, so we dont change the original unifiedData

                foreach (var record in UnifiedModel) // loop through and link each record in the UnifiedModel with a propertyChanged event. THis way we can "notice" when a field is changed.
                {
                    record.PropertyChanged += OnPropertyChanged;
                }

                GenerateDynamicColumns(); // Call the GenerateDynamicColumns() method
                UnifiedListView.ItemsSource = UnifiedModel; // updates the UI
            }
            
        }
        private void GenerateDynamicColumns() // generates Columns based on the loaded data from LoadData()
        {
            if (UnifiedModel == null || !UnifiedModel.Any()) //if there is no data, quit
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
                    textBoxFactory.SetValue(TextBox.WidthProperty, 200.0); // Set the width to Auto

                    template.VisualTree = textBoxFactory; // sets the columns cell content into the textbox

                    
                    
                    GridViewColumn column = new() // create new column
                    {
                        Header = header,  //column header is set to the header with property name tag
                        CellTemplate = template,  //uses the template above
                        Width = Double.NaN, // sets the width of the column
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
            if (UnifiedModel == null || !UnifiedModel.Any()) return;

            var propertyInfo = typeof(UnifiedModel).GetProperty(propertyName);
            if (propertyInfo == null) return;

            UnifiedModel = direction == ListSortDirection.Ascending
                ? UnifiedModel.OrderBy(x => propertyInfo.GetValue(x, null)).ToList()
                : UnifiedModel.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();

            UnifiedListView.ItemsSource = null;
            UnifiedListView.ItemsSource = UnifiedModel;
        }
        private void OnSearchPerformed(string query)
        {
            if (UnifiedModel == null) return;

            if (string.IsNullOrWhiteSpace(query))
            {
                UnifiedListView.ItemsSource = UnifiedModel; // Restore original data
            }
            else
            {
                SearchRecords = UnifiedModel
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

                    property.SetValue(model, newValue); // updates the unifiedRecords with the new value
                }
                System.Diagnostics.Debug.WriteLine($"Property {e.PropertyName} changed. New Value: {property?.GetValue(model)}");
            }
        }
        private void OnSaveRequested()
        {
            if (string.IsNullOrEmpty(_workingDocPath))
            {
                MessageBox.Show("No file loaded. Cannot save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveChangesToCsv(_workingDocPath);
            System.Diagnostics.Debug.WriteLine($"Saving to: {_workingDocPath}");
        }
        public void SaveChangesToCsv(string filePath)
        {
            if (UnifiedModel == null || !UnifiedModel.Any())
            {
                MessageBox.Show("No records to save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                Quote = '"',
                Escape = '"',
                ShouldQuote = args => args.Field.Contains(",") || args.Field.Contains("\"")
            };

            try
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, config);

                csv.Context.RegisterClassMap<UnifiedModelMap>(); // only writes specific lines (skips fuc results when saving)

                csv.WriteRecords(UnifiedModel);

                MessageBox.Show("Changes saved successfully! with the new method", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message} with new method", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public sealed class UnifiedModelMap : ClassMap<UnifiedModel>
        {
            public UnifiedModelMap()
            {
                Map(m => m.Column).Name("Column");
                Map(m => m.GgLabel).Name("GG-LABEL");
                Map(m => m.Type).Name("TYPE");
                Map(m => m.Make).Name("MAKE");
                Map(m => m.Model).Name("MODEL");
                Map(m => m.SerialNo).Name("SERIAL NO");
                Map(m => m.SecurityId).Name("SECURITY ID");
                Map(m => m.User).Name("User");
                Map(m => m.Site).Name("Site");
                Map(m => m.Status).Name("Status");
                Map(m => m.PurchaseDate).Name("Purchase date");
                Map(m => m.Received).Name("Recieved");
                Map(m => m.ShortComment).Name("Short comment");
            }
        }
    }
}