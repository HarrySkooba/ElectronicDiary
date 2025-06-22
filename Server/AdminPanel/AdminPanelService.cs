using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Server.Context;
using Server.DatabaseModel;
using Server.Models.DTO;

namespace Server.AdminPanel
{
    public class AdminPanelService : IAdminPanelService
    {
        private readonly ElectronicDiaryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AdminPanelService> _logger;

        public AdminPanelService(ElectronicDiaryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public List<Person> GetPersons()
        {
            return _context.Persons.Include(s => s.School).ToList();
        }

        public List<User> GetUsers()
        {
            return _context.Users.Include(x => x.Person).Include(a => a.Role).ToList();
        }

        public async Task<SchoolAdminDTO> GetSchool()
        {
            var school = await _context.Schools.Include(x => x.Director).FirstOrDefaultAsync();
            if (school == null)
                throw new Exception("Школа не найдена в базе данных");
            return new SchoolAdminDTO
            {
                Id = school.Id,
                Name = school.Name,
                Address = school.Address,
                Phone = school.Phone,
                IdDirector = school.DirectorId,
                Website = school.Website
            };
        }

        public List<Person> GetPotentialDirectors()
        {
            return _context.Users
                .Include(u => u.Person)
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "Director")
                .Select(u => u.Person)
                .ToList();
        }

        public List<Role> GetRoles()
        {
            return _context.Roles.ToList();
        }

        public async Task AddUser(AddUserDTO request)
        {
            if (await _context.Users.AnyAsync(u => u.Login == request.Login))
                throw new Exception("Пользователь с таким логином уже существует");

            if (await _context.Users.AnyAsync(u => u.PersonId == request.PersonId))
                throw new Exception("Эта персона уже привязана к другому пользователю");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);

            if (role?.Name == "Admin")
            {
                if (await _context.Users
                    .Include(u => u.Role)
                    .AnyAsync(u => u.Role.Name == "Admin"))
                {
                    throw new Exception("В системе уже есть администратор. Нельзя создать более одного администратора.");
                }
            }

            CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);

            var user = new User
            {
                Login = request.Login,
                PasswordHash = hash,
                RoleId = request.RoleId,
                PersonId = request.PersonId,
                IsActive = false,
                Salt = salt
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUser(AddUserDTO request, int userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Пользователь не найден");

            if (user.Login != request.Login &&
                await _context.Users.AnyAsync(u => u.Login == request.Login))
            {
                throw new Exception("Пользователь с таким логином уже существует");
            }

            if (user.PersonId != request.PersonId &&
                await _context.Users.AnyAsync(u => u.PersonId == request.PersonId))
            {
                throw new Exception("Эта персона уже привязана к другому пользователю");
            }

            var newRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);

            if (newRole?.Name == "Admin")
            {
                var existingAdmin = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Role.Name == "Admin" && u.Id != userId);

                if (existingAdmin != null)
                {
                    throw new Exception("В системе уже есть администратор. Нельзя создать более одного администратора.");
                }
            }

            if (!string.IsNullOrEmpty(request.Password))
            {
                CreatePasswordHash(request.Password, out byte[] hash, out byte[] salt);
                user.PasswordHash = hash;
                user.Salt = salt;
            }

            user.Login = request.Login;
            user.RoleId = request.RoleId;
            user.PersonId = request.PersonId;
            user.IsActive = false;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public async Task AddPerson(AddPersonDTO request)
        {
            var person = new Person
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                BirthDate = request.BirthDate,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                PhotoUrl = request.PhotoUrl,
                Schoolid = request.IdSchool
            };

