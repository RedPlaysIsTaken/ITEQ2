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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;


namespace ITEQ2
{
    public partial class MainWindow : Window
    {
        //private GridViewColumnHeader _lastHeaderClicked = null; // Keeps track of the last header clicked
        //private ListSortDirection _lastDirection = ListSortDirection.Ascending; // Switches between ascending and descending when filtering


        public ObservableCollection<EquipmentObject> EquipmentList { get; set; } = new();
        public ObservableCollection<EquipmentObject> SearchedEquipmentList { get; set; }// = new();// Filtered dataset (after a search)
        private Dictionary<EquipmentObject, Dictionary<string, object>> _modifiedRecords = new(); // Keeps track of what fields have been changed and what they have been changed to
        private string _workingDocPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workingDoc.csv"); // local variable for the path of the working document
        private string _fucDocPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fucReportExampleData.csv"); // local variable for the path of the fucreport

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public MainWindow() // Main program window
        {
            InitializeComponent(); // Start/open the main window.

            this.DataContext = this;

            EquipmentListView.ItemsSource = EquipmentList;

            SearchBarControl.SearchPerformed += OnSearchPerformed; // Check if the save event has been called from the SearchBar
            MenuBarControl.SaveRequested += OnSaveRequested; // Check if the save event has been called from the MenuBar

            IntializeData();
        }
        private void EquipmentListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EquipmentListView.SelectedItem != null)
            {
                DetailsPanel.Visibility = Visibility.Visible;
                EquipmentListView.Height = double.NaN;
            }
        }
        private void CloseDetailsPanel_Click(object sender, RoutedEventArgs e)
        {
            DetailsPanel.Visibility = Visibility.Collapsed;
            EquipmentListView.Height = double.NaN;
        }
        private void TitleBar_Loaded(object sender, RoutedEventArgs e) // Executes when the titlebar loads (must be here for it to work apparantly)
        {

        }
        private void SearchBar_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void MenuBar_Loaded(object sender, RoutedEventArgs e) // Executes when the menubar loads (must be here for it to work apparantly)
        {

        }
        public void LoadData(ObservableCollection<EquipmentObject> equipmentList)
        {
            if (equipmentList == null || !equipmentList.Any())
            {
                MessageBox.Show("No data available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            else
            {
                EquipmentList = equipmentList; // Ensure EquipmentList is set
                EquipmentListView.ItemsSource = EquipmentList;
                Debug.WriteLine("Items in Equipment list when Loading: " + equipmentList.Count);
            }
        }
        private void OnSearchPerformed(string query)
        {
            if (EquipmentList == null || !EquipmentList.Any())
            {
                Debug.WriteLine("No data to search in");
                return;
            }

            Debug.WriteLine($"Search query: {query}");

            if (string.IsNullOrWhiteSpace(query))
            {
                EquipmentListView.ItemsSource = EquipmentList; // Restore original data
                Debug.WriteLine("Restoring original data");
            }
            else
            {
                var filteredList = EquipmentList
                    .Where(item => item.GetType().GetProperties()
                        .Any(prop => prop.GetValue(item)?.ToString()
                            .IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();

                SearchedEquipmentList = new ObservableCollection<EquipmentObject>(filteredList);
                EquipmentListView.ItemsSource = SearchedEquipmentList;

                Debug.WriteLine($"Filtered list count: {SearchedEquipmentList.Count}");
            }
        }

        private void OnSaveRequested()
        {

            if (string.IsNullOrEmpty(_workingDocPath))
            {
                MessageBox.Show("Save path not found, cannot save.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SaveChangesToCsv(_workingDocPath);
            System.Diagnostics.Debug.WriteLine($"Saving to: {_workingDocPath}");
        }
        public void SaveChangesToCsv(string filePath)
        {
            EquipmentListView.ItemsSource = EquipmentList;
            Debug.WriteLine("Items in listview when saving: " + EquipmentListView.Items.Count);
            //System.Windows.Controls.ListView Items.Count

            if (EquipmentListView.Items.Count == 0)
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
                List<EquipmentObject> equipmentListCopy = EquipmentListView.Items.Cast<EquipmentObject>().ToList();

                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, config);

                csv.Context.RegisterClassMap<UnifiedModelMap>(); // only writes specific lines (skips fuc results when saving)

                csv.WriteRecords(equipmentListCopy);

                MessageBox.Show("Changes saved successfully! with the new method", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving file: {ex.Message} with new method", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public sealed class UnifiedModelMap : ClassMap<EquipmentObject>
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

        private void EquipmentListView_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                    var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                    Sort(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                          Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = null;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }
        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView =
              CollectionViewSource.GetDefaultView(EquipmentListView.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }
        private void IntializeData()
        {
            String[] filePaths = { _workingDocPath, _fucDocPath };

            MenuBar menuBarInstance = this.FindName("MenuBarControl") as MenuBar;

            if (menuBarInstance != null)
            {
                if (File.Exists(_workingDocPath) && File.Exists(_fucDocPath))
                {
                    menuBarInstance.openAndIdentifyFiles(filePaths);
                }
                else
                {
                    MessageBox.Show("Missing one or both CSV-files!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("MenuBar instance not found!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}