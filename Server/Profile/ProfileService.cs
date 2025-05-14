using Microsoft.EntityFrameworkCore;
using Server.Models.Context;
using Server.Models.DTO;

namespace Server.Profile
{
    public class ProfileService : IProfileService
    {
        private readonly ElectronicDiaryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProfileService(ElectronicDiaryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ProfileResponseDTO> GetProfileInfo()
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            return new ProfileResponseDTO
            {
                FirstName = user.Person.FirstName,
                LastName = user.Person.LastName,
                MiddleName = user.Person.MiddleName,
                BirthDate = user.Person.BirthDate.ToString(),
                Email = user.Person.Email,
                Phone = user.Person.Phone,
                Addres = user.Person.Address,
                PhotoUrl = user.Person.PhotoUrl
            };
        }

        public async Task<SchoolResponseDTO> GetSchoolInfo()
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person.Schools)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            return new SchoolResponseDTO
            {
               SchoolName = user.Person.School.Name,
               SchoolAddres = user.Person.School.Address,
               SchoolPhone = user.Person.School.Phone,
               DirectorFirstName = user.Person.School.Director.FirstName,
               DirectorLastName = user.Person.School.Director.LastName,
               DirectorMiddleName = user.Person.School.Director.MiddleName,
               SchoolWebsite = user.Person.School.Website
            };
        }

        public async Task<ClassResponseDTO> GetClassInfo()
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user?.Person == null)
                throw new KeyNotFoundException("Пользователь не найден");

            var classStudent = await _context.ClassStudents
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.ClassTeacher)
                .Include(cs => cs.Class)
                    .ThenInclude(c => c.School)
                .Include(cs => cs.Student)
                .FirstOrDefaultAsync(cs =>
                    cs.StudentId == user.Person.Id);
                    

            if (classStudent == null)
                throw new KeyNotFoundException("Пользователь не привязан к активному классу");

            var classmates = await _context.ClassStudents
                .Where(cs =>
                    cs.ClassId == classStudent.ClassId &&
                    cs.StudentId != user.Person.Id)
                .Include(cs => cs.Student)
                .ToListAsync();

            var students = new List<StudentDto>
            {
            new StudentDto
            {

            FirstNameStudent = user.Person.FirstName,
            LastNameStudent = user.Person.LastName,
            MiddleNameStudent = user.Person.MiddleName ?? string.Empty,
            }
            };

            students.AddRange(classmates.Select(cs => new StudentDto
            {
                FirstNameStudent = cs.Student.FirstName,
                LastNameStudent = cs.Student.LastName,
                MiddleNameStudent = cs.Student.MiddleName ?? string.Empty,
            }));

            return new ClassResponseDTO
            {
                ClassName = classStudent.Class.Name,
                ClassTeacherName = classStudent.Class.ClassTeacher != null
                    ? $"{classStudent.Class.ClassTeacher.LastName} {classStudent.Class.ClassTeacher.FirstName} {classStudent.Class.ClassTeacher.MiddleName}"
                    : "Классный руководитель не назначен",
                AcademicYear = classStudent.Class.AcademicYear,
                Students = students
            };
        }
    }
}
