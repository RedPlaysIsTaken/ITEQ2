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
using System.Collections.ObjectModel;
using System.Collections;


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
        private CancellationTokenSource _searchCts;

        private double LastZoomValue = 1.0;
        private int focusedElement;

        public event Action<double> ZoomChanged;

        public SearchBar()
        {
            InitializeComponent();

            ZoomSlider.ValueChanged += (s, e) => ZoomChanged?.Invoke(e.NewValue);
        }

        private async void txtBoxSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchCts?.Cancel(); // cancel previous search
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(500, token); // shorter delay

                if (token.IsCancellationRequested)
                    return;

                string searchText = txtBoxSearchBar.Text.Trim();
                Debug.WriteLine($"User typed: '{searchText}'");

                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    var result = AdvancedSearch.Search(mainWindow.EquipmentList, searchText);
                    mainWindow.SearchedEquipmentList = new ObservableCollection<EquipmentObject>(result);
                    mainWindow.EquipmentListView.ItemsSource = null;
                    mainWindow.EquipmentListView.ItemsSource = mainWindow.SearchedEquipmentList;
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Search task was canceled, ignore: Exception thrown: System.Threading.Tasks.TaskCanceledException' in System.Private.CoreLib.dll");
                // safe to ignore, user typed again quickly
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            focusedElement = mainWindow.EquipmentListView.SelectedIndex;
            if (mainWindow != null)
            {
                mainWindow.IntializeData();
                string searchText = txtBoxSearchBar.Text.Trim();
                SearchPerformed?.Invoke(searchText);
                mainWindow.EquipmentListView.SelectedIndex = focusedElement;
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
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            double zoom = e.NewValue;

            MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            if (mainWindow != null)
            {
                mainWindow.GridZoomTransform.ScaleX = zoom;
                mainWindow.GridZoomTransform.ScaleY = zoom;
            }
            
        }
        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            ZoomSlider.Value = 1.0;
        }
    }
}
