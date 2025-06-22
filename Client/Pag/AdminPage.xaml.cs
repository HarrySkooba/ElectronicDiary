using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Client.DTO;
using Newtonsoft.Json;

namespace Client.Pag
{
    public partial class AdminPage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        private bool _isInitialized = false;
        private bool _isClassSelectionHandled = false;

        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private List<UserAdminDTO> _users;
        public List<UserAdminDTO> Users
        {
            get => _users;
            set { _users = value; Signal(nameof(Users)); }
        }

        private List<PersonAdminDTO> _person;
        public List<PersonAdminDTO> Persons
        {
            get => _person;
            set { _person = value; Signal(nameof(Persons)); }
        }

        private SchoolAdminDTO _school;
        public SchoolAdminDTO School
        {
            get => _school;
            set
            {
                _school = value;
                Signal(nameof(School));
            }
        }

        private List<DirectorAdminDTO> _directors;
        public List<DirectorAdminDTO> Directors
        {
            get => _directors;
            set
            {
                _directors = value;
                Signal(nameof(Directors));
            }
        }

        private DirectorAdminDTO _selectedDirector;
        public DirectorAdminDTO SelectedDirector
        {
            get => _selectedDirector;
            set
            {
                if (_selectedDirector == value) return;

                _selectedDirector = value;
                Signal(nameof(SelectedDirector));

                if (School != null && value != null)
                {
                    School.IdDirector = value.Id;
                }
            }
        }

        private List<AllClassesAdminDTO> _classes;
        public List<AllClassesAdminDTO> Classes 
        { get => _classes; 
          set { _classes = value; Signal(nameof(Classes)); }
        }

        private ClassesAdminDTO _selectedClass;
        public ClassesAdminDTO SelectedClass 
        { 
            get => _selectedClass; 
            set { _selectedClass = value; Signal(nameof(SelectedClass)); } 
        }

        private List<TeacherAdminDTO> _teachers;
        public List<TeacherAdminDTO> Teachers 
        { 
            get => _teachers; 
            set { _teachers = value; Signal(nameof(Teachers)); } 
        }

        private TeacherAdminDTO _selectedTeacher;
        public TeacherAdminDTO SelectedTeacher
        {
            get => _selectedTeacher;
            set
            {
                if (_selectedTeacher == value) return;
                _selectedTeacher = value;
                Signal(nameof(SelectedTeacher));
                if (SelectedClass != null && value != null) SelectedClass.ClassTeacherId = value.Id;
            }
        }

        private List<StudentAdminDTO> _students;
        public List<StudentAdminDTO> Students 
        { 
            get => _students; 
            set { _students = value; Signal(nameof(Students)); } 
        }

