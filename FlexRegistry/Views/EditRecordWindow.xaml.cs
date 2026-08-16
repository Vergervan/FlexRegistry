using FlexRegistry.Models;
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

namespace FlexRegistry.Views
{
    public partial class EditRecordWindow : Window
    {
        private Record _record;
        public string Text { get; set; }
        public EditRecordWindow(Record record = null)
        {
            InitializeComponent();
            RecordBox.Focus();
            _record = record;

            if (record != null)
            {
                RecordBox.Text = record.Text;
                CreatedDateBlock.Visibility = record.CreatedAt == DateTime.MinValue ? Visibility.Collapsed : Visibility.Visible;
                ChangedDateBlock.Visibility = record.ChangedAt == DateTime.MinValue ? Visibility.Collapsed : Visibility.Visible;
                CreatedDateBlock.Text = $"Создано: {record.CreatedAt:d}";
                ChangedDateBlock.Text = $"Изменено: {record.ChangedAt:d}";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Text = RecordBox.Text;
            DialogResult = true;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
