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
    }
}
