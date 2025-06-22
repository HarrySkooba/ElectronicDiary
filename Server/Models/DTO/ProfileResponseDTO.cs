namespace Server.Models.DTO
{
    public class ProfileResponseDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
    }
    public class SchoolResponseDTO
    {
        public string SchoolName { get; set; } = string.Empty;
        public string SchoolAddress { get; set; } = string.Empty;
        public string SchoolPhone { get; set; } = string.Empty;
        public string DirectorFirstName { get; set; } = string.Empty;
        public string DirectorLastName { get; set; } = string.Empty;
        public string DirectorMiddleName { get; set; } = string.Empty;
        public string SchoolWebsite { get; set; } = string.Empty;
    }

    public class ClassResponseDTO
    {
        public string ClassName { get; set; } = string.Empty;
        public string ClassTeacherName { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public List<StudentDto> Students { get; set; } = new();
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string FirstNameStudent { get; set; } = string.Empty;
        public string LastNameStudent { get; set; } = string.Empty;
        public string MiddleNameStudent { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
    }
}

