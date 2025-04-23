using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            GetSettingsUser();
        }
        private void GetSettingsUser()
        {
            ValueTextBox.Text = Properties.Settings.Default.User;
        }
        private void SaveSettingsUser()
        {
            string input = ValueTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                MessageBox.Show("Username cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Properties.Settings.Default.User = input;
            Properties.Settings.Default.Save();
            this.DialogResult = true; // indicates successful close
        }

        private void BtnApplyNameChange_Click(object sender, RoutedEventArgs e)
        {
            SaveSettingsUser();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (this.DialogResult != true && string.IsNullOrWhiteSpace(Properties.Settings.Default.User))
            {
                var result = MessageBox.Show(
                    "A username is required to use the application.\n\nExit program?",
                    "Add your username",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
