using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using FlexRegistry.Models;
using FlexRegistry.Utils;
using FlexRegistry.Views;
using LiteDB;
using Microsoft.Win32;

namespace FlexRegistry.ViewModels
{
    public class LogInViewModel : BaseVM
    {

        private readonly IniFileService _iniService;
        private bool _isFileDialog;
        private bool _isPasswordNeeded;
        private DatabaseItem _selectedDatabase;
        private string _password;
        public delegate void DatabaseHandler();
        public event DatabaseHandler OnCreationEnd;
        public ObservableCollection<DatabaseItem> Databases { private set; get; } = new ObservableCollection<DatabaseItem>();
        public bool IsFileDialog
        {
            get => _isFileDialog;
            set
            {
                _isFileDialog = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanUseUI));
            }
        }
        public DatabaseItem SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                _selectedDatabase = value;
                OnPropertyChanged();
                if (SelectedDatabase != null)
                    Console.WriteLine("Выбрана база " + SelectedDatabase.Path);
            }
        }
        public bool IsPasswordNeeded
        {
            get => _isPasswordNeeded;
            set
            {
                _isPasswordNeeded = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanUseUI));
                OnPropertyChanged(nameof(IsCreateAvailable));
            }
        }
        public bool IsCreateAvailable => !IsFileDialog && (!IsPasswordNeeded || !string.IsNullOrWhiteSpace(Password));
        public bool CanUseUI => !IsFileDialog;
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanUseUI));
                OnPropertyChanged(nameof(IsCreateAvailable));
            }
        }

        public ICommand RenameDatabase
        {
            get => new ClickCommand((obj) =>
            {
                var inputName = new InputWindow("Название базы");
                if (inputName.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputName.InputText))
                {
                    _iniService.Write("databases", SelectedDatabase.Path, inputName.InputText);
                    RefreshDatabaseList();
                }
            });
        }

        public ICommand DatabaseItemDoubleClick
        {
            get => new ClickCommand((obj) =>
            {
                DatabaseItem item = (DatabaseItem)obj;
                if (!File.Exists(item.Path))
                {
                    MessageBox.Show($"Файл базы данных не найден в:\n{item.Path}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    _iniService.RemoveKey("databases", item.Path);
                    RefreshDatabaseList();
                    return;
                }
                var connString = new ConnectionString
                {
                    Filename = item.Path,
                };
                LiteDatabase db = null;
                MainWindow main;
                try
                {
                    BasicSettings.Reset();
                    db = new LiteDatabase(connString);
                    main = new MainWindow(item.Name, db);
                    main.Show();
                    main.Activate();
                }
                catch (LiteDB.LiteException liteExc)
                {
                    if (liteExc.Message == "This data file is encrypted and needs a password to open")
                    {
                        InputPasswordWindow inputPassword = new InputPasswordWindow();
                        if (inputPassword.ShowDialog() == true && inputPassword.Password != null)
                        {
                            connString.Password = inputPassword.Password;
                            try
                            {
                                BasicSettings.Reset();
                                db = new LiteDatabase(connString);
                                main = new MainWindow(item.Name, db, GetHashString(connString.Password));
                                main.Show();
                                main.Activate();
                            }
                            catch (Exception e)
                            {
                                db?.Dispose();
                                MessageBox.Show(e.Message, "Ошибка");
                            }
                            connString.Password = null;
                            inputPassword.Password = null;
                        }
                    }
                    else
                    {
                        db?.Dispose();
                        MessageBox.Show(liteExc.ToString(), "Ошибка");
                    }
                }
                catch(System.IO.IOException)
                {
                    db?.Dispose();
                    MessageBox.Show("База данных уже используется", "Ошибка чтения базы данных");
                }
                catch(Exception e)
                {
                    db?.Dispose();
                    MessageBox.Show(e.ToString(), "Ошибка");
                }
            });
        }

        public ICommand AddExistingDatabase
        {
            get => new ClickCommand((obj) =>
            {
                IsFileDialog = true;

                try
                {

                    var inputName = new InputWindow("Название базы");
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "LiteDB|*.db";
                    openFileDialog.DefaultExt = "*.db";
                    if (openFileDialog.ShowDialog() == true)
                    {
                        var connString = new ConnectionString
                        {
                            Filename = openFileDialog.FileName,
                        };
                        LiteDatabase db = null;
                        try
                        {
                            using (db = new LiteDatabase(connString))
                            {
                                inputName.ShowDialog();
                                _iniService.Write("databases", openFileDialog.FileName, string.IsNullOrWhiteSpace(inputName.InputText) ? openFileDialog.SafeFileName : inputName.InputText);
                                RefreshDatabaseList();
                            }
                        }
                        catch (LiteDB.LiteException liteExc)
                        {
                            if (liteExc.Message == "This data file is encrypted and needs a password to open")
                            {
                                InputPasswordWindow inputPassword = new InputPasswordWindow();
                                if (inputPassword.ShowDialog() == true && inputPassword.Password != null)
                                {
                                    connString.Password = inputPassword.Password;
                                    try
                                    {
                                        using (db = new LiteDatabase(connString))
                                        {
                                            inputName.ShowDialog();
                                            _iniService.Write("databases", openFileDialog.FileName, string.IsNullOrWhiteSpace(inputName.InputText) ? openFileDialog.SafeFileName : inputName.InputText);
                                            RefreshDatabaseList();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        db?.Dispose();
                                        MessageBox.Show(e.ToString(), "Ошибка");
                                    }
                                    connString.Password = null;
                                    inputPassword.Password = null;
                                }
                            }
                            db?.Dispose();
                        }
                    }
                }catch(Exception e)
                {
                    MessageBox.Show(e.ToString(), "Ошибка");
                }

                IsFileDialog = false;
            });
        }

        public ICommand ChoosePlaceForDB
        {
            get => new ClickCommand((obj) =>
            {
                IsFileDialog = true;

                try
                {
                    var inputName = new InputWindow("Название базы");
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "LiteDB|*.db";
                    saveFileDialog.DefaultExt = "*.db";
                    //saveFileDialog.FileName = "registry.db";
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        var connString = new ConnectionString
                        {
                            Filename = saveFileDialog.FileName
                        };
                        if (IsPasswordNeeded)
                        {
                            connString.Password = Password;
                        }
                        using (var db = new LiteDatabase(connString))
                        {
                            // Проверка соединения (необязательно)
                            inputName.ShowDialog();
                            _iniService.Write("databases", saveFileDialog.FileName, string.IsNullOrWhiteSpace(inputName.InputText) ? saveFileDialog.SafeFileName : inputName.InputText);
                            RefreshDatabaseList();
                            // Пример работы с коллекцией
                            //var collection = db.GetCollection<YourEntity>("yourCollection");

                            // Далее ваша логика работы с базой данных...
                        }
                        connString.Password = null;
                    }
                }catch(Exception e)
                {
                    MessageBox.Show(e.ToString(), "Ошибка");
                }
                IsFileDialog = false;
                Password = null;
                IsPasswordNeeded = false;
                OnCreationEnd?.Invoke();
            });
        }

        public LogInViewModel()
        {
            _iniService = new IniFileService(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini"));
            RefreshDatabaseList();
        }

        public static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public void RefreshDatabaseList()
        {
            Databases.Clear();
            foreach (var db in _iniService.ReadSectionKeyValues("databases"))
            {
                Databases.Add(new DatabaseItem(db.Value, db.Key));
            }
        }
    }
}