        private StudentAdminDTO _selectedStudent;
        public StudentAdminDTO SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                _selectedStudent = value;
                Signal(nameof(SelectedStudent));
            }
        }

        private List<SubjectTeacherDTO> _subjectTeachers;
        public List<SubjectTeacherDTO> SubjectTeachers
        {
            get => _subjectTeachers;
            set
            {
                _subjectTeachers = value;
                Signal(nameof(SubjectTeachers));
                FilterSubjectTeachers();
            }
        }

        private string _userSearchText = "";
        public string UserSearchText
        {
            get => _userSearchText;
            set
            {
                _userSearchText = value;
                Signal(nameof(UserSearchText));
                FilterUsers();
            }
        }

        private string _personSearchText = "";
        public string PersonSearchText
        {
            get => _personSearchText;
            set
            {
                _personSearchText = value;
                Signal(nameof(PersonSearchText));
                FilterPersons();
            }
        }

        private string _subjectTeacherSearchText = "";
        public string SubjectTeacherSearchText
        {
            get => _subjectTeacherSearchText;
            set
            {
                _subjectTeacherSearchText = value;
                Signal(nameof(SubjectTeacherSearchText));
                FilterSubjectTeachers();
            }
        }

        private List<UserAdminDTO> _filteredUsers = new();
        public List<UserAdminDTO> FilteredUsers
        {
            get => _filteredUsers;
            set
            {
                _filteredUsers = value;
                Signal(nameof(FilteredUsers));
            }
        }

        private List<PersonAdminDTO> _filteredPersons = new();
        public List<PersonAdminDTO> FilteredPersons
        {
            get => _filteredPersons;
            set
            {
                _filteredPersons = value;
                Signal(nameof(FilteredPersons));
            }
        }

        private List<SubjectTeacherDTO> _filteredSubjectTeachers;
        public List<SubjectTeacherDTO> FilteredSubjectTeachers
        {
            get => _filteredSubjectTeachers;
            set
            {
                _filteredSubjectTeachers = value;
                Signal(nameof(FilteredSubjectTeachers));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; Signal(nameof(IsLoading)); }
        }

        public ObservableCollection<DayScheduleDTO> Days { get; } = new ObservableCollection<DayScheduleDTO>();

        private DateOnly _currentWeekStart;
        public DateOnly CurrentWeekStart
        {
            get => _currentWeekStart;
            set { _currentWeekStart = value; Signal(nameof(CurrentWeekStart)); Signal(nameof(WeekRangeText)); }
        }
        public string WeekRangeText => $"{CurrentWeekStart:dd.MM.yyyy} - {CurrentWeekStart.AddDays(6):dd.MM.yyyy}";

        private int? _selectedClassForSchedule;
        public int? SelectedClassForSchedule
        {
            get => _selectedClassForSchedule;
            set { _selectedClassForSchedule = value; Signal(nameof(SelectedClassForSchedule)); }
        }

        private string _dayName;
        public string DayName
        {
            get => _dayName;
            set { _dayName = value; Signal(nameof(DayName)); }
        }

        private DateOnly _date;
        public DateOnly Date
        {
            get => _date;
            set { _date = value; Signal(nameof(Date)); }
        }

        private List<ScheduleDTO> _lessons;
        public List<ScheduleDTO> Lessons
        {
            get => _lessons;
            set { _lessons = value; Signal(nameof(Lessons)); }
        }

        private bool _isCurrentDay;
        public bool IsCurrentDay
        {
            get => _isCurrentDay;
            set { _isCurrentDay = value; Signal(nameof(IsCurrentDay)); }
        }

        private ScheduleDTO _selectedLesson;
        public ScheduleDTO SelectedLesson
        {
            get => _selectedLesson;
            set { _selectedLesson = value; Signal(nameof(SelectedLesson)); }
        }

        public ICommand SearchUsersCommand { get; }
        public ICommand SearchPersonsCommand { get; }
        public ICommand SelectClassCommand { get; }
        public ICommand RemoveStudentCommand { get; }
        public ICommand SearchSubjectTeachersCommand { get; }


        public AdminPage()
        {
            InitializeComponent();
            this.DataContext = this;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());

            FilteredUsers = new List<UserAdminDTO>();
            FilteredPersons = new List<PersonAdminDTO>();

            SelectClassCommand = new RelayCommand<AllClassesAdminDTO>(async (selectedClass) =>
            {
                await SelectClassHandler(selectedClass);
            });

            SearchUsersCommand = new RelayCommand((_) => FilterUsers());
            SearchPersonsCommand = new RelayCommand((_) => FilterPersons());
            SearchSubjectTeachersCommand = new RelayCommand((_) => FilterSubjectTeachers());

            RemoveStudentCommand = new RelayCommand(async (student) => await RemoveStudentFromClass(student as StudentAdminDTO));
            MainTabControl.SelectionChanged += MainTabControl_SelectionChanged;
            LoadTeachersData();
            LoadClassesData();
        }

        private async Task SelectClassHandler(AllClassesAdminDTO selectedClass)
        {
            if (_isClassSelectionHandled || selectedClass == null)
                return;

            _isClassSelectionHandled = true;
            try
            {
                var loadClassTask = LoadSelectedClassData(selectedClass.Id);

                await Task.WhenAll(loadClassTask);
                SelectedTeacher = Teachers.FirstOrDefault(t => t.Id == SelectedClass.ClassTeacherId);


                await LoadStudentsData(selectedClass.Id);
            }
            finally
            {
                _isClassSelectionHandled = false;
            }
        }

        private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;
            if (MainTabControl.SelectedItem is TabItem selectedTab)
            {
                switch (selectedTab.Header.ToString())
                {
                    case "Аккаунты":
                        await LoadAccountsData();
                        break;
                    case "Персоны":
                        await LoadPersonsData();
                        break;
                    case "Школа":
                        await LoadSchoolData();
                        break;
                    case "Классы":
                        await LoadClassesData();
                        break;
                    case "Учителя":
                        await LoadSubjectTeachersData();
                        break;
                    case "Расписание":
                        CurrentWeekStart = GetCurrentWeekStart();
                        break;
                }
            }
        }

        private async Task LoadAccountsData()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7273/api/Admin/users");
                response.EnsureSuccessStatusCode();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new DateOnlyJsonConverter() }
                };

                Users = await response.Content.ReadFromJsonAsync<List<UserAdminDTO>>(options);
                FilteredUsers = Users.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private async Task LoadPersonsData()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7273/api/Admin/persons");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                Persons = JsonConvert.DeserializeObject<List<PersonAdminDTO>>(content);
                FilteredPersons = Persons.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private async Task LoadSchoolData(bool forceReload = false)
        {
            if (!forceReload && _isInitialized) return;

            try
            {
                var schoolResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/school");
                schoolResponse.EnsureSuccessStatusCode();
                School = await schoolResponse.Content.ReadFromJsonAsync<SchoolAdminDTO>();

                if (Directors == null || forceReload)
                {
                    var directorsResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/potential-directors");
                    directorsResponse.EnsureSuccessStatusCode();
                    Directors = await directorsResponse.Content.ReadFromJsonAsync<List<DirectorAdminDTO>>();
                }

                SelectedDirector = Directors?.FirstOrDefault(d => d.Id == School?.IdDirector);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private async Task LoadClassesData()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7273/api/Admin/classes");
                response.EnsureSuccessStatusCode();
                var classes = await response.Content.ReadFromJsonAsync<List<AllClassesAdminDTO>>();

                Classes = classes?
                    .OrderBy(c =>
                    {
                        try
                        {
                            var match = System.Text.RegularExpressions.Regex.Match(c.Name, @"\d+");
                            return match.Success ? int.Parse(match.Value) : int.MaxValue;
                        }
                        catch
                        {
                            return int.MaxValue;
                        }
                    })
                    .ThenBy(c => c.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки классов: {ex.Message}");
            }
        }

        private async Task LoadSubjectTeachersData()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7273/api/Admin/subject-teachers");
                response.EnsureSuccessStatusCode();
                SubjectTeachers = await response.Content.ReadFromJsonAsync<List<SubjectTeacherDTO>>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки связей учитель-предмет-класс: {ex.Message}");
            }
        }

        private async Task LoadScheduleData()
        {
            if (SelectedClassForSchedule == null)
            {
                MessageBox.Show("Выберите класс");
                return;
            }

            try
            {
                IsLoading = true;
                Days.Clear();

                var endpoint = $"api/Admin/schedule?weekStart={CurrentWeekStart:yyyy-MM-dd}&classId={SelectedClassForSchedule}";
                var fullUrl = $"https://localhost:7273/{endpoint}";

                var response = await _httpClient.GetAsync(fullUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Ошибка сервера: {response.StatusCode}\n{errorContent}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new DateOnlyJsonConverter() }
                };

                var schedule = System.Text.Json.JsonSerializer.Deserialize<Dictionary<DateOnly, List<ScheduleDTO>>>(content, options);

                if (schedule != null)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        var currentDate = CurrentWeekStart.AddDays(i);
                        var dayName = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)((i + 1) % 7));
                        dayName = char.ToUpper(dayName[0]) + dayName.Substring(1);

                        var dayLessons = schedule.TryGetValue(currentDate, out var lessons)
                            ? lessons.OrderBy(l => l.Lesson_Number).ToList()
                            : new List<ScheduleDTO>();

                        Days.Add(new DayScheduleDTO
                        {
                            DayName = dayName,
                            Date = currentDate,
                            Lessons = dayLessons,
                            IsCurrentDay = currentDate == DateOnly.FromDateTime(DateTime.Now)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки расписания: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddUser_Button_Click(object sender, RoutedEventArgs e)
        {
            var userEditPage = new AddUserPage();

            NavigationService.Navigate(userEditPage);
        }

        private void EditUser_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var user = button.DataContext as UserAdminDTO;
            if (user == null) return;

            var userEditPage = new AddUserPage(user);

            NavigationService.Navigate(userEditPage);
        }

        private void AddPerson_Button_Click(object sender, RoutedEventArgs e)
        {
            var personEditPage = new AddPersonPage();

            NavigationService.Navigate(personEditPage);
        }

        private void EditPerson_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var person = button.DataContext as PersonAdminDTO;
            if (person == null) return;

            var personEditPage = new AddPersonPage(person);

            NavigationService.Navigate(personEditPage);
        }

        private async void EditSchool_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(School.Name) || string.IsNullOrWhiteSpace(School.Phone) || string.IsNullOrWhiteSpace(School.Website) || string.IsNullOrWhiteSpace(School.Address) || SelectedDirector == null)
            {
                MessageBox.Show("Не все поля заполнены");
                return;
            }
            try
            {
                UpdateSchool updateSchool = new UpdateSchool
                {
                    Name = SchoolNameTB.Text,
                    Phone = SchoolPhoneTB.Text,
                    Website = SchoolWebsiteTB.Text,
                    Address = SchoolAdressTB.Text,
                    IdDirector = SelectedDirector.Id,
                };
                var response = await _httpClient.PutAsJsonAsync($"https://localhost:7273/api/Admin/updateschool/{School.Id}", updateSchool);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Данные обновлены!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task LoadSelectedClassData(int classId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://localhost:7273/api/Admin/selected_class/{classId}");
                response.EnsureSuccessStatusCode();
                SelectedClass = await response.Content.ReadFromJsonAsync<ClassesAdminDTO>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных класса: {ex.Message}");
            }
        }

        private async Task LoadTeachersData()
        {
            try
            {
                var response = await _httpClient.GetAsync("https://localhost:7273/api/Admin/teachers");
                response.EnsureSuccessStatusCode();
                Teachers = await response.Content.ReadFromJsonAsync<List<TeacherAdminDTO>>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки учителей: {ex.Message}");
            }
        }

        private async Task LoadStudentsData(int classId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://localhost:7273/api/Admin/students/{classId}");
                response.EnsureSuccessStatusCode();
                Students = await response.Content.ReadFromJsonAsync<List<StudentAdminDTO>>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки учеников: {ex.Message}");
            }
        }

        private async void SaveClassChanges_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClass == null)
            {
                MessageBox.Show("Класс не выбран");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedClass.Name))
            {
                MessageBox.Show("Введите название класса");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedClass.AcademicYear.ToString()))
            {
                MessageBox.Show("Введите учебный год");
                return;
            }

            if (SelectedClass.ClassTeacherId == null)
            {
                MessageBox.Show("Выберите классного руководителя");
                return;
            }

            try
            {
                var classDto = new AddClassDto
                {
                    Name = SelectedClass.Name,
                    AcademicYear = SelectedClass.AcademicYear,
                    ClassTeacherId = SelectedClass.ClassTeacherId.Value
                };

                HttpResponseMessage response;

                if (SelectedClass.Id == 0) 
                {
                    response = await _httpClient.PostAsJsonAsync("https://localhost:7273/api/Admin/addclass", classDto);
                }
                else
                {
                    response = await _httpClient.PutAsJsonAsync(
                        $"https://localhost:7273/api/Admin/updateclass/{SelectedClass.Id}",
                        classDto);
                }

                if (response.IsSuccessStatusCode)
                {
                    var message = SelectedClass.Id == 0 ? "Класс успешно добавлен" : "Изменения сохранены";
                    MessageBox.Show(message);

                    await LoadClassesData();

                    if (SelectedClass.Id == 0)
                    {
                        SelectedClass = null;
                        Students = new List<StudentAdminDTO>();
                    }
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var status = response.StatusCode == HttpStatusCode.BadRequest
                        ? errorContent
                        : "Ошибка сервера";
                    MessageBox.Show(status);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка сети: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
            }
        }

        private void AddClass_Button_Click(object sender, RoutedEventArgs e)
        {
            SelectedClass = new ClassesAdminDTO
            {
                Id = 0, 
                Name = string.Empty,
                AcademicYear = 0,
                ClassTeacherId = null
            };

            Students = new List<StudentAdminDTO>();

            SelectedTeacher = null;

            Signal(nameof(SelectedClass));
            Signal(nameof(Students));
            Signal(nameof(SelectedTeacher));
        }

        private void AddStudent_Button_Click(object sender, RoutedEventArgs e)
        {
            var studentEditPage = new AddStudentsToClassPage();

            NavigationService.Navigate(studentEditPage);
        }

        private async Task RemoveStudentFromClass(StudentAdminDTO student)
        {
            if (student == null || SelectedClass == null)
            {
                MessageBox.Show("Выберите класс и ученика");
                return;
            }

            try
            {
                var response = await _httpClient.DeleteAsync(
                    $"https://localhost:7273/api/Admin/remove-student?classId={SelectedClass.Id}&studentId={student.Id}");

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Ученик успешно удален из класса");
                    await LoadStudentsData(SelectedClass.Id);
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка: {error}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void FilterUsers()
        {
            if (Users == null) return;

            if (string.IsNullOrWhiteSpace(UserSearchText) || UserSearchText == "Поиск...")
            {
                FilteredUsers = Users.ToList();
            }
            else
            {
                var searchText = UserSearchText.ToLower();
                FilteredUsers = Users.Where(u =>
                    (!string.IsNullOrEmpty(u.Login) && u.Login.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(u.RoleName) && u.RoleName.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(u.PersonName) && u.PersonName.ToLower().Contains(searchText))
                ).ToList();
            }
        }

        private void FilterPersons()
        {
            if (Persons == null) return;

            if (string.IsNullOrWhiteSpace(PersonSearchText) || PersonSearchText == "Поиск...")
            {
                FilteredPersons = Persons.ToList();
            }
            else
            {
                var searchText = PersonSearchText.ToLower();
                FilteredPersons = Persons.Where(p =>
                    (!string.IsNullOrEmpty(p.PersonName) && p.PersonName.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(p.Phone) && p.Phone.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(p.Email) && p.Email.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(p.Address) && p.Address.ToLower().Contains(searchText))
                ).ToList();
            }
        }

        private void UserSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag?.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void UserSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag?.ToString();
                textBox.Foreground = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
                UserSearchText = ""; 
            }
        }

        private void PersonSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag?.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void PersonSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag?.ToString();
                textBox.Foreground = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
                PersonSearchText = ""; 
            }
        }

        private void FilterSubjectTeachers()
        {
            if (SubjectTeachers == null) return;

            if (string.IsNullOrWhiteSpace(SubjectTeacherSearchText) || SubjectTeacherSearchText == "Поиск...")
            {
                FilteredSubjectTeachers = SubjectTeachers.ToList();
            }
            else
            {
                var searchText = SubjectTeacherSearchText.ToLower();
                FilteredSubjectTeachers = SubjectTeachers.Where(st =>
                    (!string.IsNullOrEmpty(st.TeacherName) && st.TeacherName.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(st.SubjectName) && st.SubjectName.ToLower().Contains(searchText)) ||
                    (!string.IsNullOrEmpty(st.ClassName) && st.ClassName.ToLower().Contains(searchText)) ||
                    st.AcademicYear.ToString().Contains(searchText)
                ).ToList();
            }
        }

        private void SubjectTeacherSearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text == textBox.Tag?.ToString())
            {
                textBox.Text = "";
                textBox.Foreground = new SolidColorBrush(Colors.White);
            }
        }

        private void SubjectTeacherSearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = textBox.Tag?.ToString();
                textBox.Foreground = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xFF, 0xFF));
                SubjectTeacherSearchText = "";
            }
        }

        private void AddSubjectTeacher_Button_Click(object sender, RoutedEventArgs e)
        {
            var subjectTeacherEditPage = new AddSubjectTeacherPage();
            NavigationService.Navigate(subjectTeacherEditPage);
        }

        private void EditSubjectTeacher_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            var subjectTeacher = button.DataContext as SubjectTeacherDTO;
            if (subjectTeacher == null) return;

            var subjectTeacherEditPage = new AddSubjectTeacherPage(subjectTeacher);
            NavigationService.Navigate(subjectTeacherEditPage);
        }

        private async void DeleteSubjectTeacher_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var subjectTeacher = button?.DataContext as SubjectTeacherDTO;

            if (subjectTeacher != null)
            {
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить связь {subjectTeacher.TeacherName} - {subjectTeacher.SubjectName}?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var response = await _httpClient.DeleteAsync(
                            $"https://localhost:7273/api/Admin/delete-subject-teacher/{subjectTeacher.Id}");

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Связь успешно удалена", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadSubjectTeachersData();
                        }
                        else
                        {
                            var error = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Ошибка: {error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private DateOnly GetCurrentWeekStart()
        {
            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return DateOnly.FromDateTime(today.AddDays(-1 * diff));
        }

        private async void ShowSchedule_Click(object sender, RoutedEventArgs e)
        {
            await LoadScheduleData();
        }

        private void AddLesson_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClassForSchedule == null)
            {
                MessageBox.Show("Выберите класс");
                return;
            }

            var addLessonPage = new AddEditLessonPage(SelectedClassForSchedule.Value, CurrentWeekStart);
            NavigationService.Navigate(addLessonPage);
        }

        private async void BulkEditSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedClassForSchedule == null)
            {
                MessageBox.Show("Выберите класс", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DateOnly currentWeekStart = CurrentWeekStart;

                var confirmResult = MessageBox.Show(
                    $"Вы хотите продублировать неделю с {currentWeekStart:dd.MM.yyyy} на следующую неделю?",
                    "Подтверждение дублирования",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (confirmResult != MessageBoxResult.Yes)
                {
                    return;
                }

                DateOnly nextWeekStart = currentWeekStart.AddDays(7);

                var request = new CopyScheduleWeekRequest
                {
                    SourceWeekStart = currentWeekStart,
                    TargetWeekStart = nextWeekStart
                };
                var response = await _httpClient.PostAsJsonAsync(
                    "https://localhost:7273/api/Timetable/copy-week",
                    request);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<OperationResult>();
                    if (result.IsSuccess)
                    {
                        MessageBox.Show(result.Message, "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadScheduleData();
                    }
                    else
                    {
                        MessageBox.Show(result.Message, "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка сервера: {error}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при дублировании недели: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void PrevWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(-7);
            await LoadScheduleData();
        }

        private async void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(7);
            await LoadScheduleData();
        }

        private async void CurrentWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = GetCurrentWeekStart();
            await LoadScheduleData();
        }

        private void EditLessonContext_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var contextMenu = menuItem?.Parent as ContextMenu;
            var listView = contextMenu?.PlacementTarget as ListView;

            var selectedLesson = listView?.SelectedItem as ScheduleDTO;

            if (selectedLesson != null && SelectedClassForSchedule.HasValue)
            {
                var editLessonPage = new AddEditLessonPage(SelectedClassForSchedule.Value, CurrentWeekStart, selectedLesson);
                NavigationService.Navigate(editLessonPage);
            }
            else
            {
                MessageBox.Show("Не выбрано занятие для редактирования или не выбран класс");
            }
        }

        public class RelayCommand<T> : ICommand
        {
            private readonly Action<T> _execute;
            private readonly Func<T, bool> _canExecute;

            public event EventHandler CanExecuteChanged;

            public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute((T)parameter);

            public void Execute(object parameter) => _execute((T)parameter);

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public class DateOnlyJsonConverter : System.Text.Json.Serialization.JsonConverter<DateOnly>
        {
            private const string Format = "yyyy-MM-dd";

            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return DateOnly.ParseExact(reader.GetString(), Format);
            }

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value.ToString(Format));
            }
        }

        public class CopyScheduleWeekRequest
        {
            public DateOnly SourceWeekStart { get; set; }
            public DateOnly TargetWeekStart { get; set; }
        }

        public class OperationResult
        {
            public bool IsSuccess { get; set; }
            public string Message { get; set; }

            public static OperationResult Success(string message) =>
                new OperationResult { IsSuccess = true, Message = message };

            public static OperationResult Fail(string message) =>
                new OperationResult { IsSuccess = false, Message = message };
        }
    }

}