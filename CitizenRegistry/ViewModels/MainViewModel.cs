using CitizenRegistry.Models;
using CitizenRegistry.Utils;
using CitizenRegistry.ViewModels;
using CitizenRegistry.Views;
using LiteDB;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ExcelDataReader;
using System.Data;

namespace CitizenRegistry.ViewModels
{
    public class Citizen
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime BirthDate { get; set; }
    }

    public class Note
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class Document
    {
        public int Id { get; set; }
        public int CitizenId { get; set; }
        public string DocumentType { get; set; }
        public string FilePath { get; set; }
        public string DisplayName => "";
        //public ImageSource Thumbnail => null;// Генерация превью
    }


    public class MainViewModel : BaseVM
    {
        private static readonly char[] separator = { ' ' };

        private string _searchText;
        private LiteDatabase _db;
        private string _tempFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
        private string _passHash;
        private bool _isReadyToWork = false;
        private bool _isLogPanelOpen = false;
        private Person _selectedItem;
        private Person _overviewItem;
        private ObservableCollection<Person> _persons;
        private ObservableCollection<Person> _filteredPersons;
        private ObservableCollection<Record> _records; //= new ObservableCollection<Record>();
        private ObservableCollection<FileRecord> _files;
        public delegate void MainHandler();
        public event MainHandler OnExitButton;
        public string DatabaseName { get; set; }
        public bool IsLogPanelOpen
        {
            get => _isLogPanelOpen;
            set
            {
                _isLogPanelOpen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LogPanelVisibility));
            }
        }

        public ObservableCollection<FileRecord> SelectedFiles { get; private set; } = new ObservableCollection<FileRecord>();

        public Visibility LogPanelVisibility => IsLogPanelOpen ? Visibility.Visible : Visibility.Collapsed;

        public ICommand LogPanelClick
        {
            get => new ClickCommand((obj) =>
            {
                IsLogPanelOpen = !IsLogPanelOpen;
            });
        }

        public Person SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public Person OverviewItem
        {
            get => _overviewItem;
            set
            {
                _overviewItem = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsOverviewAvailable));
            }
        }

        public Visibility IsFilesVisible => Files?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        public Visibility IsRecordsVisible => Records?.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

        public Visibility IsOverviewAvailable => _overviewItem == null ? Visibility.Hidden : Visibility.Visible;

        public ObservableCollection<Record> Records
        {
            get => _records;
            set 
            {
                _records = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsRecordsVisible));
            }
        }

        public ObservableCollection<Person> Persons
        {
            get => _persons;
            private set
            {
                _persons = value;
                RefreshFilter();
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredPersons));
            }
        }

        public ObservableCollection<FileRecord> Files
        {
            get => _files;
            set
            {
                _files = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsFilesVisible));
            }
        }

        public ObservableCollection<Person> FilteredPersons
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    return _filteredPersons;
                }
                return Persons;
            }
        }
        public string SearchText 
        { 
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }
        public bool IsReadyToWork => _isReadyToWork;

        public void RefreshFilter()
        {
            if (!string.IsNullOrEmpty(SearchText))
            {
                var splitSearch = SearchText.ToLower().Split(separator, StringSplitOptions.RemoveEmptyEntries).ToArray();
                var searchEngine = new AhoCorasick(splitSearch);

                var matches = new List<(int count, Person person)>(); 
                foreach(var person in Persons)
                {
                    int counter = searchEngine.FindAll(person.ConcatenatedValues).Count;
                    if(counter > 0)
                        matches.Add((counter, person));
                }
                matches.Sort((x, y) => y.count.CompareTo(x.count));
                _filteredPersons = new ObservableCollection<Person>(matches.Select(x => x.Item2).ToArray());
            }
            OnPropertyChanged(nameof(FilteredPersons));
        }

        public ICommand ImportMassPersons
        {
            get => new ClickCommand((obj) =>
            {
                try
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "Excel Files|*.xls;*.xlsx";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        var persons = ReadExcelFile(openFileDialog.FileName);
                        var col = _db.GetCollection<Person>("persons");
                        foreach (var person in persons)
                        {
                            col.Insert(person);
                            Persons.Add(person);
                        }
                    }
                }catch(Exception e)
                {
                    MessageBox.Show(e.ToString(), "Ошибка");
                }
            });
        }

        public ICommand AddRecord
        {
            get => new ClickCommand((obj) =>
            {
                Record record = new Record(OverviewItem.Id);
                var editRecord = new EditRecordWindow();
                if(editRecord.ShowDialog() == true)
                {
                    record.Text = editRecord.Text;
                    record.CreatedAt = DateTime.Now;
                    var col = _db.GetCollection<Record>("records");
                    col.Insert(record);
                    Records.Add(record);
                    OnPropertyChanged(nameof(IsRecordsVisible));
                }
            });
        }

        public ICommand AddFile
        {
            get => new ClickCommand((obj) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Все файлы|*.*";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == true && openFileDialog.FileNames.Length > 0)
                {
                    for(int i = 0; i < openFileDialog.FileNames.Length; i++)
                    {
                        FileRecord file = new FileRecord();
                        file.Id = ObjectId.NewObjectId();
                        file.PersonId = OverviewItem.Id; file.OriginalName = openFileDialog.SafeFileNames[i];
                        _db.FileStorage.Upload(file.Id.ToString(), openFileDialog.FileNames[i]);
                        file.AddedDate = DateTime.Now;
                        var col =_db.GetCollection<FileRecord>("file_records");
                        col.Insert(file);
                        Files.Add(file);
                        OnPropertyChanged(nameof(IsFilesVisible));
                    }
                }
            });
        }

        public ICommand RemoveFiles
        {
            get => new ClickCommand((obj) =>
            {
                try
                {
                    if (MessageBox.Show("Вы уверены что хотите удалить файлы?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        var col = _db.GetCollection<FileRecord>("file_records");
                        foreach (var file in Files.ToArray())
                        {
                            if (file.IsSelected)
                            {
                                col.Delete(file.Id);
                                _db.FileStorage.Delete(file.Id.ToString());
                                Files.Remove(file);
                            }
                        }
                        OnPropertyChanged(nameof(IsFilesVisible));
                    }
                }catch(Exception e)
                {
                    MessageBox.Show(e.ToString(), "Ошибка");
                }
            });
        }

        public ICommand ExportFiles
        {
            get => new ClickCommand((obj) =>
            {
                try
                {
                    var dialog = new SaveFileDialog();
                    dialog.Title = "Select a Directory"; // instead of default "Save As"
                    dialog.Filter = "Directory|*.this.directory"; // Prevents displaying files
                    dialog.FileName = "select"; // Filename will then be "select.this.directory"
                    if (dialog.ShowDialog() == true)
                    {
                        string path = dialog.FileName;
                        // Remove fake filename from resulting path
                        path = path.Replace("\\select.this.directory", "");
                        path = path.Replace(".this.directory", "");
                        // If user has changed the filename, create the new directory
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var col = _db.GetCollection<FileRecord>("file_records");
                        foreach (var file in Files.ToArray())
                        {
                            if (file.IsSelected)
                            {
                                var filepath = Path.Combine(path, file.OriginalName);
                                var fileInfo = _db.FileStorage.Download(file.Id.ToString(), filepath, true);
                                File.SetAttributes(filepath, FileAttributes.Normal);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString(), "Ошибка");
                }
            });
        }

        public ICommand OpenLoadedFile
        {
            get => new ClickCommand((obj) =>
            {
                Mouse.OverrideCursor = Cursors.Wait; //Кружок ожидания
                try
                {
                    FileRecord file = (FileRecord)obj;
                    if (!Directory.Exists(_tempFolderPath))
                    {
                        Directory.CreateDirectory(_tempFolderPath);
                    }

                    File.SetAttributes(_tempFolderPath, File.GetAttributes(_tempFolderPath) | FileAttributes.Hidden);
                    var filepath = Path.Combine(_tempFolderPath, file.OriginalName);
                    var fileInfo = _db.FileStorage.Download(file.Id.ToString(), filepath, true);
                    File.SetAttributes(filepath, FileAttributes.Normal);
                    //File.SetAttributes(filepath, File.GetAttributes(filepath) | FileAttributes.Hidden);
                    var process = Process.Start(new ProcessStartInfo
                    {
                        FileName = filepath,
                        UseShellExecute = true  // Важно! Использует стандартные программы Windows
                    });
                }catch(Exception e)
                {
                    MessageBox.Show(e.ToString(), "Ошибка");
                }
                Mouse.OverrideCursor = null; //Возвращаем стандартное поведение курсора
            });
        }

        public ICommand EditRecord
        {
            get => new ClickCommand((obj) =>
            {
                Record record = (Record)obj;
                var editRecord = new EditRecordWindow(record);
                if (editRecord.ShowDialog() == true)
                {
                    record.Text = editRecord.Text;
                    record.ChangedAt = DateTime.Now;
                    var col = _db.GetCollection<Record>("records");
                    col.Update(record);
                    Records = new ObservableCollection<Record>(Records);
                    OnPropertyChanged(nameof(Records));
                }
            });
        }

        public ICommand CloseOverview
        {
            get => new ClickCommand((obj) =>
            {
                OverviewItem = null;
                Records.Clear();
                ClearTempFolder();
            });
        }

        public ICommand AddPerson
        {
            get => new ClickCommand((obj) =>
            {
                Person person = new Person();
                person.BasicFields = BasicSettings.Instance().BasicFields.Select(x => new FieldData(x.Name)).ToArray();
                EditPersonWindow editPerson = new EditPersonWindow(person);
                if(editPerson.ShowDialog() == true)
                {
                    person.Refresh();
                    var col = _db.GetCollection<Person>("persons");
                    col.Insert(person);
                    Persons.Add(person);
                }
            });
        }

        public ICommand RemoveButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                if (SelectedItem == null) return;
                //TODO вызов предупреждения уверены ли они в удалении
                if(MessageBox.Show("Вы уверены что хотите полностью удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var col = _db.GetCollection<Person>("persons");
                    col.Delete(SelectedItem.Id);
                    Persons.Remove(SelectedItem);
                }
            });
        }

        public ICommand PersonDoubleClick
        {
            get => new ClickCommand((obj) =>
            {
                ClearTempFolder();
                var person = (Person)obj;
                //TODO Подгрузка файлов и записей


                var fileCol = _db.GetCollection<FileRecord>("file_records");
                var recCol = _db.GetCollection<Record>("records");
                if(fileCol != null)
                {
                    Files = new ObservableCollection<FileRecord>(fileCol.Find((fileRecord) => fileRecord.PersonId == person.Id));
                }
                if(recCol != null)
                {
                    Records = new ObservableCollection<Record>(recCol.Find((record) => record.PersonId == person.Id));
                }
                OverviewItem = person;
            });
        }

        public ICommand EditPerson
        {
            get => new ClickCommand((obj) =>
            {
                var person = (Person)obj;
                var editWindow = new Views.EditPersonWindow(person);
                if (editWindow.ShowDialog() == true)
                {
                    //TODO Присвоение новых BasicFields и AdditionalFields
                    person.Refresh();
                    try
                    {
                        var col = _db.GetCollection<Person>("persons");
                        col.Update(person);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show(exc.ToString(), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    };
                    Persons = new ObservableCollection<Person>(Persons);
                    OnPropertyChanged(nameof(Persons));
                    OnPropertyChanged(nameof(FilteredPersons));
                }
            });
        }

        public ICommand RemoveRecord
        {
            get => new ClickCommand((obj) =>
            {
                var record = (Record)obj;
                if (MessageBox.Show("Вы уверены что хотите удалить запись?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var col = _db.GetCollection<Record>("records");
                    col.Delete(record.Id);
                    Records.Remove(record);
                    OnPropertyChanged(nameof(IsRecordsVisible));
                }
            });
        }

        public ICommand ExitButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                _db.Dispose();
                OnExitButton?.Invoke();
            });
        }

        public ICommand HelpButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                MessageBox.Show("Для успешного старта использования программы необходимо определить в Настройках базовые поля для сущностей, которые вы хотите вести в этой базе", "Помощь", MessageBoxButton.OK, MessageBoxImage.Question);
            });
        }

        public ICommand SettingsButtonClick
        {
            get => new ClickCommand((obj) =>
            {
                SettingsWindow settings = new SettingsWindow();
                if(settings.ShowDialog() == true)
                {
                    var col = _db.GetCollection<BasicSettings>("settings");
                    var doc = col.FindAll().FirstOrDefault();
                    var newDoc = BasicSettings.Instance();
                    if (doc != null)
                        col.Update(newDoc);
                    else
                        col.Insert(newDoc); //Возвращает Id добавленного документа
                    Mouse.OverrideCursor = Cursors.Wait;
                    RefreshPersonData();
                    Persons = new ObservableCollection<Person>(Persons);
                    OnPropertyChanged(nameof(Persons));
                    OnPropertyChanged(nameof(FilteredPersons));
                    Mouse.OverrideCursor = null;
                }
            });
        }

        public void SetDatabase(LiteDatabase db, string hash)
        {
            _db = db;
            _passHash = hash;
            //TODO: Сделать логгирование входа
            var settings = _db.GetCollection<BasicSettings>("settings").FindAll().FirstOrDefault();
            if (settings != null) 
            {
                //Обдумать как сделать обратное присвоение настроек
                BasicSettings.Instance().BasicFields = settings.BasicFields;
            }
            Persons = new ObservableCollection<Person>(_db.GetCollection<Person>("persons").FindAll());
            foreach (var person in Persons)
                person.Refresh();
            OnPropertyChanged(nameof(Persons));
            OnPropertyChanged(nameof(FilteredPersons));
            _isReadyToWork = true;
        }

        public void RefreshPersons()
        {
            foreach(var person in Persons.ToArray())
            {
                person.Refresh();
            }
        }

        public void ClearTempFolder()
        {
            try
            {
                if (!Directory.Exists(_tempFolderPath)) return;
                foreach (var file in Directory.GetFiles(_tempFolderPath))
                {
                    File.Delete(file);
                }
            }catch(Exception e)
            {
                MessageBox.Show(e.ToString(), "Ошибка");
            }
        }

        public IEnumerable<Person> ReadExcelFile(string filePath)
        {
            // Регистрация провайдера кодировок (для .NET Core)
            //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                // Конфигурация для чтения заголовков
                var config = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true // Первая строка = названия колонок
                    }
                };

                reader.Read();

                var settings = BasicSettings.Instance();

                // Создаем маппинг: базовое поле -> индекс столбца
                var basicFieldMapping = new Dictionary<string, int>();
                var basicFieldHeaders = settings.BasicFields.Select(x => x.Name).ToList();

                for (int i = 0; i < basicFieldHeaders.Count; i++)
                    basicFieldMapping.Add(basicFieldHeaders[i], i);


                var headers = new string[reader.FieldCount];
                for (int i = 0; i < headers.Length; i++)
                {
                    headers[i] = reader.GetValue(i)?.ToString() ?? $"Column_{i}";
                }

                var persons = new List<Person>();

                // Обработка данных
                while (reader.Read())
                {
                    var personBasicFields = new List<FieldData>();
                    var personAdditionalFields = new List<FieldData>();

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var value = reader.GetValue(i)?.ToString();
                        if (basicFieldMapping.TryGetValue(headers[i], out var positionIndex))
                        {
                            personBasicFields.Add(new FieldData(headers[i], value));
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(value)) continue;
                            personAdditionalFields.Add(new FieldData(headers[i], value));
                        }
                    }

                    var person = new Person()
                    {
                        BasicFields = personBasicFields.ToList(),
                        AdditionalFields = personAdditionalFields
                    };
                    person.Refresh();
                    persons.Add(person);
                }
                return persons;
            }
        }

        public void RefreshPersonData()
        {
            var settings = BasicSettings.Instance();
            var col = _db.GetCollection<Person>("persons");
            //TODO ОПТИМИЗИРОВАТЬ КОД НА DICTIONARY string

            //var basicDict = new Dictionary<string, FieldSettings>();
            //var dict = new Dictionary<string, FieldData>();

            foreach (var person in Persons)
            {
                var basicFields = new List<FieldData>();
                var additionalFields = new List<FieldData>();

                var set = new HashSet<string>();
                var addedSet = new HashSet<string>();
                foreach (var basicField in settings.BasicFields)
                    set.Add(basicField.Name);
                foreach (var basicField in settings.BasicFields)
                {
                    bool isFound = false;
                    foreach (var field in person.BasicFields)
                    {
                        if (!set.Contains(field.Key) && !addedSet.Contains(field.Key))
                        {
                            additionalFields.Add(field.Clone());
                            addedSet.Add(field.Key);
                            continue;
                        }
                        if (basicField.Name == field.Key)
                        {
                            basicFields.Add(field.Clone());
                            isFound = true;
                            break;
                        }
                    }
                    if (isFound) continue;
                    foreach (var addField in person.AdditionalFields)
                    {
                        if (basicField.Name == addField.Key)
                        {
                            basicFields.Add(addField.Clone());
                            isFound = true;
                            break;
                        }
                    }
                    if (!isFound)
                    {
                        basicFields.Add(new FieldData(basicField.Name));
                    }

                }
                foreach (var addField in person.AdditionalFields)
                {
                    if (set.Contains(addField.Key)) continue;
                    additionalFields.Add(addField.Clone());
                }
                person.BasicFields = basicFields;
                person.AdditionalFields = additionalFields.Where(x => !(string.IsNullOrWhiteSpace(x.Key) && string.IsNullOrWhiteSpace(x.Value))).ToArray();
                col.Update(person);
                person.Refresh();
            }
        }

        ~MainViewModel()
        {
            _db?.Dispose();
        }
    }
}
