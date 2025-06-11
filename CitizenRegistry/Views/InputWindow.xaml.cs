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
    public partial class InputWindow : Window
    {
        public string InputText { get; set; }
        public InputWindow(string title)
        {
            InitializeComponent();
            Title = title;
            InputBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            InputText = InputBox.Text;
            DialogResult = true;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void InputBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InputText = InputBox.Text;
                DialogResult = true;
            }
        }
    }
}
