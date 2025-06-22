using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Client.DTO;

namespace Client.Pag
{
    public partial class AddPersonPage : Page, INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        public event PropertyChangedEventHandler? PropertyChanged;
        void Signal(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));

        private PersonAdminDTO _person;
        public PersonAdminDTO Person
        {
            get => _person;
            set
            {
                _person = value;
                Signal(nameof(Person));
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

        public DateTime? BirthDate
        {
            get => Person.BirthDate.HasValue ? Person.BirthDate.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;
            set
            {
                Person.BirthDate = value.HasValue ? DateOnly.FromDateTime(value.Value) : (DateOnly?)null;
                Signal(nameof(BirthDate));
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddPersonPage(PersonAdminDTO person = null, HttpClient httpClient = null)
        {
            InitializeComponent();
            this.DataContext = this;
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", App.Current.Properties["AuthToken"]?.ToString());
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);

            if (person != null)
            {
                Person = person;
                Title = "Редактирование персоны";
            }
            else
            {
                Person = new PersonAdminDTO
                {
                    IdSchool = 4,
                    SchoolName = "Гимназия №2"
                };
                Title = "Добавление персоны";
            }
        }

        private async void Save(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Person.LastName) || string.IsNullOrWhiteSpace(Person.FirstName))
            {
                MessageBox.Show("Фамилия и имя обязательны для заполнения");
                return;
            }

            try
            {
                AddPerson addPerson = new AddPerson
                {
                    FirstName = FirstNameTB.Text,
                    LastName = LastNameTB.Text,
                    MiddleName = MiddleNameTB.Text,
                    BirthDate = BirthDP.SelectedDate.HasValue ? DateOnly.FromDateTime(BirthDP.SelectedDate.Value) : (DateOnly?)null,
                    Email = EmailTB.Text,
                    Phone = PhoneTB.Text,
                    Address = AddressTB.Text,
                    PhotoUrl = PhoneTB.Text,
                    IdSchool = Person.IdSchool,
                };
                if (Person.Id > 0)
                {
                    int personId = Person.Id;
                    var response = await _httpClient.PutAsJsonAsync($"https://localhost:7273/api/Admin/updateperson/{personId}", addPerson);
                    if (response.IsSuccessStatusCode)
                    {
                        NavigationService.GoBack();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при обновлении персоны");
                    }
                }
                else
                {
                    var response = await _httpClient.PostAsJsonAsync("https://localhost:7273/api/Admin/addperson", addPerson);
                    if (response.IsSuccessStatusCode)
                    {
                        NavigationService.GoBack();
                    }
                    else
                    {
                        MessageBox.Show("Ошибка при добавлении персоны");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Cancel(object parameter)
        {
            NavigationService.GoBack();
        }
    }
}