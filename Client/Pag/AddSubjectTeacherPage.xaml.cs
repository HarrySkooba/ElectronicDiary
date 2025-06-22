using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Client.DTO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Windows.Input;
using System.Net.Http.Json;
using System.ComponentModel;

namespace Client.Pag
{
    public partial class AddSubjectTeacherPage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private SubjectTeacherDTO _teacher;
        public SubjectTeacherDTO SubjectTeacher
        {
            get => _teacher;
            set { _teacher = value; Signal(nameof(SubjectTeacher)); }
        }

        private List<AllClassesAdminDTO> _classes;
        public List<AllClassesAdminDTO> Classes
        {
            get => _classes;
            set { _classes = value; Signal(nameof(Classes)); }
        }

        private List<TeacherAdminDTO> _teachers;
        public List<TeacherAdminDTO> Teachers
        {
            get => _teachers;
            set { _teachers = value; Signal(nameof(Teachers)); }
        }

        private List<SubjectAdminDTO> _subjects;
        public List<SubjectAdminDTO> Subjects
        {
            get => _subjects;
            set { _subjects = value; Signal(nameof(Subjects)); }
        }

        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Signal(nameof(Title));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddSubjectTeacherPage(SubjectTeacherDTO SubjectTeachers = null, HttpClient httpClient = null)
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());

            SubjectTeacher = SubjectTeachers ?? new SubjectTeacherDTO();
            Title = SubjectTeachers != null ? "Редактирование связи" : "Добавление связи";

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadData();
            this.DataContext = this;
        }

        private async void LoadData()
        {
            try
            {
                var teachersResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/teachers");
                teachersResponse.EnsureSuccessStatusCode();
                Teachers = JsonConvert.DeserializeObject<List<TeacherAdminDTO>>(await teachersResponse.Content.ReadAsStringAsync());

                var subjectsResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/subjects");
                subjectsResponse.EnsureSuccessStatusCode();
                Subjects = JsonConvert.DeserializeObject<List<SubjectAdminDTO>>(await subjectsResponse.Content.ReadAsStringAsync());

                var classesResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/classes");
                classesResponse.EnsureSuccessStatusCode();
                Classes = JsonConvert.DeserializeObject<List<AllClassesAdminDTO>>(await classesResponse.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async void Save()
        {
            if (SubjectTeacher.TeacherId == null || SubjectTeacher.SubjectId == null ||
               SubjectTeacher.ClassId == null || SubjectTeacher.AcademicYear == null)
            {
                MessageBox.Show("Заполните все поля");
                return;
            }

            try
            {
                HttpResponseMessage response;
                if (SubjectTeacher.Id == 0)
                {
                    var dto = new AddSubjectTeacherDTO
                    {
                        TeacherId = SubjectTeacher.TeacherId,
                        SubjectId = SubjectTeacher.SubjectId,
                        ClassId = SubjectTeacher.ClassId,
                        AcademicYear = SubjectTeacher.AcademicYear
                    };

                    response = await _httpClient.PostAsJsonAsync(
                        "https://localhost:7273/api/Admin/add-subject-teacher", dto);
                }
                else
                {
                    response = await _httpClient.PutAsJsonAsync(
                        $"https://localhost:7273/api/Admin/update-subject-teacher/{SubjectTeacher.Id}", SubjectTeacher);
                }

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Данные сохранены успешно");
                    NavigationService.GoBack();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void Cancel()
        {
            NavigationService.GoBack();
        }

        public class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;

            public event EventHandler CanExecuteChanged;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

            public void Execute(object parameter) => _execute(parameter);

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}