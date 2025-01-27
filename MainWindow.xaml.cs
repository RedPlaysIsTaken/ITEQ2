using System.Windows;
using ITEQ2.View;


namespace ITEQ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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
                txtInput.Text = modalWindow.Input;
            }
        }
    }
}