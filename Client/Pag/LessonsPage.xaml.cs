using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Client.DTO;

namespace Client.Pag
{
    public partial class LessonsPage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; Signal(nameof(IsLoading)); }
        }

        private bool _isTeacher;
        public bool IsTeacher
        {
            get => _isTeacher;
            set { _isTeacher = value; Signal(nameof(IsTeacher)); }
        }

        private DateOnly _currentWeekStart;
        public DateOnly CurrentWeekStart
        {
            get => _currentWeekStart;
            set { _currentWeekStart = value; Signal(nameof(CurrentWeekStart)); Signal(nameof(WeekRangeText)); }
        }

        public string WeekRangeText => $"{CurrentWeekStart:dd.MM.yyyy} - {CurrentWeekStart.AddDays(6):dd.MM.yyyy}";

        public ObservableCollection<DayLessons> Days { get; } = new ObservableCollection<DayLessons>();

        public LessonsPage()
        {
            InitializeComponent();
            DataContext = this;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());

            var userInfo = App.Current.Properties["UserInfo"] as UserResponseDTO;
            IsTeacher = userInfo?.RoleName == "Teacher" || userInfo?.RoleName == "Director";

            CurrentWeekStart = GetCurrentWeekStart();

            LoadLessonsAsync().ConfigureAwait(false);
        }

        private DateOnly GetCurrentWeekStart()
        {
            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return DateOnly.FromDateTime(today.AddDays(-1 * diff));
        }

        private async Task LoadLessonsAsync(int? classId = null, int? subjectId = null)
        {
            try
            {
                IsLoading = true;
                Days.Clear();

                var baseUrl = "https://localhost:7273/"; 
                var weekEnd = CurrentWeekStart.AddDays(6);

                var queryParams = new List<string>
        {
            $"startDate={CurrentWeekStart:yyyy-MM-dd}",
            $"endDate={weekEnd:yyyy-MM-dd}"
        };

                if (IsTeacher && classId.HasValue)
                    queryParams.Add($"classId={classId.Value}");

                if (IsTeacher && subjectId.HasValue)
                    queryParams.Add($"subjectId={subjectId.Value}");

                var endpoint = IsTeacher ? "api/Lessons/teacher" : "api/Lessons/student";
                var requestUri = $"{baseUrl}{endpoint}?{string.Join("&", queryParams)}";

                Debug.WriteLine($"Sending request to: {requestUri}");

                var response = await _httpClient.GetAsync(requestUri);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"HTTP {response.StatusCode}: {errorContent}");
                }

                var lessons = await response.Content.ReadFromJsonAsync<List<LessonViewDto>>();
                UpdateDays(lessons);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateDays(List<LessonViewDto> lessons)
        {
            if (lessons == null) return;

            for (int i = 0; i < 6; i++)
            {
                var currentDate = CurrentWeekStart.AddDays(i);
                var dayName = CultureInfo.CurrentCulture.DateTimeFormat.GetDayName((DayOfWeek)((i + 1) % 7));
                dayName = char.ToUpper(dayName[0]) + dayName.Substring(1);

                var dayLessons = lessons
                    .Where(l => l.Date == currentDate)
                    .OrderBy(l => l.Date)
                    .ToList();

                Days.Add(new DayLessons
                {
                    DayName = dayName,
                    Date = currentDate,
                    Lessons = dayLessons,
                    IsCurrentDay = currentDate == DateOnly.FromDateTime(DateTime.Now)
                });
            }
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                IsLoading = true;

                var updates = Days
                    .SelectMany(d => d.Lessons)
                    .Where(l => l.IsModified)
                    .Select(l => new UpdateGradesDTO
                    {
                        LessonId = l.Id,
                        Homework = l.Homework,
                        Grades = l.Grades.Select(g => new GradeUpdateDTO
                        {
                            StudentId = g.StudentId,
                            Score = g.Score,
                            WasPresent = g.WasPresent,
                            Comment = g.Comment
                        }).ToList()
                    }).ToList();

                if (updates.Count == 0)
                {
                    MessageBox.Show("Нет изменений для сохранения", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                foreach (var update in updates)
                {
                    var response = await _httpClient.PutAsJsonAsync(
                        "https://localhost:7273/api/Lessons/update-grades", update);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Ошибка сохранения: {response.StatusCode}\n{errorContent}");
                    }
                }

                MessageBox.Show($"Сохранено {updates.Count} изменений", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                await LoadLessonsAsync();
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadLessonsAsync();
        }

        private async void PrevWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(-7);
            await LoadLessonsAsync();
        }

        private async void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(7);
            await LoadLessonsAsync();
        }

        private async void CurrentWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = GetCurrentWeekStart();
            await LoadLessonsAsync();
        }

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            await SaveChangesAsync();
        }

        private void Homework_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is LessonViewDto lesson)
            {
                lesson.IsModified = true;
                Debug.WriteLine($"Homework changed for lesson {lesson.Id}, marked as modified");
            }
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit &&
                sender is DataGrid dataGrid &&
                dataGrid.Tag is LessonViewDto lesson)
            {
                lesson.IsModified = true;
                Debug.WriteLine($"Cell edited for lesson {lesson.Id}, marked as modified");
            }
        }

        private void DataGrid_CurrentCellChanged(object sender, EventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.DataContext is LessonViewDto lesson)
            {
                lesson.IsModified = true;
                Debug.WriteLine($"Current cell changed for lesson {lesson.Id}, marked as modified");
            }
        }

    }

    public class DayLessons
    {
        public string DayName { get; set; }
        public DateOnly Date { get; set; }
        public List<LessonViewDto> Lessons { get; set; }
        public bool IsCurrentDay { get; set; }
    }
}