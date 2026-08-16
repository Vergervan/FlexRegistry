using FlexRegistry.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace FlexRegistry.ViewModels
{
    public class SettingsViewModel : BaseVM
    {
        private FieldSettings _selectedItem;

        public delegate void SettingsHandler();
        public event SettingsHandler OnSaveButton;
        public event SettingsHandler OnCancelButton;
        
        public FieldSettings SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FieldSettings> BasicFields { get; private set; } = new ObservableCollection<FieldSettings>(BasicSettings.Instance().BasicFields);
        public ICommand SaveButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                BasicSettings.Instance().BasicFields = BasicFields.Where(x => !string.IsNullOrWhiteSpace(x.Name)).ToList();
                OnSaveButton?.Invoke();
            });
        }

        public ICommand CancelButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                OnCancelButton?.Invoke();
            });
        }

        public ICommand AddItemButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                var field = new FieldSettings(string.Empty);
                BasicFields.Add(field);
                SelectedItem = field;
            });
        }

        public ICommand RemoveItemButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                if (SelectedItem != null)
                {
                    BasicFields.Remove(SelectedItem);
                }
            });
        }
    }
}
