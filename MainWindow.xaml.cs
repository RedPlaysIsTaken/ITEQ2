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


namespace ITEQ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<UnifiedModel> UnifiedRecords; // Original dataset
        private List<UnifiedModel> FilteredRecords; // Filtered dataset


        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();
            SearchBarControl.SearchPerformed += OnSearchPerformed; // Subscribe to search event
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



        public void LoadData(List<UnifiedModel> unifiedData)
        {
            if (unifiedData == null || !unifiedData.Any())
            {
                MessageBox.Show("No data available.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UnifiedRecords = unifiedData;
            GenerateDynamicColumns();
            UnifiedListView.ItemsSource = UnifiedRecords;
        }

        private void GenerateDynamicColumns()
        {
            if (UnifiedRecords == null || !UnifiedRecords.Any())
                return;

            if (UnifiedListView.View == null)
                UnifiedListView.View = new GridView();

            GridView gridView = (GridView)UnifiedListView.View;
            gridView.Columns.Clear(); // Clear existing columns

            PropertyInfo[] properties = typeof(UnifiedModel).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                GridViewColumnHeader header = new GridViewColumnHeader()
                {
                    Content = property.Name,
                    Tag = property.Name // Store property name for sorting
                };

                header.Click += GridViewColumnHeader_Click;

                GridViewColumn column = new()
                {
                    Header = header,
                    DisplayMemberBinding = new Binding(property.Name),
                    Width = 150
                };
                gridView.Columns.Add(column);
            }
        }
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (sender is GridViewColumnHeader header && header.Tag is string propertyName)
            {
                ListSortDirection direction = (_lastHeaderClicked == header && _lastDirection == ListSortDirection.Ascending)
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;

                Sort(propertyName, direction);

                _lastHeaderClicked = header;
                _lastDirection = direction;
            }
        }
        private void Sort(string propertyName, ListSortDirection direction)
        {
            if (UnifiedRecords == null) return;

            var propertyInfo = typeof(UnifiedModel).GetProperty(propertyName);
            if (propertyInfo == null) return;

            if (direction == ListSortDirection.Ascending)
                UnifiedRecords = UnifiedRecords.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
            else
                UnifiedRecords = UnifiedRecords.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();

            //UnifiedListView.ItemsSource = null; // Reset source
            UnifiedListView.ItemsSource = UnifiedRecords; // Update sorted data
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
                FilteredRecords = UnifiedRecords
                    .Where(item => item.GetType().GetProperties()
                        .Any(prop => prop.GetValue(item)?.ToString()
                            .IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0))
                    .ToList();

                UnifiedListView.ItemsSource = FilteredRecords;
            }
        }
    }
}