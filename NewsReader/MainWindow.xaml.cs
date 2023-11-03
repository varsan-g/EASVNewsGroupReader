using NewsReader.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace NewsReader
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string serverDetails = ServerDetailsTextBox.Text;
            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Text;
        }

        private void ViewNewsGroupButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void PostArticleButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}