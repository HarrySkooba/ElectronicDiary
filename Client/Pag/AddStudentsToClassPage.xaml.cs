using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Channels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.DTO;
using Newtonsoft.Json;

namespace Client.Pag
{
    public partial class AddStudentsToClassPage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private List<AllClassesAdminDTO> _classes;
        public List<AllClassesAdminDTO> Classes
        {
            get => _classes;
            set { _classes = value; Signal(nameof(Classes)); }
        }

        private AllClassesAdminDTO _selectedClass;
        public AllClassesAdminDTO SelectedClass
        {
            get => _selectedClass;
            set
            {
                if (_selectedClass == value) return;
                _selectedClass = value;
                Signal(nameof(SelectedClass));
                if (value != null)
                {
                    LoadStudentsForClass();
                }
                else
                {
                    AvailableStudents = new List<StudentAdminDTO>();
                }
            }
        }

        private List<StudentAdminDTO> _availableStudents;
        public List<StudentAdminDTO> AvailableStudents
        {
            get => _availableStudents;
            set { _availableStudents = value; Signal(nameof(AvailableStudents)); }
        }

        private List<StudentAdminDTO> _selectedStudents = new List<StudentAdminDTO>();
        public List<StudentAdminDTO> SelectedStudents
        {
            get => _selectedStudents;
            set { _selectedStudents = value; Signal(nameof(SelectedStudents)); }
        }

        public ICommand AddSelectedCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public AddStudentsToClassPage()
        {
            InitializeComponent();
            this.DataContext = this;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());

            AddSelectedCommand = new RelayCommand(AddSelectedStudents);
            RemoveSelectedCommand = new RelayCommand(RemoveSelectedStudents);
            SaveCommand = new RelayCommand(async () => await SaveChanges());
            CancelCommand = new RelayCommand(Cancel);

            LoadInitialData();
            this.Loaded += (s, e) =>
            {
                StudentsListBox = FindName("AvailableStudentsListBox") as ListBox;
                SelectedStudentsListBox = FindName("SelectedStudentsListBoxes") as ListBox;
            };
        }

        private async void LoadInitialData()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7273/api/Admin/classes");
                response.EnsureSuccessStatusCode();
                var classes = await response.Content.ReadFromJsonAsync<List<AllClassesAdminDTO>>();
                Classes = classes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async void LoadStudentsForClass()
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://localhost:7273/api/Admin/students");
                response.EnsureSuccessStatusCode();
                AvailableStudents = await response.Content.ReadFromJsonAsync<List<StudentAdminDTO>>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки студентов класса: {ex.Message}");
            }
        }

        private void AddSelectedStudents()
        {
            if (StudentsListBox?.SelectedItems == null) return;

            var newSelections = StudentsListBox.SelectedItems.Cast<StudentAdminDTO>().ToList();
            SelectedStudents = SelectedStudents
                .Concat(newSelections)
                .Distinct()
                .ToList();
        }

        private void RemoveSelectedStudents()
        {
            if (SelectedStudentsListBox?.SelectedItems == null) return;

            var toRemove = SelectedStudentsListBox.SelectedItems.Cast<StudentAdminDTO>().ToList();
            SelectedStudents = SelectedStudents
                .Except(toRemove)
                .ToList();
        }

        private async Task SaveChanges()
        {
            if (SelectedClass == null || !SelectedStudents.Any())
            {
                MessageBox.Show("Выберите класс и хотя бы одного ученика");
                return;
            }

            try
            {
                var studentIds = SelectedStudents.Select(s => s.Id).ToList();
                var response = await _httpClient.PostAsJsonAsync(
                    $"https://localhost:7273/api/Admin/add-students?classId={SelectedClass.Id}",
                    studentIds);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AddStudentsResponse>();

                    var message = new StringBuilder();
                    message.AppendLine($"Успешно добавлено: {result.AddedCount} учеников");

                    if (result.InvalidPersonIds?.Any() == true)
                    {
                        message.AppendLine($"Не найдены или не являются учениками: {result.InvalidPersonIds.Count}");
                        var invalidStudents = SelectedStudents
                            .Where(s => result.InvalidPersonIds.Contains(s.Id))
                            .Select(s => s.FullName)
                            .ToList();
                        message.AppendLine(string.Join(", ", invalidStudents));
                    }

                    if (result.AlreadyInClassIds?.Any() == true)
                    {
                        message.AppendLine($"Уже были в классе: {result.AlreadyInClassIds.Count}");
                        var existingStudents = SelectedStudents
                            .Where(s => result.AlreadyInClassIds.Contains(s.Id))
                            .Select(s => s.FullName)
                            .ToList();
                        message.AppendLine(string.Join(", ", existingStudents));
                    }

                    
                    MessageBox.Show(message.ToString(), "Результат операции");
                    SelectedStudents.Clear();
                    LoadStudentsForClass();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class AddStudentsResponse
        {
            public int AddedCount { get; set; }
            public List<int> InvalidPersonIds { get; set; }
            public List<int> AlreadyInClassIds { get; set; }
            public string Message { get; set; }
        }

        private void Cancel()
        {
            NavigationService.GoBack();
        }

        public ListBox StudentsListBox { get; set; }
        public ListBox SelectedStudentsListBox { get; set; }

        public class RelayCommand : ICommand
        {
            private readonly Action _execute;
            private readonly Func<bool> _canExecute;

            public event EventHandler CanExecuteChanged;

            public RelayCommand(Action execute, Func<bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

            public void Execute(object parameter) => _execute();

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
