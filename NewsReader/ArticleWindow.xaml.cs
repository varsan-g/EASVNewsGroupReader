using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NewsReader
{
    public partial class ArticleWindow : Window
    {
        public ArticleWindow(string article)
        {
            InitializeComponent();
            articleTextBox.Text = new TextBlock { Text = article, TextWrapping = TextWrapping.Wrap }.Text;
        }
    }
}