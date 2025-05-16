using Microsoft.EntityFrameworkCore;
using Server.Models.DTO;

namespace Server.Timetable
{
    public class TimetableService : ITimetableService
    {
        private readonly ElectronicDiaryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TimetableService(ElectronicDiaryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<ScheduleDTO>> GetStudentSchedule()
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            var classId = await _context.ClassStudents
                .Where(cs => cs.StudentId == user.PersonId)
                .Select(cs => cs.ClassId)
                .FirstOrDefaultAsync();

            if (classId == 0)
                return new List<ScheduleDTO>();

            return await _context.Schedules
                .Where(s => s.ClassId == classId)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.LessonNumber)
                .Select(s => new ScheduleDTO
                {
                    Subject = s.Subject.Name,
                    Teacher = $"{s.Teacher.LastName} {s.Teacher.FirstName[0]}.{(!string.IsNullOrEmpty(s.Teacher.MiddleName) ? s.Teacher.MiddleName[0] + "." : "")}",
                    Day_Of_Week = s.DayOfWeek,
                    Lesson_Number = s.LessonNumber,
                    Room = s.Room ?? "Не указано",
                    Start_Time = s.StartTime,
                    End_Time = s.EndTime
                })
                .ToListAsync();
        }

        public async Task<List<ScheduleDTO>> GetTeacherSchedule()
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            return await _context.Schedules
                .Where(s => s.TeacherId == user.PersonId)
                .Include(s => s.Subject)
                .Include(s => s.Class)
                .OrderBy(s => s.DayOfWeek)
                .ThenBy(s => s.LessonNumber)
                .Select(s => new ScheduleDTO
                {
                    Subject = s.Subject.Name,
                    Teacher = $"{s.Teacher.LastName} {s.Teacher.FirstName[0]}.{(!string.IsNullOrEmpty(s.Teacher.MiddleName) ? s.Teacher.MiddleName[0] + "." : "")}",
                    Day_Of_Week = s.DayOfWeek,
                    Lesson_Number = s.LessonNumber,
                    Room = s.Room ?? "Не указано",
                    Start_Time = s.StartTime,
                    End_Time = s.EndTime,
                    ClassName = s.Class.Name
                })
                .ToListAsync();
        }

        public async Task<ScheduleDTO> CreateOrUpdateSchedule(ScheduleEditDTO scheduleDto)
        {
            if (scheduleDto.DayOfWeek < 1 || scheduleDto.DayOfWeek > 7)
                throw new ArgumentException("День недели должен быть от 1 до 7");

            if (scheduleDto.LessonNumber < 1 || scheduleDto.LessonNumber > 7)
                throw new ArgumentException("Номер урока должен быть от 1 до 7");

            var classInfo = await _context.Classes
                .Include(c => c.School)
                .FirstOrDefaultAsync(c => c.Id == scheduleDto.ClassId);

            if (classInfo == null)
                throw new KeyNotFoundException("Класс не найден");

            var classesInSchool = await _context.Classes
                .CountAsync(c => c.SchoolId == classInfo.SchoolId);

            var classesWithSchedule = await _context.Schedules
                .Where(s => s.Class.SchoolId == classInfo.SchoolId)
                .Select(s => s.ClassId)
                .Distinct()
                .CountAsync();

            if (!await _context.Schedules.AnyAsync(s => s.ClassId == scheduleDto.ClassId))
            {
                if (classesWithSchedule >= classesInSchool)
                {
                    throw new InvalidOperationException(
                        $"Школа может создавать расписание только для своих {classesInSchool} классов");
                }
            }

            var lessonsInDay = await _context.Schedules
                .CountAsync(s => s.ClassId == scheduleDto.ClassId &&
                                s.DayOfWeek == scheduleDto.DayOfWeek);

            if (lessonsInDay >= 7)
            {
                throw new InvalidOperationException(
                    "Превышено максимальное количество уроков (7) для класса в этот день");
            }

            Schedule schedule;
            if (scheduleDto.Id.HasValue)
            {
                schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.Id == scheduleDto.Id.Value);
                if (schedule == null)
                    throw new KeyNotFoundException("Расписание не найдено");
            }
            else
            {
                schedule = new Schedule();
                _context.Schedules.Add(schedule);
            }

            schedule.ClassId = scheduleDto.ClassId;
            schedule.SubjectId = scheduleDto.SubjectId;
            schedule.TeacherId = scheduleDto.TeacherId;
            schedule.DayOfWeek = (short)scheduleDto.DayOfWeek;
            schedule.LessonNumber = (short)scheduleDto.LessonNumber;
            schedule.Room = scheduleDto.Room;
            schedule.StartTime = scheduleDto.StartTime;
            schedule.EndTime = scheduleDto.EndTime;

            await _context.SaveChangesAsync();

            return await MapScheduleToDto(schedule.Id);
        }

        private async Task<ScheduleDTO> MapScheduleToDto(int scheduleId)
        {
            return await _context.Schedules
                .Where(s => s.Id == scheduleId)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .Include(s => s.Class)
                .Select(s => new ScheduleDTO
                {
                    Id = s.Id,
                    Subject = s.Subject.Name,
                    Teacher = $"{s.Teacher.LastName} {s.Teacher.FirstName[0]}." +
                             $"{(!string.IsNullOrEmpty(s.Teacher.MiddleName) ? s.Teacher.MiddleName[0] + "." : "")}",
                    Day_Of_Week = s.DayOfWeek,
                    Lesson_Number = s.LessonNumber,
                    Room = s.Room ?? "Не указано",
                    Start_Time = s.StartTime,
                    End_Time = s.EndTime,
                    ClassName = s.Class.Name
                })
                .FirstAsync();
        }

        public async Task<bool> ValidateScheduleLimits(int classId)
        {
            var hasInvalidDays = await _context.Schedules
                .Where(s => s.ClassId == classId)
                .GroupBy(s => s.DayOfWeek)
                .AnyAsync(g => g.Count() > 7);

            return !hasInvalidDays;
        }

    }
}
