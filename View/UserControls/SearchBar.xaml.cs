using ITEQ2.CsvHandling;
using ITEQ2.Presets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;


namespace ITEQ2.View.UserControls
{
    /// <summary>
    /// Interaction logic for SearchBar.xaml
    /// </summary>
    public partial class SearchBar : UserControl
    {

        public event Action<string> SearchPerformed;
        public event Action ZoomResetRequested;
        public event Action<double> ZoomChangedByWheel;

        private double LastZoomValue = 1.0;
       

        public SearchBar()
        {
            InitializeComponent();

            ZoomComboBox.SelectionChanged += ZoomComboBox_SelectionChanged;
        }

        private async void txtBoxSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            //Debug.WriteLine("wow du skrev: " + txtBoxSearchBar.Text);

            await Task.Delay(700);

            string searchText = txtBoxSearchBar.Text.Trim();

            SearchPerformed?.Invoke(searchText);
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.IntializeData(); 
            }
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.AddNewEquipment();
            }
        }

        private void BtnCreateGridPreset_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.SaveGridPreset();
            }
        }
        private void BtnSetValuesForSelectedRows_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.SetValuesForSelectedRows();
            }
        }

        // zoom logic

       
    

       
        
        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            double resetZoom = 1.0;
            ZoomChangedByWheel?.Invoke(resetZoom);
            LastZoomValue = resetZoom;
            UpdateZoomComboBox(resetZoom);
            Debug.WriteLine("Zoom reset to 100%");

        }
        private void IncZoom_Click(object sender, RoutedEventArgs e)
        {

            double newZoom = Math.Clamp(LastZoomValue - 0.05, 0.25, 2.0);
            ZoomChangedByWheel?.Invoke(newZoom);
            LastZoomValue = newZoom;
            //  Debug.WriteLine($"Zoom decreased to {newZoom}");
            UpdateZoomComboBox(newZoom);

        }

        private void RedZoom_Click(object sender, RoutedEventArgs e)
        {
            double newZoom = Math.Clamp(LastZoomValue + 0.05, 0.25, 2.0);
            ZoomChangedByWheel?.Invoke(newZoom);
            LastZoomValue = newZoom;
            //   Debug.WriteLine($"Zoom increased to {newZoom}");
            UpdateZoomComboBox(newZoom);
        }
        private void EquipmentListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double delta = e.Delta > 0 ? 0.1 : -0.1;
                ZoomChangedByWheel?.Invoke(delta); // Send zoom delta to MainWindow
                e.Handled = true;
            }
        }

        private void ZoomComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Debug.WriteLine("ZoomComboBox_SelectionChanged called");

            if (ZoomComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Debug.WriteLine($"Selected Tag: {selectedItem.Tag}");

                // Try using `Convert.ToDouble` instead (it can handle boxed doubles)
                try
                {
                    double zoom = double.Parse(selectedItem.Tag.ToString(), CultureInfo.InvariantCulture);

                    ZoomChangedByWheel?.Invoke(zoom);
                    LastZoomValue = zoom;
                   // Debug.WriteLine($"Zoom changed to {zoom}");
                }
                catch (Exception ex)
                {
                 //   Debug.WriteLine($"Failed to convert Tag to double: {ex.Message}");
                }
            }
        }
    private void UpdateZoomComboBox(double zoom)
{
    // Try to find a matching predefined item
    foreach (ComboBoxItem item in ZoomComboBox.Items)
    {
        if (double.TryParse(item.Tag.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double tagValue))
        {
            if (Math.Abs(tagValue - zoom) < 0.01)
            {
                ZoomComboBox.SelectedItem = item;
                return;
            }
        }
    }

    // No match found — show a custom label
    string customLabel = $" {Math.Round(zoom * 100)}%";

    // Check if there's already a custom item
    ComboBoxItem customItem = ZoomComboBox.Items
        .OfType<ComboBoxItem>()
        .FirstOrDefault(i => i.Tag?.ToString() == "custom");

    if (customItem == null)
    {
        customItem = new ComboBoxItem { Tag = "custom" };
        ZoomComboBox.Items.Add(customItem);
    }

    customItem.Content = customLabel;
    ZoomComboBox.SelectedItem = customItem;
}


    }
}
