using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace FlexRegistry.Views
{
    /// <summary>
    /// Логика взаимодействия для LogInWindow.xaml
    /// </summary>
    public partial class LogInWindow : Window
    {
        private readonly ViewModels.LogInViewModel vm;
        public LogInWindow()
        {
            InitializeComponent();
            vm = (ViewModels.LogInViewModel)DataContext;
            vm.OnCreationEnd += () => DatabasePasswordBox.Password = null;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            vm.Password = ((PasswordBox)sender).Password;
        }

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (e.OriginalSource is Button) return;
            if(e.ChangedButton == MouseButton.Left)
                vm.DatabaseItemDoubleClick.Execute(((ListViewItem)sender).Content);
        }

        private void DatabaseItemRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var item = (Models.DatabaseItem)button.CommandParameter;
            vm.Databases.Remove(item);
            var ini = new Utils.IniFileService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
            ini.RemoveKey("databases", item.Path);
        }
    }
}
