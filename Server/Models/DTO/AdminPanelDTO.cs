using Server.DatabaseModel;

namespace Server.Models.DTO
{
    public class PersonAdminDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PhotoUrl { get; set; }
        public int IdSchool { get; set; }
        public string? SchoolName { get; set; }
        public string? PersonName { get; set; }

        public PersonAdminDTO(Person person)
        {
            Id = person.Id;
            FirstName = person.FirstName;
            LastName = person.LastName;
            MiddleName = person.MiddleName;
            BirthDate = person.BirthDate;
            Email = person.Email;
            Phone = person.Phone;
            Address = person.Address;
            PhotoUrl = person.PhotoUrl;
            IdSchool = person.Schoolid;
            SchoolName = person.School.Name;
            PersonName = $"{person.LastName} {person.FirstName} {person.MiddleName}";
        }
    }

    public class UserAdminDTO
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public byte[] Password { get; set; }
        public byte[] Salt { get; set; }
        public int Idrole { get; set; }
        public int IdPerson { get; set; }
        public string? RoleName { get; set; }
        public string? PersonName { get; set; }

        public UserAdminDTO(User user)
        {
            Id = user.Id;
            Login = user.Login;
            Password = user.PasswordHash;
            Salt = user.Salt;
            Idrole = user.RoleId;
            IdPerson = user.PersonId;
            RoleName = user.Role.Name;
            PersonName = $"{user.Person.LastName} {user.Person.FirstName} {user.Person.MiddleName}";
        }
    }

    public class SchoolAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int? IdDirector { get; set; }
        public string Website { get; set; }
    }

    public class DirectorsAdminDTO
    {
        public int Id { get; set; }
        public string DirectorName { get; set; }
        public DirectorsAdminDTO(Person person)
        {
            Id = person.Id;
            DirectorName = $"{person.LastName} {person.FirstName} {person.MiddleName}";
        }
    }

    public class RoleAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public RoleAdminDTO(Role role)
        {
            Id = role.Id;
            Name = role.Name;
        }
    }

    public class AddUserDTO
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public int PersonId { get; set; }
    }

    public class AddPersonDTO
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PhotoUrl { get; set; }
        public int IdSchool { get; set; }
    }

    public class UpdateSchoolDTO
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int IdDirector { get; set; }
        public string Website { get; set; }
    }

    public class ClassesAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AcademicYear { get; set; }
        public int? ClassTeacherId { get; set; }
        public string TeacherName { get; set; }

        public ClassesAdminDTO(Class classes)
        {
            Id = classes.Id;
            Name = classes.Name;
            ClassTeacherId = classes.ClassTeacherId;
            AcademicYear = classes.AcademicYear;
            if (classes.ClassTeacher != null)
            {
                TeacherName = $"{classes.ClassTeacher.LastName} {classes.ClassTeacher.FirstName} {classes.ClassTeacher.MiddleName}";
            }
        }
    }

    public class AllClassesAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public AllClassesAdminDTO(Class classes)
        {
            Id = classes.Id;
            Name = classes.Name;
        }
    }

    public class TeacherAdminDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }

        public TeacherAdminDTO(Person person)
        {
            Id = person.Id;
            FullName = $"{person.LastName} {person.FirstName} {person.MiddleName}";
        }
    }

    public class StudentAdminDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }

        public StudentAdminDTO(Person person)
        {
            Id = person.Id;
            FullName = $"{person.LastName} {person.FirstName} {person.MiddleName}";
            PhotoUrl = person.PhotoUrl;
        }
    }

    public class UpdateClassDto
    {
        public string Name { get; set; }
        public int AcademicYear { get; set; }
        public int? ClassTeacherId { get; set; }
    }

    public class AddClassDto
    {
        public string Name { get; set; }
        public int AcademicYear { get; set; }
        public int? ClassTeacherId { get; set; }
    }

    public class AddStudentsToClassRequest
    {
        public int ClassId { get; set; }
        public List<int> StudentIds { get; set; } = new List<int>();
    }

    public class SubjectTeacherDTO
    {
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int AcademicYear { get; set; }
    }

    public class AddSubjectTeacherDTO
    {
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public int ClassId { get; set; }
        public int AcademicYear { get; set; }
    }

    public class SubjectAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public SubjectAdminDTO(Subject subject)
        {
            Id = subject.Id;
            Name = subject.Name;
        }
    }
}

