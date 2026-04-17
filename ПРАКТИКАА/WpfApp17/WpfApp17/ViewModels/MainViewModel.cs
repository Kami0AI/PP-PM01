using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WpfApp17.Models;
using WpfApp17.Repositories;
using WpfApp17.Services;

namespace WpfApp17.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IStudentRepository _repository;
        private readonly ReportService _reportService;

        // --- Поля данных ---
        private ObservableCollection<Student> _students = new ObservableCollection<Student>();
        private Student _selectedStudent;
        private string _filterYear = "Все";
        private string _statusMessage = "Готов к работе";
        private int _recordsCount;

        private string _newStudentFIO = string.Empty;
        private string _newStudentSpecialty = string.Empty;
        private string _newStudentType = "Демоэкзамен";
        private int? _newStudentYear;
        private double? _newStudentPercent;

        // --- Свойства для привязки (Binding) ---
        public ObservableCollection<Student> Students
        {
            get => _students;
            set { _students = value; OnPropertyChanged(); }
        }

        public Student SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                OnPropertyChanged();
                // Обновляем состояние кнопки "Удалить"
                ((RelayCommand)DeleteSelectedCommand)?.RaiseCanExecuteChanged();
            }
        }

        public string FilterYear { get => _filterYear; set { _filterYear = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }
        public int RecordsCount { get => _recordsCount; set { _recordsCount = value; OnPropertyChanged(); } }

        public string NewStudentFIO { get => _newStudentFIO; set { _newStudentFIO = value; OnPropertyChanged(); } }
        public string NewStudentSpecialty { get => _newStudentSpecialty; set { _newStudentSpecialty = value; OnPropertyChanged(); } }
        public string NewStudentType { get => _newStudentType; set { _newStudentType = value; OnPropertyChanged(); } }
        public int? NewStudentYear { get => _newStudentYear; set { _newStudentYear = value; OnPropertyChanged(); } }
        public double? NewStudentPercent { get => _newStudentPercent; set { _newStudentPercent = value; OnPropertyChanged(); } }

        // --- Команды (ICommand) ---
        public ICommand AddStudentCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand DeleteSelectedCommand { get; }
        public ICommand ApplyFilterCommand { get; }

        public MainViewModel(IStudentRepository repository)
        {
            _repository = repository;
            _reportService = new ReportService();
            Students = new ObservableCollection<Student>();

            AddStudentCommand = new RelayCommand(AddStudent);
            ImportCommand = new RelayCommand(Import);
            ExportReportCommand = new RelayCommand(ExportReport);
            DeleteSelectedCommand = new RelayCommand(DeleteSelected, () => SelectedStudent != null);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);

            LoadStudents();
        }

        // ================= ЛОГИКА КОМАНД =================

        private void AddStudent()
        {
            // 1. Собираем список незаполненных обязательных полей
            var missingFields = new List<string>();
            if (string.IsNullOrWhiteSpace(NewStudentFIO)) missingFields.Add("ФИО");
            if (string.IsNullOrWhiteSpace(NewStudentSpecialty)) missingFields.Add("Специальность");
            if (!NewStudentPercent.HasValue) missingFields.Add("Балл (%)");

            // 2. Если есть пропуски → показываем ошибку и выходим
            if (missingFields.Count > 0)
            {
                string errorMsg = "⚠️ Пожалуйста, заполните следующие поля:\n• " + string.Join("\n• ", missingFields);
                StatusMessage = "Ошибка: не все данные введены";
                MessageBox.Show(errorMsg, "Ошибка ввода данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Проверка диапазона баллов
            if (NewStudentPercent < 0 || NewStudentPercent > 100)
            {
                StatusMessage = "⚠️ Ошибка: балл должен быть от 0 до 100";
                MessageBox.Show("Балл должен быть в диапазоне от 0 до 100!", "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 4. Создание и сохранение студента
            try
            {
                var student = new Student
                {
                    FIO = NewStudentFIO.Trim(),
                    Specialty = NewStudentSpecialty.Trim(),
                    Type = NewStudentType,
                    Year = NewStudentYear ?? DateTime.Now.Year,
                    Date = DateTime.Now,
                    Percent = NewStudentPercent.Value
                };

                _repository.Add(student);
                LoadStudents();
                ClearForm();
                StatusMessage = "✅ Студент успешно добавлен";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка сохранения: {ex.Message}";
                MessageBox.Show($"Не удалось добавить запись:\n{ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Import()
        {
            var dlg = new OpenFileDialog { Filter = "CSV файлы|*.csv" };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var imported = _reportService.ImportFromCsv(dlg.FileName);
                int addedCount = 0;
                foreach (var s in imported)
                {
                    try { _repository.Add(s); addedCount++; }
                    catch { /* Пропускаем дубликаты */ }
                }
                LoadStudents();
                StatusMessage = $"✅ Импорт завершён. Добавлено записей: {addedCount}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка импорта: {ex.Message}";
            }
        }

        private void ExportReport()
        {
            var dlg = new SaveFileDialog { Filter = "Excel (*.xlsx)|*.xlsx|CSV (*.csv)|*.csv" };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var data = GetFilteredStudents();
                bool isExcel = dlg.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase);
                _reportService.Export(data, dlg.FileName, isExcel);
                StatusMessage = "✅ Отчёт успешно сохранён";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка экспорта: {ex.Message}";
            }
        }

        private void DeleteSelected()
        {
            if (SelectedStudent == null) return;
            if (MessageBox.Show($"Удалить запись: {SelectedStudent.FIO}?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            try
            {
                _repository.Delete(SelectedStudent);
                LoadStudents();
                StatusMessage = "🗑️ Запись удалена";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Ошибка удаления: {ex.Message}";
            }
        }

        private void LoadStudents()
        {
            var allData = _repository.GetAll();
            ApplyFilterToCollection(allData);
        }

        private void ApplyFilter() => RefreshList();

        private void RefreshList()
        {
            var allData = _repository.GetAll();
            ApplyFilterToCollection(allData);
        }

        private void ApplyFilterToCollection(List<Student> source)
        {
            Students.Clear();
            int targetYear = 0; // Явная инициализация убирает ошибку CS0165
            bool filterByYear = FilterYear != "Все" && int.TryParse(FilterYear, out targetYear);

            foreach (var s in source)
            {
                // Если фильтр выключен (filterByYear == false) ИЛИ год совпадает
                if (!filterByYear || s.Year == targetYear)
                {
                    Students.Add(s);
                }
            }
            RecordsCount = Students.Count;
        }

        private List<Student> GetFilteredStudents()
        {
            var q = _repository.GetAll();
            if (FilterYear != "Все" && int.TryParse(FilterYear, out int y))
                q = q.Where(s => s.Year == y).ToList();
            return q;
        }

        private void ClearForm()
        {
            NewStudentFIO = string.Empty;
            NewStudentSpecialty = string.Empty;
            NewStudentPercent = null;
            NewStudentYear = null;
            NewStudentType = "Демоэкзамен";
        }

        // ================= INotifyPropertyChanged =================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ================= РЕАЛИЗАЦИЯ ICommand (КНОПКИ) =================
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object parameter) => _execute();

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}