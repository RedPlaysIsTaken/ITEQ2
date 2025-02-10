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


namespace ITEQ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<UnifiedModel> UnifiedRecords;

        public MainWindow()
        {
            InitializeComponent();
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
                GridViewColumn column = new()
                {
                    Header = property.Name,
                    DisplayMemberBinding = new Binding(property.Name),
                    Width = 150
                };
                gridView.Columns.Add(column);
            }
        }
    }
}