            await _context.Persons.AddAsync(person);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePerson(AddPersonDTO request, int personId)
        {
            var person = await _context.Persons
                .FirstOrDefaultAsync(u => u.Id == personId);

            if (person == null)
                throw new Exception("Персона не найден");

            person.FirstName = request.FirstName;
            person.LastName = request.LastName;
            person.MiddleName = request.MiddleName;
            person.BirthDate = request.BirthDate;
            person.Email = request.Email;
            person.Phone = request.Phone;
            person.Address = request.Address;
            person.PhotoUrl = request.PhotoUrl;
            person.Schoolid = request.IdSchool;
            _context.Persons.Update(person);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSchool(UpdateSchoolDTO request, int schoolId)
        {
            var school = await _context.Schools.FirstOrDefaultAsync(x => x.Id == schoolId);

            if (school == null)
                throw new Exception("Школа не найден");

            school.Name = request.Name;
            school.Address = request.Address;
            school.Phone = request.Phone;
            school.DirectorId = request.IdDirector;
            school.Website = request.Website;
            _context.Schools.Update(school);
            await _context.SaveChangesAsync();
        }

        public List<Class> GetClasses()
        {
            return _context.Classes.ToList();
        }

        public async Task<List<StudentAdminDTO>> GetStudents()
        {
            return await _context.Users
               .Include(u => u.Person)
               .Include(u => u.Role)
               .Where(u => u.Role.Name == "Student")
               .Select(u => new StudentAdminDTO(u.Person))
               .ToListAsync();
        }

        public async Task<ClassesAdminDTO> GetSelectedClass(int classId)
        {
            var classInfo = await _context.Classes
                .Include(c => c.ClassTeacher)
                .Where(c => c.Id == classId)
                .Select(c => new ClassesAdminDTO(c))
                .FirstOrDefaultAsync();

            return classInfo;
        }

        public async Task<List<TeacherAdminDTO>> GetTeachers()
        {
            return await _context.Users
                .Include(u => u.Person)
                .Include(u => u.Role)
                .Where(u => u.Role.Name == "Teacher")
                .Select(u => new TeacherAdminDTO(u.Person))
                .ToListAsync();
        }

        public async Task<List<StudentAdminDTO>> GetAllStudentsInClass(int classId)
        {
            var students = await _context.ClassStudents
                .Where(cs => cs.ClassId == classId)
                .Include(cs => cs.Student)
                .Select(cs => new StudentAdminDTO(cs.Student))
                .ToListAsync();

            return students;
        }

        public async Task UpdateClass(UpdateClassDto model, int classId)
        {
            var classEntity = await _context.Classes
              .Include(c => c.ClassTeacher)
              .FirstOrDefaultAsync(c => c.Id == classId);

            if (classEntity == null)
                throw new ArgumentException("Класс не найден");

            if (model.ClassTeacherId.HasValue && model.ClassTeacherId != classEntity.ClassTeacherId)
            {
                var isTeacherAssigned = await _context.Classes
                    .AnyAsync(c => c.ClassTeacherId == model.ClassTeacherId && c.Id != classId);

                if (isTeacherAssigned)
                {
                    var teacher = await _context.Users
                        .Include(u => u.Person)
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Person.Id == model.ClassTeacherId);

                    var teacherName = teacher != null
                        ? $"{teacher.Person.LastName} {teacher.Person.FirstName} {teacher.Person.MiddleName}"
                        : "Неизвестный учитель";

                    throw new InvalidOperationException(
                        $"Учитель {teacherName} уже является классным руководителем другого класса");
                }

                bool isTeacher = await _context.Users
          .Include(u => u.Person)
          .Include(u => u.Role)
          .AnyAsync(u => u.Person.Id == model.ClassTeacherId && u.Role.Name == "Teacher");

                if (!isTeacher)
                    throw new ArgumentException("Указанный пользователь не является учителем");
            }

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException("Название класса не может быть пустым");
            }

            if (model.AcademicYear < 2000 || model.AcademicYear > DateTime.Now.Year + 5)
            {
                throw new ArgumentException("Некорректный учебный год");
            }

            var classNameExists = await _context.Classes
                .AnyAsync(c => c.Name == model.Name &&
                              c.AcademicYear == model.AcademicYear &&
                              c.Id != classId);

            if (classNameExists)
            {
                throw new InvalidOperationException(
                    $"Класс с названием '{model.Name}' уже существует в {model.AcademicYear} учебном году");
            }

            classEntity.Name = model.Name;
            classEntity.AcademicYear = model.AcademicYear;
            classEntity.ClassTeacherId = model.ClassTeacherId;

