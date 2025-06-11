using CitizenRegistry.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CitizenRegistry
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ViewModels.MainViewModel vm;
        private LiteDB.LiteDatabase _db;
        public MainWindow(string dbName, LiteDB.LiteDatabase db, string hash = null)
        {
            InitializeComponent();

            Closing += MainWindow_Closing;

            _db = db;

            vm = (ViewModels.MainViewModel)DataContext;
            vm.DatabaseName = dbName;
            vm.OnExitButton += () => Close();
            vm.SetDatabase(db, hash);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _db?.Dispose();
            BasicSettings.Reset();
            vm.ClearTempFolder();
        }

        private void PersonList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Button) return;
            if (e.ChangedButton != MouseButton.Left) return;
            vm.PersonDoubleClick.Execute(((ListViewItem)sender).Content);
        }

        private void EditPersonButton_Click(object sender, RoutedEventArgs e)
        {
            vm.EditPerson.Execute(((Button)sender).CommandParameter);
        }

        private void RemovePersonButton_Click(object sender, RoutedEventArgs e)
        {
            vm.EditPerson.Execute(((Button)sender).CommandParameter);
        }

        private void EditRecordButton_Click(object sender, RoutedEventArgs e)
        {
            vm.EditRecord.Execute(((Button)sender).CommandParameter);
        }

        private void RemoveRecordButton_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveRecord.Execute(((Button)sender).CommandParameter);
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                vm.SearchText = null;
                vm.RefreshFilter();
            }
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                vm.SearchText = SearchBox.Text;
                vm.RefreshFilter();
            }
        }

/*        private void OpenLoadedFile_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;
            vm.OpenLoadedFile.Execute(((Button)sender).CommandParameter);
        }
*/
        private void ExportFiles_Click(object sender, RoutedEventArgs e)
        {
            vm.ExportFiles.Execute(sender);
        }

        private void RemoveFiles_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveFiles.Execute(sender);
        }
    }
}
