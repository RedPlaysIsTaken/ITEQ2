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
using System.ComponentModel;

namespace ITEQ2.View.UserControls
{
    public partial class SearchBar : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private string _searchErrorFeedback;
        public string SearchErrorFeedback
        {
            get => _searchErrorFeedback;
            set
            {
                if (_searchErrorFeedback != value)
                {
                    _searchErrorFeedback = value;
                    OnPropertyChanged(nameof(SearchErrorFeedback));
                }
            }
        }


        public event Action<string> SearchPerformed;
        private CancellationTokenSource _searchCts;

        private int focusedElement;

        public SearchBar()
        {
            InitializeComponent();
        }

        private async void txtBoxSearchBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(500, token);

                if (token.IsCancellationRequested)
                    return;

                string searchText = txtBoxSearchBar.Text.Trim();
                Debug.WriteLine($"User typed: '{searchText}'");

                MainWindow mainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                if (mainWindow != null)
                {
                    var searcher = new AdvancedSearch();
                    var result = searcher.Search(mainWindow.EquipmentList, searchText);

                    SearchErrorFeedback = searcher.SearchErrorFeedback;

                    mainWindow.SearchedEquipmentList = new ObservableCollection<EquipmentObject>(result);
                    mainWindow.EquipmentListView.ItemsSource = null;
                    mainWindow.EquipmentListView.ItemsSource = mainWindow.SearchedEquipmentList;
                }
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine("Nothing was searched for");
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
            //ZoomSlider.Value = 1.0;
        }

    }
}