            await _context.SaveChangesAsync();
        }

        public async Task<ClassesAdminDTO> AddClass(AddClassDto model)
        {
            if (model.ClassTeacherId.HasValue)
            {
                bool isTeacherAlreadyAssigned = await _context.Classes
                    .AnyAsync(c => c.ClassTeacherId == model.ClassTeacherId);

                if (isTeacherAlreadyAssigned)
                {
                    throw new InvalidOperationException("Этот учитель уже является классным руководителем другого класса");
                }

                bool isTeacher = await _context.Users
             .Include(u => u.Person)
             .Include(u => u.Role)
             .AnyAsync(u => u.Person.Id == model.ClassTeacherId && u.Role.Name == "Teacher");

                if (!isTeacher)
                {
                    throw new InvalidOperationException("Указанный пользователь не является учителем");
                }
            }
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ArgumentException("Название класса не может быть пустым");
            }
            bool classNameExists = await _context.Classes
    .AnyAsync(c => c.Name == model.Name && c.AcademicYear == model.AcademicYear);

            if (classNameExists)
            {
                throw new InvalidOperationException("Класс с таким названием уже существует в этом учебном году");
            }
            if (model.AcademicYear < 2000 || model.AcademicYear > DateTime.Now.Year + 5)
            {
                throw new ArgumentException("Некорректный учебный год");
            }

            var newClass = new Class
            {
                Name = model.Name,
                AcademicYear = model.AcademicYear,
                ClassTeacherId = model.ClassTeacherId
            };

            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();

            await _context.Entry(newClass)
                .Reference(c => c.ClassTeacher)
                .LoadAsync();

            return new ClassesAdminDTO(newClass);
        }

        public async Task<(int addedCount, List<string> invalidStudents, List<string> alreadyInClassStudents, List<string> inOtherClassesStudents)> AddStudentsToClass(int classId, List<int> personIds)
        {
            if (!await _context.Classes.AnyAsync(c => c.Id == classId))
                throw new ArgumentException("Класс не найден", nameof(classId));

            var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student")
                ?? throw new Exception("Роль 'Student' не найдена в системе");

            var requestedPersons = await _context.Persons
                .Where(p => personIds.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.FirstName,
                    p.LastName,
                    HasStudentRole = p.Users.Any(u => u.RoleId == studentRole.Id)
                })
                .ToListAsync();

            var validStudents = requestedPersons
                .Where(x => x.HasStudentRole)
                .ToList();

            var invalidStudents = requestedPersons
                .Where(x => !x.HasStudentRole)
                .Select(x => $"{x.FirstName} {x.LastName}")
                .Concat(personIds.Except(requestedPersons.Select(x => x.Id))
                    .Select(id => $"ID {id} (не найден)"))
                .ToList();

            var existingInClassIds = await _context.ClassStudents
                .Where(cs => cs.ClassId == classId && validStudents.Select(s => s.Id).Contains(cs.StudentId))
                .Select(cs => cs.StudentId)
                .ToListAsync();

            var alreadyInClassStudents = validStudents
                .Where(s => existingInClassIds.Contains(s.Id))
                .Select(s => $"{s.FirstName} {s.LastName}")
                .ToList();

            var inOtherClassesIds = await _context.ClassStudents
                .Where(cs => validStudents.Select(s => s.Id).Contains(cs.StudentId) && cs.ClassId != classId)
                .Select(cs => cs.StudentId)
                .Distinct()
                .ToListAsync();

            var inOtherClassesStudents = validStudents
                .Where(s => inOtherClassesIds.Contains(s.Id))
                .Select(s => $"{s.FirstName} {s.LastName}")
                .ToList();

            if (inOtherClassesStudents.Any())
            {
                throw new InvalidOperationException(
                    $"Некоторые ученики уже привязаны к другим классам: {string.Join(", ", inOtherClassesStudents)}");
            }

            var studentsToAddIds = validStudents
                .Select(s => s.Id)
                .Except(existingInClassIds)
                .ToList();

            if (studentsToAddIds.Any())
            {
                await _context.ClassStudents.AddRangeAsync(
                    studentsToAddIds.Select(id => new ClassStudent { ClassId = classId, StudentId = id })
                );
                await _context.SaveChangesAsync();
            }

            return (studentsToAddIds.Count, invalidStudents, alreadyInClassStudents, inOtherClassesStudents);
        }

        public async Task<bool> RemoveStudentFromClass(int classId, int studentId)
        {
            var classStudent = await _context.ClassStudents
                .FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.StudentId == studentId);

            if (classStudent == null)
            {
                return false; 
            }

            _context.ClassStudents.Remove(classStudent);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<List<SubjectTeacherDTO>> GetAllSubjectTeachers()
        {
            return await _context.SubjectTeachers
                .Include(st => st.Subject)
                .Include(st => st.Teacher)
                .Include(st => st.Class)
                .Select(st => new SubjectTeacherDTO
                {
                    Id = st.Id,
                    SubjectId = st.SubjectId,
                    SubjectName = st.Subject.Name,
                    TeacherId = st.TeacherId,
                    TeacherName = $"{st.Teacher.LastName} {st.Teacher.FirstName} {st.Teacher.MiddleName}",
                    ClassId = st.ClassId,
                    ClassName = st.Class.Name,
                    AcademicYear = st.AcademicYear
                })
                .ToListAsync();
        }

        public async Task AddSubjectTeacher(AddSubjectTeacherDTO request)
        {
            var teacher = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Person.Id == request.TeacherId && u.Role.Name == "Teacher");

            if (teacher == null)
                throw new Exception("Указанный учитель не найден или не имеет роли Teacher");

            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == request.SubjectId);
            if (!subjectExists)
                throw new Exception("Указанный предмет не найден");

            var classExists = await _context.Classes.AnyAsync(c => c.Id == request.ClassId);
            if (!classExists)
                throw new Exception("Указанный класс не найден");

            if (request.AcademicYear < 2000 || request.AcademicYear > DateTime.Now.Year + 5)
                throw new Exception("Некорректный учебный год");

            var exists = await _context.SubjectTeachers
                .AnyAsync(st => st.SubjectId == request.SubjectId &&
                               st.TeacherId == request.TeacherId &&
                               st.ClassId == request.ClassId &&
                               st.AcademicYear == request.AcademicYear);

            if (exists)
                throw new Exception("Эта связь учитель-предмет-класс уже существует для указанного учебного года");

            var subjectTeacher = new SubjectTeacher
            {
                SubjectId = request.SubjectId,
                TeacherId = request.TeacherId,
                ClassId = request.ClassId,
                AcademicYear = request.AcademicYear
            };

            await _context.SubjectTeachers.AddAsync(subjectTeacher);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubjectTeacher(AddSubjectTeacherDTO request, int subjectTeacherId)
        {
            var subjectTeacher = await _context.SubjectTeachers
                .FirstOrDefaultAsync(st => st.Id == subjectTeacherId);

            if (subjectTeacher == null)
                throw new Exception("Связь учитель-предмет-класс не найдена");

            var teacher = await _context.Users
                .Include(u => u.Person)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Person.Id == request.TeacherId && u.Role.Name == "Teacher");

            if (teacher == null)
                throw new Exception("Указанный учитель не найден или не имеет роли Teacher");

            var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == request.SubjectId);
            if (!subjectExists)
                throw new Exception("Указанный предмет не найден");

            var classExists = await _context.Classes.AnyAsync(c => c.Id == request.ClassId);
            if (!classExists)
                throw new Exception("Указанный класс не найден");

            if (request.AcademicYear < 2000 || request.AcademicYear > DateTime.Now.Year + 5)
                throw new Exception("Некорректный учебный год");

            var exists = await _context.SubjectTeachers
                .AnyAsync(st => st.SubjectId == request.SubjectId &&
                               st.TeacherId == request.TeacherId &&
                               st.ClassId == request.ClassId &&
                               st.AcademicYear == request.AcademicYear &&
                               st.Id != subjectTeacherId);

            if (exists)
                throw new Exception("Эта связь учитель-предмет-класс уже существует для указанного учебного года");

            subjectTeacher.SubjectId = request.SubjectId;
            subjectTeacher.TeacherId = request.TeacherId;
            subjectTeacher.ClassId = request.ClassId;
            subjectTeacher.AcademicYear = request.AcademicYear;

            _context.SubjectTeachers.Update(subjectTeacher);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteSubjectTeacher(int subjectTeacherId)
        {
            var subjectTeacher = await _context.SubjectTeachers
                .FirstOrDefaultAsync(st => st.Id == subjectTeacherId);

            if (subjectTeacher == null)
                throw new Exception("Связь учитель-предмет-класс не найдена");

            _context.SubjectTeachers.Remove(subjectTeacher);
            await _context.SaveChangesAsync();
        }

        public List<Subject> GetSubjects()
        {
            return _context.Subjects.ToList();
        }

        public async Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetStudentSchedule(DateOnly? weekStartDate = null, int classId = 0)
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var (startDate, endDate) = GetWeekDates(weekStartDate);

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            if (classId == 0)
                return new Dictionary<DateOnly, List<ScheduleDTO>>();

            var schedules = await _context.Schedules
        .Where(s => s.ClassId == classId &&
                   (!weekStartDate.HasValue ||
                    (s.DateTimetable >= weekStartDate.Value &&
                     s.DateTimetable <= weekStartDate.Value.AddDays(6))))
        .Include(s => s.Subject)
        .Include(s => s.Teacher)
        .OrderBy(s => s.DateTimetable)
        .ThenBy(s => s.LessonNumber)
        .Select(s => new ScheduleDTO
        {
            Id = s.Id,
            Subject = s.Subject.Name,
            Teacher = s.Teacher.LastName + " " + s.Teacher.FirstName[0] + "." +
                     (string.IsNullOrEmpty(s.Teacher.MiddleName) ? "" : s.Teacher.MiddleName[0] + "."),
            Day_Of_Week = s.DayOfWeek,
            Lesson_Number = s.LessonNumber,
            Room = s.Room ?? "Не указано",
            Start_Time = s.StartTime,
            End_Time = s.EndTime,
            ClassName = s.Class.Name,
            Date = s.DateTimetable
        })
        .ToListAsync();

            return schedules
                .GroupBy(s => s.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        private (DateOnly startDate, DateOnly endDate) GetWeekDates(DateOnly? weekStartDate)
        {
            var startDate = weekStartDate ?? GetCurrentWeekStart();
            var endDate = startDate.AddDays(6);
            return (startDate, endDate);
        }

        private DateOnly GetCurrentWeekStart()
        {
            var today = DateTime.Today;
            var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
            return DateOnly.FromDateTime(today.AddDays(-1 * diff).Date);
        }
    }
}
