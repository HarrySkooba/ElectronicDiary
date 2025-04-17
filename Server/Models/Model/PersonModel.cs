using Server.Models.DatabaseModel;

namespace Server.Models.Model
{
    public class PersonModel
    {
        public int Idperson { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public DateOnly? BirthDate { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        public string? PhotoUrl { get; set; }

        public PersonModel(Person person)
        {
            if (person != null)
            {
                Idperson = person.Id;
                FirstName = person.FirstName;
                LastName = person.LastName;
                MiddleName = person.MiddleName;
                BirthDate = person.BirthDate;
                Email = person.Email;
                Phone = person.Phone;
                Address = person.Address;
                PhotoUrl = person.PhotoUrl;
            }
            else
            {
                Idperson = -1;
                FirstName = "Unknown";
                LastName = "Unknown";
                MiddleName = "Unknown";
                BirthDate = null;
                Email = "Unknown";
                Phone = "Unknown";
                Address = "Unknown";
                PhotoUrl = "Unknown";
            }
        }
    }
}
