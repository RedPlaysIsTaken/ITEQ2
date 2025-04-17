using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace ITEQ2.View.Windows
{
    /// <summary>
    /// Interaction logic for ChangeMultipleValuesWindow.xaml
    /// </summary>
    public partial class ChangeMultipleValuesWindow : Window
    {

        public event Action<string, string> ApplyClicked;
        public string SelectedField => FieldComboBox.SelectedItem as string;
        public string NewValue => ValueTextBox.Text;

        public ChangeMultipleValuesWindow()
        {
            InitializeComponent();

            FieldComboBox.ItemsSource = new List<string>
        {
            "GG-LABEL","TYPE","MAKE","MODEL","SERIAL NO","SECURITY ID",
            "User","Site","Status","Purchase date","Recieved","Short comment"
        };
            FieldComboBox.SelectedIndex = 0;
        }
        private void BtnApplyChanges_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ValueTextBox.Text))
            {
                MessageBox.Show("Enter a value.");
                return;
            }

            ApplyClicked?.Invoke(FieldComboBox.SelectedItem as string, ValueTextBox.Text);
        }
    }
}
