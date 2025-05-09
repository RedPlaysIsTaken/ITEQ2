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
using System.Data.Common;
using System.Windows.Threading;
using ITEQ2.Presets;
using Microsoft.VisualBasic;
using ITEQ2.View.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media;
using ITEQ2.Logging;


namespace ITEQ2
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<EquipmentObject> EquipmentList { get; set; } = new();// The list of equipment
        public ObservableCollection<EquipmentObject> SearchedEquipmentList { get; set; }// = new();// Filtered dataset (after a search)
        private Dictionary<EquipmentObject, Dictionary<string, object>> _modifiedRecords = new(); // Keeps track of what fields have been changed and what they have been changed to
        private string _workingDocPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workingDoc.csv"); // local variable for the path of the working document
        private string _fucDocPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "fucReportExampleData.csv"); // local variable for the path of the fucreport
        public ObservableCollection<GridViewPreset> GridViewPresets { get; set; } = new();

        GridViewColumnHeader _lastHeaderClicked = null;
        ListSortDirection _lastDirection = ListSortDirection.Ascending;
        double _lastGridHeight = double.NaN;
        double currentGridHeight = 0.0;

        //list of columns with 0 width
        public ObservableCollection<string> HiddenColumns { get; set; } = new ObservableCollection<string>();

        public MainWindow() // Main program window
        {
            InitializeComponent(); // Start/open the main window.

            this.PreviewKeyDown += Window_PreviewKeyDown;

            this.DataContext = this;

            EquipmentListView.ItemsSource = EquipmentList;

           SearchBarControl.SearchPerformed += OnSearchPerformed; // Check if the save event has been called from the SearchBar
           MenuBarControl.SaveRequested += OnSaveRequested; // Check if the save event has been called from the MenuBar

            SearchBar searchBar = SearchBarControl as SearchBar;
            searchBar.ZoomChanged += ApplyZoom;

            PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(
                GridViewColumn.WidthProperty, typeof(GridViewColumn));
            GridView gv = (GridView)EquipmentListView.View;
            foreach (GridViewColumn col in gv.Columns)
            {
                pd.AddValueChanged(col, ColumnWidthChanged);
            }
            IntializeData();
            LoadGridPresets();
            CheckName();
        }
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var focusedElement = Keyboard.FocusedElement;

            if (focusedElement is TextBox || focusedElement is PasswordBox || focusedElement is RichTextBox)
                return;

            if (e.Key == Key.LeftShift || e.Key == Key.RightShift ||
                e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl ||
                e.Key == Key.LeftAlt || e.Key == Key.RightAlt ||
                e.Key == Key.Escape || e.Key == Key.Tab ||
                e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.Left || e.Key == Key.Right ||
                e.Key == Key.Enter || e.Key == Key.CapsLock)
            {
                return;
            }

            if (SearchBarControl is SearchBar searchBar && searchBar.txtBoxSearchBar != null)
            {
                searchBar.txtBoxSearchBar.Focus();
                searchBar.txtBoxSearchBar.SelectAll();
            }
        }

        private void CheckName()
        {
            if (string.IsNullOrWhiteSpace(Properties.Settings.Default.User))
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            }
        }
        private void SubscribeToEquipment(EquipmentObject obj)
        {
            obj.PropertyChanged += EquipmentObject_PropertyChanged;

            // Save initial values (for diffing later)
            if (!_modifiedRecords.ContainsKey(obj))
            {
                _modifiedRecords[obj] = new Dictionary<string, object>();
            }
        }
        private void EquipmentObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is EquipmentObject obj)
            {
                var propInfo = obj.GetType().GetProperty(e.PropertyName);
                if (propInfo != null)
                {
                    object newValue = propInfo.GetValue(obj);
                    var record = _modifiedRecords[obj];

                    record.TryGetValue(e.PropertyName, out object oldValue);

                    if (!Equals(oldValue, newValue))
                    {
                        // Log change
                        LogChanges.AddChange(e.PropertyName, oldValue?.ToString(), newValue?.ToString(), obj.GgLabel);

                        // Update record
                        record[e.PropertyName] = newValue;
                    }
                }
            }

        }
        private void EquipmentListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EquipmentListView.SelectedItem is EquipmentObject selected)
            {
                var logs = LogChanges.LoadLogsForItem(selected.GgLabel);
                ChangeHistoryListBox.ItemsSource = logs;
            }
            else
            {
                Debug.WriteLine("This is where the error is");
            }
        }




        private void ApplyZoom(double zoom)
        {
            AnimateScale(GridZoomTransform, zoom);

            Properties.Settings.Default.GridZoom = zoom;
            Properties.Settings.Default.Save();
        }
        private void AnimateScale(ScaleTransform transform, double targetZoom)
        {
            var duration = TimeSpan.FromMilliseconds(200);

            var scaleXAnim = new DoubleAnimation
            {
                To = targetZoom,
                Duration = new Duration(duration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            var scaleYAnim = new DoubleAnimation
            {
                To = targetZoom,
                Duration = new Duration(duration),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
        }
        private void EquipmentListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            SearchBar searchBar = SearchBarControl as SearchBar;

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double delta = e.Delta > 0 ? 0.1 : -0.1;
                double newZoom = Math.Clamp(searchBar.ZoomSlider.Value + delta, searchBar.ZoomSlider.Minimum, searchBar.ZoomSlider.Maximum);
                searchBar.ZoomSlider.Value = newZoom;
                e.Handled = true;
            }
        }
        private void ColumnWidthChanged(object sender, EventArgs e)
        {
            if (sender is GridViewColumn column)
            {
                string headerText = (column.Header is GridViewColumnHeader header)
                        ? header.Content.ToString()
                        : column.Header.ToString();
                Debug.WriteLine($"Column '{headerText}' has width: '{column.Width}'.");
                if (column.Width == 0)
                {
                    if (!HiddenColumns.Contains(headerText)) 
                    {
                        HiddenColumns.Add(headerText);
                        Debug.WriteLine($"Column '{headerText}' is now hidden.");
                    }
                }
                else
                {
                    if (HiddenColumns.Contains(headerText))
                    {
                        HiddenColumns.Remove(headerText);
                        Debug.WriteLine($"Column '{headerText}' is now visible.");
                    }
                }
            }
        }
        public void SaveGridPreset()
        {
            GridView gv = (GridView)EquipmentListView.View;

            string presetName = Interaction.InputBox("Enter preset name (max 24 characters):", "Save GridView Preset", "MyPreset");

            if (string.IsNullOrWhiteSpace(presetName))
                return;

            if (presetName.Length > 20)
            {
                MessageBox.Show("Preset name cannot exceed 24 characters.", "Invalid Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string description = Interaction.InputBox("Enter description (optional):", "Save GridView Preset Description", "");

            GridViewPresetManager.SavePreset(presetName, gv, description); // <-- Adjust this method to accept description

            foreach (GridViewPreset preset in GridViewPresetManager.GetAvailablePresets())
            {
                Debug.WriteLine($"Saved preset: '{preset.Name}'");
            }

            LoadGridPresets();
        }
        private void LoadGridPresets()
        {
            GridViewPresets.Clear();
            foreach (GridViewPreset preset in GridViewPresetManager.GetAvailablePresets())
            {
                GridViewPresets.Add(preset);
            }
        }
        private void GridViewPreset_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && !string.IsNullOrWhiteSpace(tb.Text))
            {
                if (EquipmentListView.View is GridView gv)
                {
                    GridViewPresetManager.LoadPreset(tb.Text, gv);
                }
            }
        }
        public void SetValuesForSelectedRows()
        {
            var selectedItems = EquipmentListView.SelectedItems.Cast<EquipmentObject>().ToList();
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select one or more rows", "Warning", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var changeValuesDialog = new ChangeMultipleValuesWindow();
            changeValuesDialog.Owner = this;

            changeValuesDialog.ApplyClicked += (field, value) =>
            {
                var selectedItems = EquipmentListView.SelectedItems.Cast<EquipmentObject>().ToList();

                foreach (var item in selectedItems)
                {
                    switch (field)
                    {
                        case "GG-LABEL": item.GgLabel = value; break;
                        case "TYPE": item.Type = value; break;
                        case "MAKE": item.Make = value; break;
                        case "MODEL": item.Model = value; break;
                        case "SERIAL NO": item.SerialNo = value; break;
                        case "SECURITY ID": item.SecurityId = value; break;
                        case "User": item.User = value; break;
                        case "Site": item.Site = value; break;
                        case "Status": item.Status = value; break;
                        case "Purchase date":
                            if (DateTime.TryParse(value, out var d1)) item.PurchaseDate = d1;
                            break;
                        case "Recieved":
                            if (DateTime.TryParse(value, out var d2)) item.Received = d2;
                            break;
                        case "Short comment": item.ShortComment = value; break;
                    }

                    if (!_modifiedRecords.ContainsKey(item))
                        _modifiedRecords[item] = new Dictionary<string, object>();

                    _modifiedRecords[item][field] = value;
                }
            };
            changeValuesDialog.Show();
        }
        public void SaveDetailsPanel_Click(object sender, RoutedEventArgs e)
        {
            SaveChangesToCsv(_workingDocPath);
        }
        private void EquipmentListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EquipmentListView.SelectedItem != null)
            {
                if (DetailsPanelGrid.Visibility != Visibility.Collapsed)
                {
                    currentGridHeight = PropertiesRow.Height.Value;
                    _lastGridHeight = PropertiesRow.Height.Value;
                }

                DetailsPanelGrid.Visibility = Visibility.Visible;

                if (_lastGridHeight >= 50.0)
                {
                    PropertiesRow.Height = new GridLength(_lastGridHeight);
                }
                else
                {
                    PropertiesRow.Height = GridLength.Auto;
                }
            }
        }
        private void CloseDetailsPanel_Click(object sender, RoutedEventArgs e)
        {
            currentGridHeight = PropertiesRow.Height.Value;

            if (currentGridHeight >= 50.0)
            {
                _lastGridHeight = currentGridHeight;
            }
            DetailsPanelGrid.Visibility = Visibility.Collapsed;
            PropertiesRow.Height = new GridLength(0);
        }
        private void TitleBar_Loaded(object sender, RoutedEventArgs e) // Executes when the titlebar loads (must be here for it to work apparantly)
        {

        }
        private void SearchBar_Loaded(object sender, RoutedEventArgs e)
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
                EquipmentList = equipmentList;
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
                EquipmentListView.ItemsSource = EquipmentList;
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
            LogChanges.SaveLog();

            // refreshes UI List
            if (EquipmentListView.SelectedItem is EquipmentObject selected)
            {
                var logs = LogChanges.LoadLogsForItem(selected.GgLabel);
                ChangeHistoryListBox.ItemsSource = null; // force WPF to refresh
                ChangeHistoryListBox.ItemsSource = logs;
            }


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

                //MessageBox.Show("Changes saved successfully! with the new method", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private void MenuBar_Loaded(object sender, RoutedEventArgs e)
        {
            // Optional logic on load
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
        public void IntializeData()
        {
            String[] filePaths = { _workingDocPath, _fucDocPath };

            MenuBar menuBarInstance = MenuBarControl;

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

            // make list to track changes for logging
            foreach (var item in _modifiedRecords.Keys)
            {
                item.PropertyChanged -= EquipmentObject_PropertyChanged; // clear list 
            }
            _modifiedRecords.Clear();
            foreach (var item in EquipmentList)
            {
                SubscribeToEquipment(item); // create list
            }

            InitializeFooter();
        }
        public void InitializeFooter()
        {
            Footer_Control footerControlInstance = this.FindName("FooterControl") as Footer_Control;
            if (footerControlInstance != null)
            {
                footerControlInstance.WorkingDocPath = _workingDocPath;
                footerControlInstance.FucDocPath = _fucDocPath;
            }
        }
        private void ColumnHeader_RightClick(object sender, ContextMenuEventArgs e)
        {

        }
        private void HideColumn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                if (menuItem.Parent is ContextMenu contextMenu)
                {
                    if (contextMenu.PlacementTarget is GridViewColumnHeader header)
                    {
                        GridViewColumn column = header.Column;
                        if (column != null)
                        {
                            column.Width = 0; // Hide the column
                        }
                    }
                }
            }
        }
        private void ResetColumns_Click(object sender, RoutedEventArgs e)
        {
            foreach (var column in ((GridView)EquipmentListView.View).Columns)
            {
                column.Width = 100; // Reset to default width
            }
        }
        private void HiddenColumn_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock textBlock)
            {
                string columnName = textBlock.Text; // Get clicked column name

                foreach (GridViewColumn column in (EquipmentListView.View as GridView).Columns)
                {
                    string headerText = (column.Header is GridViewColumnHeader header)
                        ? header.Content.ToString()
                        : column.Header.ToString();

                    if (headerText == columnName)
                    {
                        column.Width = 100; // Restore width
                        Debug.WriteLine($"Restored column '{columnName}' to width 100");

                        // Remove from HiddenColumns list (if using ObservableCollection)
                        HiddenColumns.Remove(columnName);
                        break;
                    }
                }
            }
        }
        public void AddNewEquipment()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                Quote = '"',
                Escape = '"',
                PrepareHeaderForMatch = args => args.Header.ToLower(),
                MissingFieldFound = null,
                HeaderValidated = null
            };

            try
            {
                List<EquipmentObject> equipmentList;

                // get current data
                using (var reader = new StreamReader(_workingDocPath))
                using (var csvReader = new CsvReader(reader, config))
                {
                    csvReader.Context.RegisterClassMap<UnifiedModelMap>();
                    equipmentList = csvReader.GetRecords<EquipmentObject>().ToList();
                }

                // add next row number
                int maxGgLabel = equipmentList.Any()
                    ? equipmentList.Max(e => int.TryParse(e.GgLabel, out int val) ? val : 0)
                    : 0;

                var newItem = new EquipmentObject
                {
                    GgLabel = (maxGgLabel + 1).ToString()
                };

                equipmentList.Add(newItem);

                // Save the file with the new row
                using (var writer = new StreamWriter(_workingDocPath))
                using (var csvWriter = new CsvWriter(writer, config))
                {
                    csvWriter.Context.RegisterClassMap<UnifiedModelMap>();
                    csvWriter.WriteRecords(equipmentList);
                }

                IntializeData(); //refresh

                var addedItem = EquipmentList.FirstOrDefault(e => e.GgLabel == (maxGgLabel + 1).ToString());
                if (addedItem != null)
                {
                    EquipmentListView.SelectedItem = addedItem;
                    EquipmentListView.ScrollIntoView(addedItem);

                    // Manually invoke double-click logic
                    EquipmentListView_MouseDoubleClick(
                        EquipmentListView,
                        new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                        {
                            RoutedEvent = Control.MouseDoubleClickEvent
                        }
                    );
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating equipment list: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}