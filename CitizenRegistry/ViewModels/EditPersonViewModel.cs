using CitizenRegistry.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CitizenRegistry.ViewModels
{
    public class EditPersonViewModel : BaseVM
    {
        private Person _person;
        private FieldData _additionalSelectedItem;
        public delegate void EditHandler();
        public event EditHandler OnSaveButton;
        public event EditHandler OnCancelButton;

        public ObservableCollection<FieldData> BasicFields { get; private set; } = new ObservableCollection<FieldData>();
        public ObservableCollection<FieldData> AdditionalFields { get; private set; } = new ObservableCollection<FieldData>();
        public FieldData AdditionalSelectedItem
        {
            get => _additionalSelectedItem;
            set
            {
                _additionalSelectedItem = value;
                OnPropertyChanged();
            }
        }
        public void SetPerson(Person person)
        {
            _person = person;
            var settings = BasicSettings.Instance();

            //TODO ОПТИМИЗИРОВАТЬ КОД НА DICTIONARY string

            //var basicDict = new Dictionary<string, FieldSettings>();
            //var dict = new Dictionary<string, FieldData>();

            var set = new HashSet<string>();
            var addedSet = new HashSet<string>();
            foreach (var basicField in settings.BasicFields)
                set.Add(basicField.Name);
            foreach(var basicField in settings.BasicFields)
            {
                bool isFound = false;
                foreach (var field in person.BasicFields)
                {
                    if (!set.Contains(field.Key) && !addedSet.Contains(field.Key))
                    {
                        AdditionalFields.Add(field.Clone());
                        addedSet.Add(field.Key);
                        continue;
                    }
                    if(basicField.Name == field.Key)
                    {
                        BasicFields.Add(field.Clone());
                        isFound = true;
                        break;
                    }
                }
                if (isFound) continue;
                foreach(var addField in person.AdditionalFields)
                {
                    if(basicField.Name == addField.Key)
                    {
                        BasicFields.Add(addField.Clone());
                        isFound = true;
                        break;
                    }
                }
                if (!isFound)
                {
                    BasicFields.Add(new FieldData(basicField.Name));
                }

            }
            foreach(var addField in person.AdditionalFields)
            {
                if (set.Contains(addField.Key)) continue;
                AdditionalFields.Add(addField.Clone());
            }
        }

        public ICommand AddItemButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                var field = new FieldData(string.Empty);
                AdditionalFields.Add(field);
                AdditionalSelectedItem = field;
            });
        }

        public ICommand RemoveItemButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                if (AdditionalSelectedItem != null)
                {
                    AdditionalFields.Remove(AdditionalSelectedItem);
                }
            });
        }

        public ICommand SaveButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                bool ok = true;
                bool hasKey = false, hasValue = false;
                var required = BasicSettings.Instance().RequiredFields;

                foreach(var field in BasicFields)
                {
                    if(required.Contains(field.Key) && string.IsNullOrWhiteSpace(field.Value))
                    {
                        MessageBox.Show($"Поле '{field.Key}' обязательно к заполнению", "Ошибка");
                        ok = false;
                        break;
                    }
                }
                foreach(var field in AdditionalFields)
                {
                    hasKey = !string.IsNullOrWhiteSpace(field.Key);
                    hasValue = !string.IsNullOrWhiteSpace(field.Value);
                    if(hasKey && !hasValue)
                    {
                        MessageBox.Show($"Укажите значение к полю '{field.Key}', либо удалите его", "Ошибка");
                        ok = false;
                        break;
                    }
                    if (hasValue && !hasKey)
                    {
                        MessageBox.Show($"Укажите название поля к значению '{field.Value}', либо удалите его", "Ошибка");
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    _person.BasicFields = BasicFields;
                    _person.AdditionalFields = AdditionalFields.Where(x => !(string.IsNullOrWhiteSpace(x.Key) && string.IsNullOrWhiteSpace(x.Value))).ToArray();
                    OnSaveButton?.Invoke();
                }
            });
        }
        public ICommand CancelButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                OnCancelButton?.Invoke();
            });
        }
    }

}
