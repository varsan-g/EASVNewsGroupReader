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

namespace NewsReader
{
    public partial class PostWindow : Window
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public PostWindow()
        {
            InitializeComponent();
        }

        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            From = FromTextBox.Text;
            Subject = SubjectTextBox.Text;
            Body = BodyTextBox.Text;

            DialogResult = true;
        }
    }
}
