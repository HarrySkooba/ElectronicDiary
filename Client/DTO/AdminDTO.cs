namespace Client.DTO
{
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
    }
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
        public int? IdSchool { get; set; }
        public string? SchoolName { get; set; }
        public string? PersonName { get; set; }
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
    public class DirectorAdminDTO
    {
        public int Id { get; set; }
        public string DirectorName { get; set; }
    }
    public class RoleAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AddUser
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public int PersonId { get; set; }
    }
    public class AddPerson
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? MiddleName { get; set; }
        public DateOnly? BirthDate { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? PhotoUrl { get; set; }
        public int? IdSchool { get; set; }
    }
    public class UpdateSchool
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public int? IdDirector { get; set; }
        public string Website { get; set; }
    }
    public class ClassesAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AcademicYear { get; set; }
        public int? ClassTeacherId { get; set; }
        public string TeacherName { get; set; }
    }
    public class AllClassesAdminDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class TeacherAdminDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string LastName => FullName?.Split(' ').FirstOrDefault();
        public string FirstName => FullName?.Split(' ').Length > 1 ? FullName.Split(' ')[1] : "";
    }
    public class StudentAdminDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhotoUrl { get; set; }
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
        public List<int> StudentIds { get; set; }
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
        public string Name { get; set; }
    }
}
