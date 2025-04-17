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

namespace ITEQ2.View.UserControls
{
    /// <summary>
    /// Interaction logic for SearchBar.xaml
    /// </summary>
    public partial class SearchBar : UserControl
    {
        public event Action<string> SearchPerformed;

        public SearchBar()
        {
            InitializeComponent();
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
    }
}
