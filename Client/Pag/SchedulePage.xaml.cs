using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Client.DTO;

namespace Client.Pag
{
    public partial class SchedulePage : Page, INotifyPropertyChanged
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

        private string _title;
        public string Title
        {
            get => _title;
            set { _title = value; Signal(nameof(Title)); }
        }

        private DateOnly _currentWeekStart;
        public DateOnly CurrentWeekStart
        {
            get => _currentWeekStart;
            set { _currentWeekStart = value; Signal(nameof(CurrentWeekStart)); Signal(nameof(WeekRangeText)); }
        }

        public string WeekRangeText => $"{CurrentWeekStart:dd.MM.yyyy} - {CurrentWeekStart.AddDays(6):dd.MM.yyyy}";

        public ObservableCollection<DaySchedule> Days { get; } = new ObservableCollection<DaySchedule>();

        public SchedulePage()
        {
            InitializeComponent();
            DataContext = this;

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());

            var userInfo = App.Current.Properties["UserInfo"] as UserResponseDTO;
            Title = userInfo?.RoleName == "Teacher" ? "Моё расписание (Учитель)" : "Моё расписание";

            CurrentWeekStart = GetCurrentWeekStart();

            LoadScheduleAsync().ConfigureAwait(false);
        }

        private DateOnly GetCurrentWeekStart()
        {
            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return DateOnly.FromDateTime(today.AddDays(-1 * diff));
        }

        private async Task LoadScheduleAsync()
        {
            try
            {
                IsLoading = true;
                Days.Clear();

                var userInfo = App.Current.Properties["UserInfo"] as UserResponseDTO;
                string endpoint;
                string role = userInfo?.RoleName;

                if (role == "Teacher" || role == "Director")
                {
                    endpoint = $"api/Timetable/schedule_teacher?weekStart={CurrentWeekStart:yyyy-MM-dd}";
                }
                else if (role == "Student" || role == "Parent")
                {
                    endpoint = $"api/Timetable/schedule?weekStart={CurrentWeekStart:yyyy-MM-dd}";
                }
                else
                {
                    throw new Exception("Недостаточно прав для просмотра расписания");
                }

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

                var schedule = JsonSerializer.Deserialize<Dictionary<DateOnly, List<ScheduleDTO>>>(content, options);

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

                        Days.Add(new DaySchedule
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
                Debug.WriteLine($"Error details: {ex}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadScheduleAsync();
        }

        private async void PrevWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(-7);
            await LoadScheduleAsync();
        }

        private async void NextWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = CurrentWeekStart.AddDays(7);
            await LoadScheduleAsync();
        }

        private async void CurrentWeekButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentWeekStart = GetCurrentWeekStart();
            await LoadScheduleAsync();
        }
    }

    public class DaySchedule
    {
        public string DayName { get; set; }
        public DateOnly Date { get; set; }
        public List<ScheduleDTO> Lessons { get; set; }
        public bool IsCurrentDay { get; set; }
    }

    public class DateOnlyJsonConverter : JsonConverter<DateOnly>
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
}