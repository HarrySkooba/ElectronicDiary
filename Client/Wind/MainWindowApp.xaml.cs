using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Client.Pag;
using System.Linq;
using Client.DTO;

namespace Client.Wind
{
    public partial class MainWindowApp : Window
    {
        private Button _activeButton;
        private string RoleName;
        public MainWindowApp()
        {
            InitializeComponent();
            var userInfo = App.Current.Properties["UserInfo"] as UserResponseDTO;
            if (userInfo != null)
            {
                RoleName = userInfo.RoleName;
            }
            CheckAdminRole();
            if (RoleName == "Admin")
            {
                btnMain.Visibility = Visibility.Collapsed;
                btnLessons.Visibility = Visibility.Collapsed;
                btnReport.Visibility = Visibility.Collapsed;
                btnSchedule.Visibility = Visibility.Collapsed;
                Loaded += (s, e) => NavigateToAdminPanel();
            }
            else
            {
                Loaded += (s, e) => NavigateToProfile();
            }
        }

        private void CheckAdminRole()
        {
            if (RoleName != null && RoleName == "Admin")
            {
                btnAdmin.Visibility = Visibility.Visible;
            }
        }

        private void NavigateToProfile()
        {
            MyFrame.Content = new ProfilePage();
            SetActiveButton(btnMain);
        }

        private void NavigateToAdminPanel()
        {
            MyFrame.Content = new AdminPage();
            SetActiveButton(btnAdmin);
        }

        private void SetActiveButton(Button button)
        {
            if (_activeButton != null)
            {
                _activeButton.Style = (Style)FindResource("MenuButtonStyle");
            }

            _activeButton = button;
            if (_activeButton != null)
            {
                _activeButton.Style = (Style)FindResource("ActiveMenuButtonStyle");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyFrame.Content = new ProfilePage();
            SetActiveButton(sender as Button);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MyFrame.Content = new SchedulePage();
            SetActiveButton(sender as Button);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            // MyFrame.Content = new ReportPage();
            SetActiveButton(sender as Button);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            MyFrame.Content = new LessonsPage();
            SetActiveButton(sender as Button);
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            MyFrame.Content = new AdminPage(); 
            SetActiveButton(sender as Button);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
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
    }
}