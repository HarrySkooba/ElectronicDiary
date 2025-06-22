using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using Client.DTO;

namespace Client.Pag
{
    public partial class ProfilePage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private ProfileResponseDTO _profile;
        public ProfileResponseDTO Profile
        {
            get => _profile;
            set { _profile = value; Signal(nameof(Profile)); }
        }

        private SchoolResponseDTO _school;
        public SchoolResponseDTO School
        {
            get => _school;
            set { _school = value; Signal(nameof(School)); }
        }

        private ClassResponseDTO _class;
        public ClassResponseDTO Class
        {
            get => _class;
            set { _class = value; Signal(nameof(Class)); }
        }

        private string _roleName;
        public string RoleName
        {
            get => _roleName;
            set { _roleName = value; Signal(nameof(RoleName)); }
        }

        private string _login;
        public string Login
        {
            get => _login;
            set { _login = value; Signal(nameof(Login)); }
        }

        public string FullName => $"{Profile?.LastName} {Profile?.FirstName} {Profile?.MiddleName}";
        public string DirectorFullName => $"{School?.DirectorLastName} {School?.DirectorFirstName} {School?.DirectorMiddleName}";

        public ProfilePage()
        {
            InitializeComponent();
            this.DataContext = this;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());

            var userInfo = App.Current.Properties["UserInfo"] as UserResponseDTO;
            if (userInfo != null)
            {
                RoleName = userInfo.RoleName;
                Login = userInfo.Login;
            }

            LoadDataAsync().ConfigureAwait(false);
        }

        private async Task LoadDataAsync()
        {
            try
            {
                await LoadProfileInfo();
                await LoadSchoolInfo();

                if (RoleName == "Student")
                {
                    await LoadClassInfo();
                }
                else
                {
                    var classTab = FindTabItem("Класс");
                    if (classTab != null) classTab.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private TabItem FindTabItem(string header)
        {
            foreach (var item in MainTabControl.Items)
            {
                if (item is TabItem tabItem && tabItem.Header.ToString() == header)
                {
                    return tabItem;
                }
            }
            return null;
        }

        private async Task LoadProfileInfo()
        {
            var response = await _httpClient.GetAsync("https://localhost:7273/api/Profile/profile");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            Profile = JsonSerializer.Deserialize<ProfileResponseDTO>(content);
            Signal(nameof(FullName));
        }

        private async Task LoadSchoolInfo()
        {
            var response = await _httpClient.GetAsync("https://localhost:7273/api/Profile/school");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            School = JsonSerializer.Deserialize<SchoolResponseDTO>(content);
            Signal(nameof(DirectorFullName));
        }

        private async Task LoadClassInfo()
        {
            var response = await _httpClient.GetAsync("https://localhost:7273/api/Profile/class");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
            Class = JsonSerializer.Deserialize<ClassResponseDTO>(content, options);
        }

        public void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}