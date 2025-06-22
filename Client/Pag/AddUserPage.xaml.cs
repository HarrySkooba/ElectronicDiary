using System.ComponentModel;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.DTO;

namespace Client.Pag
{
    public partial class AddUserPage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        private UserAdminDTO _user;
        public UserAdminDTO User
        {
            get => _user;
            set
            {
                _user = value;
                Signal(nameof(User));
            }
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

        private List<RoleAdminDTO> _roles;
        public List<RoleAdminDTO> Roles
        {
            get => _roles;
            set
            {
                _roles = value;
                Signal(nameof(Roles));
            }
        }

        private RoleAdminDTO _selectedRole;
        public RoleAdminDTO SelectedRole
        {
            get => _selectedRole;
            set
            {
                _selectedRole = value;
                Signal(nameof(SelectedRole));
            }
        }

        private List<PersonAdminDTO> _persons;
        public List<PersonAdminDTO> Persons
        {
            get => _persons;
            set
            {
                _persons = value;
                Signal(nameof(Persons));
            }
        }

        private PersonAdminDTO _selectedPerson;
        public PersonAdminDTO SelectedPerson
        {
            get => _selectedPerson;
            set
            {
                _selectedPerson = value;
                Signal(nameof(SelectedPerson));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public AddUserPage(UserAdminDTO user = null, HttpClient httpClient = null)
        {
            InitializeComponent();
            this.DataContext = this;
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);

            if (user != null)
            {
                User = user;
                Title = "Редактирование аккаунта";
            }
            else
            {
                User = new UserAdminDTO();
                Title = "Добавление аккаунта";
            }

            LoadData();
        }
        private async void LoadData()
        {
            try
            {
                var rolesResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/roles");
                rolesResponse.EnsureSuccessStatusCode();
                Roles = await rolesResponse.Content.ReadFromJsonAsync<List<RoleAdminDTO>>();

                var personsResponse = await _httpClient.GetAsync("https://localhost:7273/api/Admin/persons");
                personsResponse.EnsureSuccessStatusCode();
                Persons = await personsResponse.Content.ReadFromJsonAsync<List<PersonAdminDTO>>();

                if (User.Idrole > 0)
                    SelectedRole = Roles.FirstOrDefault(r => r.Id == User.Idrole);

                if (User.IdPerson > 0)
                    SelectedPerson = Persons.FirstOrDefault(p => p.Id == User.IdPerson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }
        private async void Save(object parameter)
        {
                if (SelectedRole != null && SelectedPerson != null && !string.IsNullOrWhiteSpace(tblogin.Text) && !string.IsNullOrWhiteSpace(tbpass.Text))
                {
                    AddUser addUser = new AddUser
                    {
                        Login = tblogin.Text,
                        Password = tbpass.Text,
                        RoleId = SelectedRole.Id,
                        PersonId = SelectedPerson.Id
                    };
                    if (User.Id > 0)
                    {
                    var originalRole = Roles.FirstOrDefault(r => r.Id == User.Idrole);

                    if (originalRole != null && originalRole.Name == "Admin" && SelectedRole.Id != originalRole.Id)
                    {
                        MessageBox.Show("Нельзя изменять роль администратора");
                        return;
                    }
                    await UpdateUserAsync(User.Id, addUser);
                    }
                    else
                    {
                       await AddUserAsync(addUser);
                    }
                }
                else
                {
                    MessageBox.Show("Не оставляйте пустые поля");
                }
        }
     
        private void Cancel(object parameter)
        {
            NavigationService.GoBack();
        }

        public async Task AddUserAsync(AddUser addUser)
        {
            try
            {
                var json = JsonSerializer.Serialize(addUser);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var adduser = await _httpClient.PostAsync("https://localhost:7273/api/Admin/adduser", content);
                if (adduser.IsSuccessStatusCode)
                {
                    NavigationService.GoBack(); 
                }
                else
                {
                    var errorContent = await adduser.Content.ReadAsStringAsync();
                    var status = adduser.StatusCode == HttpStatusCode.BadRequest ? errorContent : "Ошибка сервера";
                    MessageBox.Show(status);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async Task UpdateUserAsync(int userId, AddUser addUser)
        {
            try
            {
                var json = JsonSerializer.Serialize(addUser);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"https://localhost:7273/api/Admin/updateuser/{userId}", content);
                if (response.IsSuccessStatusCode)
                {
                    NavigationService.GoBack();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var status = response.StatusCode == HttpStatusCode.BadRequest ? errorContent : "Ошибка сервера";
                    MessageBox.Show(status);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter) => _execute(parameter);
    }
}