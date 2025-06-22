using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Client.DTO;
using Client.Wind;

namespace Client
{
    public partial class LoginWindow : Window
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://localhost:7273/api/Auth/";

        public LoginWindow()
        {
            InitializeComponent();

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(ApiBaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
            => Application.Current.Shutdown();

        private void MinButton_MouseDown(object sender, MouseButtonEventArgs e)
            => WindowState = WindowState.Minimized;

        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Logo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            WaterMark.Visibility = tb2.Password.Length > 0
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private async Task<AuthResult> AuthenticateAsync(UserLoginDTO loginModel)
        {
            try
            {
                var json = JsonSerializer.Serialize(loginModel);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var userResponse = JsonSerializer.Deserialize<UserResponseDTO>(responseContent, options);

                    return new AuthResult
                    {
                        IsSuccess = true,
                        Token = userResponse.Token,
                        UserInfo = userResponse
                    };
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = response.StatusCode == HttpStatusCode.BadRequest
                        ? errorContent
                        : "Ошибка сервера"
                };
            }
            catch (HttpRequestException ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Ошибка сети: {ex.Message}"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Неожиданная ошибка: {ex.Message}"
                };
            }
        }

        private async void Auth_Button(object sender, RoutedEventArgs e)
        {
            AuthButton.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                if (string.IsNullOrWhiteSpace(tb1.Text) || string.IsNullOrWhiteSpace(tb2.Password))
                {
                    MessageBox.Show("Пожалуйста, введите логин и пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var loginModel = new UserLoginDTO
                {
                    Login = tb1.Text,
                    Password = tb2.Password
                };

                var authResult = await AuthenticateAsync(loginModel);

                if (authResult.IsSuccess)
                {
                    App.Current.Properties["AuthToken"] = authResult.Token;
                    App.Current.Properties["UserInfo"] = authResult.UserInfo;

                    var mainWindow = new MainWindowApp();
                    mainWindow.Show();
                    Close();
                }
                else
                {
                    MessageBox.Show(authResult.ErrorMessage, "Ошибка авторизации",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                AuthButton.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }
        private class AuthResult
        {
            public bool IsSuccess { get; set; }
            public string Token { get; set; } = string.Empty;
            public UserResponseDTO? UserInfo { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}