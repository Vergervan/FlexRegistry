using FlexRegistry.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private FieldSettings draggedItem;
        private Point startPoint;
        private ViewModels.SettingsViewModel vm;

        public SettingsWindow()
        {
            InitializeComponent();
            vm = (ViewModels.SettingsViewModel)DataContext;
            vm.OnSaveButton += () => DialogResult = true;
            vm.OnCancelButton += () => DialogResult = false;
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            vm.AddItemButtonClick.Execute(sender);
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveItemButtonClick.Execute(sender);
        }

        private void BasicFieldsListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border || e.OriginalSource is Path)
            {
                startPoint = e.GetPosition(null);
                draggedItem = (FieldSettings)GetListViewItem(e.OriginalSource as DependencyObject);
            }
        }

        private void BasicFieldsListView_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || draggedItem == null)
                return;
            if (e.OriginalSource is Border || e.OriginalSource is Path)
            {
                var currentPosition = e.GetPosition(null);
                var diff = startPoint - currentPosition;

                if (System.Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    System.Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    DragDrop.DoDragDrop(BasicFieldsListView, draggedItem, DragDropEffects.Move);
                    draggedItem = null; // Сбрасываем после перетаскивания
                }
            }
        }

        private void BasicFieldsListView_Drop(object sender, DragEventArgs e)
        {
            if (draggedItem == null) return;

            var target = GetListViewItem(e.OriginalSource as DependencyObject);

            // Если дроп на пустое место
            if (target == null)
            {
                // Проверяем что дроп внутри ListView
                Point dropPosition = e.GetPosition(BasicFieldsListView);
                if (dropPosition.Y >= 0 && dropPosition.Y <= BasicFieldsListView.ActualHeight)
                {
                    // Перемещаем в конец списка
                    int oldIndex = vm.BasicFields.IndexOf(draggedItem);
                    if (oldIndex != -1)
                    {
                        vm.BasicFields.RemoveAt(oldIndex);
                        vm.BasicFields.Add(draggedItem);
                    }
                }
            }
            // Если дроп на существующий элемент
            else if (target != draggedItem)
            {
                int oldIndex = vm.BasicFields.IndexOf(draggedItem);
                int newIndex = vm.BasicFields.IndexOf((FieldSettings)target);

                if (oldIndex == -1 || newIndex == -1) return;

                vm.BasicFields.RemoveAt(oldIndex);
                vm.BasicFields.Insert(newIndex, draggedItem);
            }
        }

        private object GetListViewItem(DependencyObject source)
        {
            while (source != null && !(source is ListViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source != null ? ((ListViewItem)source).Content : null;
        }

        private void FieldBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var originalSource = e.OriginalSource as DependencyObject;

            // Находим родительский элемент ListViewItem
            var listViewItem = GetListViewItem(originalSource);
            if(listViewItem != null)
                vm.SelectedItem = (FieldSettings)listViewItem;
        }
    }
}
