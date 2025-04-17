using System.Windows;
using System.Windows.Input;
using Client.Wind;

namespace Client
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }
        private void ExitButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void MinButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void ToolBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void Logo_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (tb2.Password.Length > 0)
            {
                WaterMark.Visibility = Visibility.Collapsed;
            }
            else
            {
                WaterMark.Visibility = Visibility.Visible;
            }
        }
        private void Auth_Button(object sender, RoutedEventArgs e)
        {
            CheckAuth(tb1.Text, tb2.Password);
        }
        private void CheckAuth(string login, string password)
        {
            if (login == "123")
            {
                if (password == "123")
                {
                    MainWindowApp mainWindowApp = new MainWindowApp();
                    mainWindowApp.Show();
                    Hide();
                }
            }
        }
    }
}