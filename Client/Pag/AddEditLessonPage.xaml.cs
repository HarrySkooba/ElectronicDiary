using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using Client.DTO;
using Newtonsoft.Json;

namespace Client.Pag
{
    public partial class AddEditLessonPage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        private readonly int _classId;
        private readonly DateOnly _weekStart;
        private readonly ScheduleDTO? _lessonToEdit;
        private bool _isEditMode;

        private List<SubjectAdminDTO> _subjects;
        private List<TeacherAdminDTO> _teachers;
        private List<ClassTimeSlotDTO> _timeSlots;
        private List<ClassesAdminDTO> _classes;

        public List<SubjectAdminDTO> Subjects
        {
            get => _subjects;
            set { _subjects = value; Signal(nameof(Subjects)); }
        }

        public List<TeacherAdminDTO> Teachers
        {
            get => _teachers;
            set { _teachers = value; Signal(nameof(Teachers)); }
        }

        public List<ClassTimeSlotDTO> TimeSlots
        {
            get => _timeSlots;
            set { _timeSlots = value; Signal(nameof(TimeSlots)); }
        }

        public List<ClassesAdminDTO> Classs
        {
            get => _classes;
            set { _classes = value; Signal(nameof(Classs)); }
        }

        private SubjectAdminDTO _selectedSubject;
        public SubjectAdminDTO SelectedSubject
        {
            get => _selectedSubject;
            set
            {
                _selectedSubject = value;
                Signal(nameof(SelectedSubject));
            }
        }

        private TeacherAdminDTO _selectedTeacher;
        public TeacherAdminDTO SelectedTeacher
        {
            get => _selectedTeacher;
            set { _selectedTeacher = value; Signal(nameof(SelectedTeacher)); }
        }

        private ClassTimeSlotDTO _selectedTimeSlot;
        public ClassTimeSlotDTO SelectedTimeSlot
        {
            get => _selectedTimeSlot;
            set { _selectedTimeSlot = value; Signal(nameof(SelectedTimeSlot)); }
        }

        private string _room;
        public string Room
        {
            get => _room;
            set { _room = value; Signal(nameof(Room)); }
        }

        private DateTime _lessonDate;
        public DateTime LessonDate
        {
            get => _lessonDate;
            set
            {
                _lessonDate = value;
                Signal(nameof(LessonDate));
                UpdateDayOfWeek();
            }
        }

        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        public AddEditLessonPage(int classId, DateOnly weekStart, ScheduleDTO? lessonToEdit = null)
        {
            InitializeComponent();
            DataContext = this;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());

            _classId = classId;
            _weekStart = weekStart;
            _lessonToEdit = lessonToEdit;
            _isEditMode = lessonToEdit != null;
            LessonDate = _isEditMode && _lessonToEdit != null
           ? _lessonToEdit.Date.ToDateTime(TimeOnly.MinValue)
           : DateTime.Today;

            TitleText.Text = _isEditMode ? "Редактирование урока" : "Добавление урока";

            LoadInitialData();
        }

        private async void LoadInitialData()
        {
            try
            {
                var subjectsResponse = await _httpClient.GetAsync($"https://localhost:7273/api/Admin/subjects");
                subjectsResponse.EnsureSuccessStatusCode();
                Subjects = await subjectsResponse.Content.ReadFromJsonAsync<List<SubjectAdminDTO>>();

                var teachersResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/teachers");
                teachersResponse.EnsureSuccessStatusCode();
                Teachers = await teachersResponse.Content.ReadFromJsonAsync<List<TeacherAdminDTO>>();

                var classesResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/classes");
                classesResponse.EnsureSuccessStatusCode();
                Classs = await classesResponse.Content.ReadFromJsonAsync<List<ClassesAdminDTO>>();

                var timeSlotsResponse = await _httpClient.GetAsync("https://localhost:7273/api/Timetable/time-slots");
                timeSlotsResponse.EnsureSuccessStatusCode();
                TimeSlots = await timeSlotsResponse.Content.ReadFromJsonAsync<List<ClassTimeSlotDTO>>();



                if (_isEditMode && _lessonToEdit != null)
                {
                    SelectedSubject = Subjects.FirstOrDefault(s => s.Name == _lessonToEdit.Subject);

                    SelectedTeacher = Teachers.FirstOrDefault(t =>
                        t.FullName.StartsWith(_lessonToEdit.Teacher.Split(' ')[0])); 

                    SelectedTimeSlot = TimeSlots.FirstOrDefault(t =>
                        t.LessonNumber == _lessonToEdit.Lesson_Number);

                   
                    Room = _lessonToEdit.Room;

                    DayOfWeekComboBox.SelectedIndex = (int)_lessonToEdit.Day_Of_Week - 1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSubject == null || SelectedTeacher == null || SelectedTimeSlot == null)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                ClassesAdminDTO foundClass = Classs.FirstOrDefault(c => c.Id == _classId);
                var scheduleDay = new ScheduleDayDto
                {
                    Date = DateOnly.FromDateTime(LessonDate),
                    DayOfWeek = DayOfWeekComboBox.SelectedIndex + 1,
                    Lessons = new List<ScheduleDTO>
            {
                new ScheduleDTO
                {
                    Id = _isEditMode ? _lessonToEdit?.Id ?? 0 : 0,
                    Subject = SelectedSubject.Name,
                    Teacher = $"{SelectedTeacher.LastName} {SelectedTeacher.FirstName[0]}.",
                    Lesson_Number = SelectedTimeSlot.LessonNumber,
                    Start_Time = SelectedTimeSlot.StartTime,
                    End_Time = SelectedTimeSlot.EndTime,
                    Room = Room,
                    ClassName = foundClass.Name,
                    Date = DateOnly.FromDateTime(LessonDate),
            }
                }
                };
                var response = await _httpClient.PostAsJsonAsync(
                    "https://localhost:7273/api/Timetable/edit",
                    new List<ScheduleDayDto> { scheduleDay });

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Урок успешно сохранен", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.GoBack();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка сохранения: {error}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void UpdateDayOfWeek()
        {
            int dayIndex = (int)_lessonDate.DayOfWeek - 1;
            DayOfWeekComboBox.SelectedIndex = dayIndex >= 0 ? dayIndex : 6;
        }
    }

    public class ClassTimeSlotDTO
    {
        public int LessonNumber { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public TimeSpan? BreakAfterDuration { get; set; }

        public string TimeSlotDisplay =>
            $"{LessonNumber} урок: {StartTime:HH\\:mm} - {EndTime:HH\\:mm}" +
            (BreakAfterDuration.HasValue ? $" (перемена {BreakAfterDuration.Value.TotalMinutes} мин)" : "");
    }


}