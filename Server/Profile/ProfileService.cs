using Microsoft.EntityFrameworkCore;
using Server.Models.DTO;
using Server.Utils;


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

            if (user?.Person == null)
                throw new KeyNotFoundException("Данные пользователя не найдены");

            return new ProfileResponseDTO
            {
                FirstName = user.Person.FirstName.GetValueOrDefault(),
                LastName = user.Person.LastName.GetValueOrDefault(),
                MiddleName = user.Person.MiddleName.GetValueOrDefault(),
                BirthDate = user.Person.BirthDate.GetValueOrDefaultDate(),
                Email = user.Person.Email.GetValueOrDefault(),
                Phone = user.Person.Phone.GetValueOrDefault(),
                Address = user.Person.Address.GetValueOrDefault(),
                PhotoUrl = user.Person.PhotoUrl.GetValueOrDefault("default-avatar.jpg")
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

            if (user?.Person == null)
                throw new KeyNotFoundException("Данные пользователя не найдены");

            if (user.Person.School == null)
                throw new KeyNotFoundException("Школа не найдена");

            return new SchoolResponseDTO
            {
                SchoolName = user.Person.School?.Name.GetValueOrDefault(),
                SchoolAddress = user.Person.School?.Address.GetValueOrDefault(),
                SchoolPhone = user.Person.School?.Phone.GetValueOrDefault(),
                DirectorFirstName = user.Person.School?.Director?.FirstName.GetValueOrDefault(),
                DirectorLastName = user.Person.School?.Director?.LastName.GetValueOrDefault(),
                DirectorMiddleName = user.Person.School?.Director?.MiddleName.GetValueOrDefault(),
                SchoolWebsite = user.Person.School?.Website.GetValueOrDefault()
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
            FirstNameStudent = user.Person.FirstName.GetValueOrDefault(),
            LastNameStudent = user.Person.LastName.GetValueOrDefault(),
            MiddleNameStudent = user.Person.MiddleName.GetValueOrDefault(),
            }
            };

            students.AddRange(classmates.Select(cs => new StudentDto
            {
                Id = cs.StudentId,
                FirstNameStudent = cs.Student.FirstName.GetValueOrDefault(),
                LastNameStudent = cs.Student.LastName.GetValueOrDefault(),
                MiddleNameStudent = cs.Student.MiddleName.GetValueOrDefault()
            }));

            return new ClassResponseDTO
            {
                ClassName = classStudent.Class.Name.GetValueOrDefault(),
                ClassTeacherName = classStudent.Class.ClassTeacher != null
                    ? $"{classStudent.Class.ClassTeacher.LastName} {classStudent.Class.ClassTeacher.FirstName} {classStudent.Class.ClassTeacher.MiddleName}"
                    : "Классный руководитель не назначен",
                AcademicYear = classStudent.Class.AcademicYear,
                Students = students
            };
        }
    }
}
