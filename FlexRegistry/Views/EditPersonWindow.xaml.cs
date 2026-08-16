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
using Person = FlexRegistry.Models.Person;

namespace FlexRegistry.Views
{
    public partial class EditPersonWindow : Window
    {
        private FieldData draggedItem;
        private Point startPoint;
        private ViewModels.EditPersonViewModel vm;
        public EditPersonWindow(Person person)
        {
            InitializeComponent();
            vm = (ViewModels.EditPersonViewModel)DataContext;
            vm.OnSaveButton += () => DialogResult = true;
            vm.OnCancelButton += () => DialogResult = false;
            vm.SetPerson(person);
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            vm.AddItemButtonClick.Execute(sender);
        }

        private void RemoveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            vm.RemoveItemButtonClick.Execute(sender);
        }

        private void AdditionalDataListView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Border || e.OriginalSource is Path)
            {
                startPoint = e.GetPosition(null);
                draggedItem = (FieldData)GetListViewItem(e.OriginalSource as DependencyObject);
            }
        }

        private void AdditionalDataListView_PreviewMouseMove(object sender, MouseEventArgs e)
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
                    DragDrop.DoDragDrop(AdditionalDataListView, draggedItem, DragDropEffects.Move);
                    draggedItem = null; // Сбрасываем после перетаскивания
                }
            }
        }

        private void AdditionalDataListView_Drop(object sender, DragEventArgs e)
        {
            if (draggedItem == null) return;

            var target = GetListViewItem(e.OriginalSource as DependencyObject);

            // Если дроп на пустое место
            if (target == null)
            {
                // Проверяем что дроп внутри ListView
                Point dropPosition = e.GetPosition(AdditionalDataListView);
                if (dropPosition.Y >= 0 && dropPosition.Y <= AdditionalDataListView.ActualHeight)
                {
                    // Перемещаем в конец списка
                    int oldIndex = vm.AdditionalFields.IndexOf(draggedItem);
                    if (oldIndex != -1)
                    {
                        vm.AdditionalFields.RemoveAt(oldIndex);
                        vm.AdditionalFields.Add(draggedItem);
                    }
                }
            }
            // Если дроп на существующий элемент
            else if (target != draggedItem)
            {
                int oldIndex = vm.AdditionalFields.IndexOf(draggedItem);
                int newIndex = vm.AdditionalFields.IndexOf((FieldData)target);

                if (oldIndex == -1 || newIndex == -1) return;

                vm.AdditionalFields.RemoveAt(oldIndex);
                vm.AdditionalFields.Insert(newIndex, draggedItem);
            }
        }

        private object GetListViewItem(DependencyObject source)
        {
            while (source != null && !(source is ListViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source != null ? ((ListViewItem)source).Content : null;
        }
    }
}
