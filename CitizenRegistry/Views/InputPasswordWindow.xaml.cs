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

namespace CitizenRegistry.Views
{
    public partial class InputPasswordWindow : Window
    {
        public string Password { get; set; }
        public InputPasswordWindow()
        {
            InitializeComponent();
            PassBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Password = PassBox.Password;
            DialogResult = true;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void PassBox_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                Password = PassBox.Password;
                DialogResult = true;
            }
        }
    }
}
