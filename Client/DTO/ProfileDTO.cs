using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.DTO
{
    public class ProfileResponseDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string BirthDate { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class SchoolResponseDTO
    {
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        public string SchoolPhone { get; set; }
        public string DirectorFirstName { get; set; }
        public string DirectorLastName { get; set; }
        public string DirectorMiddleName { get; set; }
        public string SchoolWebsite { get; set; }
    }

    public class ClassResponseDTO
    {
        public string ClassName { get; set; }
        public string ClassTeacherName { get; set; }
        public int AcademicYear { get; set; }
        public List<StudentDto> Students { get; set; } = new List<StudentDto>();
    }

    public class StudentDto
    {
        public int id { get; set; }
        public string FirstNameStudent { get; set; }
        public string LastNameStudent { get; set; }
        public string MiddleNameStudent { get; set; }
        public string PhotoUrl { get; set; }
        public string FullName => $"{LastNameStudent} {FirstNameStudent} {MiddleNameStudent}";
    }
}
