using Microsoft.EntityFrameworkCore;
using Server.Lessons;
using Server.Models.DTO;
using Server.Utils;

namespace Server.Timetable
{
    public class TimetableService : ITimetableService
    {
        private readonly ElectronicDiaryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IGradeService _gradeService;

        public TimetableService(ElectronicDiaryContext context, IHttpContextAccessor httpContextAccessor, IGradeService gradeService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _gradeService = gradeService;
        }

        public async Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetStudentSchedule()
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
                return new Dictionary<DateOnly, List<ScheduleDTO>>();

            var className = await _context.Classes
                .Where(c => c.Id == classId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? "Неизвестный класс";

            var schedules = await _context.Schedules
                .Where(s => s.ClassId == classId)
                .Include(s => s.Subject)
                .Include(s => s.Teacher)
                .OrderBy(s => s.DateTimetable)
                .ThenBy(s => s.LessonNumber)
                .Select(s => new ScheduleDTO
                {
                    Id = s.Id,
                    Subject = s.Subject.Name,
                    Teacher = $"{s.Teacher.LastName} {s.Teacher.FirstName[0]}.{(!string.IsNullOrEmpty(s.Teacher.MiddleName) ? s.Teacher.MiddleName[0] + "." : "")}",
                    Day_Of_Week = s.DayOfWeek,
                    Lesson_Number = s.LessonNumber,
                    Room = s.Room ?? "Не указано",
                    Start_Time = s.StartTime,
                    End_Time = s.EndTime,
                    ClassName = className,
                    Date = s.DateTimetable
                })
                .ToListAsync();

            var groupedSchedules = schedules
                .GroupBy(s => s.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            return groupedSchedules;
        }

        public async Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetTeacherSchedule()
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                throw new KeyNotFoundException("Пользователь не найден");

            var schedules = await _context.Schedules
                .Where(s => s.TeacherId == user.PersonId)
                .Include(s => s.Subject)
                .Include(s => s.Class)
                .Include(s => s.Teacher)
                .OrderBy(s => s.DateTimetable)
                .ThenBy(s => s.LessonNumber)
                .Select(s => new ScheduleDTO
                {
                    Id = s.Id,
                    Subject = s.Subject.Name,
                    Teacher = $"{s.Teacher.LastName} {s.Teacher.FirstName[0]}.{(!string.IsNullOrEmpty(s.Teacher.MiddleName) ? s.Teacher.MiddleName[0] + "." : "")}",
                    Day_Of_Week = s.DayOfWeek,
                    Lesson_Number = s.LessonNumber,
                    Room = s.Room ?? "Не указано",
                    Start_Time = s.StartTime,
                    End_Time = s.EndTime,
                    ClassName = s.Class.Name,
                    Date = s.DateTimetable
                })
                .ToListAsync();

            var groupedSchedules = schedules
                .GroupBy(s => s.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );

            return groupedSchedules;
        }

        public async Task<OperationResult> CreateOrUpdateSchedule(List<ScheduleDayDto> scheduleDays)
        {
            try
            {
                var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                if (string.IsNullOrEmpty(login))
                    return OperationResult.Fail("Пользователь не авторизован");

                if (scheduleDays == null || !scheduleDays.Any())
                    return OperationResult.Fail("Не передано расписание");

                var validationResult = ValidateSchedule(scheduleDays);
                if (!validationResult.IsSuccess)
                    return validationResult;

                foreach (var day in scheduleDays)
                {
                    var classGroups = day.Lessons.GroupBy(l => l.ClassName);

                    foreach (var classGroup in classGroups)
                    {
                        var className = classGroup.Key;
                        var lessons = classGroup.ToList();
                        var classEntity = await _context.Classes
                            .FirstOrDefaultAsync(c => c.Name == className);

                        if (classEntity == null)
                            return OperationResult.Fail($"Класс {className} не найден");

                        foreach (var lesson in lessons)
                        {
                            if (lesson.Start_Time >= lesson.End_Time)
                                return OperationResult.Fail(
                                    $"Некорректное время урока ({lesson.Start_Time}-{lesson.End_Time}) для {lesson.Subject}");
                        }

                        var existingSchedules = await _context.Schedules
                            .Where(s => s.ClassId == classEntity.Id && s.DateTimetable == day.Date)
                            .ToListAsync();

                        var timeConflicts = CheckTimeConflicts(lessons, existingSchedules);
                        if (timeConflicts.Any())
                            return timeConflicts.First();
                    }
                }

                var newSchedules = new List<Schedule>();

                foreach (var day in scheduleDays)
                {
                    foreach (var lessonDto in day.Lessons)
                    {
                        var classEntity = await _context.Classes
                            .FirstOrDefaultAsync(c => c.Name == lessonDto.ClassName);
                        var subject = await _context.Subjects
                            .FirstOrDefaultAsync(s => s.Name == lessonDto.Subject);
                        var teacher = await GetTeacherAsync(lessonDto.Teacher);

                        if (teacher == null)
                            return OperationResult.Fail($"Учитель {lessonDto.Teacher} не найден");

                        var existingSchedule = await _context.Schedules
                            .FirstOrDefaultAsync(s => s.Id == lessonDto.Id && s.ClassId == classEntity.Id);

                        if (existingSchedule != null)
                        {
                            UpdateSchedule(existingSchedule, subject.Id, teacher.Id, day, lessonDto);

                            await _gradeService.UpdateLessonFromScheduleAsync(existingSchedule);
                        }
                        else
                        {
                            var newSchedule = CreateNewSchedule(classEntity.Id, subject.Id, teacher.Id, day, lessonDto);
                            _context.Schedules.Add(newSchedule);
                            newSchedules.Add(newSchedule);
                        }
                    }
                }

                await _context.SaveChangesAsync();

                foreach (var newSchedule in newSchedules)
                {
                    await _gradeService.CreateLessonFromScheduleAsync(newSchedule);
                }

                return OperationResult.Fail("Расписание успешно сохранено");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Ошибка при сохранении расписания: {ex.Message}");
            }
        }

        private async Task<Person> GetTeacherAsync(string teacherName)
        {
            var teacherNames = teacherName.Split(' ');
            return await _context.Persons
                .FirstOrDefaultAsync(p =>
                    p.LastName == teacherNames[0] &&
                    p.FirstName.StartsWith(teacherNames[1][0].ToString()));
        }

        private void UpdateSchedule(Schedule schedule, int subjectId, int teacherId, ScheduleDayDto day, ScheduleDTO lessonDto)
        {
            schedule.SubjectId = subjectId;
            schedule.TeacherId = teacherId;
            schedule.DayOfWeek = (short)day.DayOfWeek;
            schedule.LessonNumber = (short)lessonDto.Lesson_Number;
            schedule.Room = lessonDto.Room;
            schedule.StartTime = lessonDto.Start_Time;
            schedule.EndTime = lessonDto.End_Time;
            schedule.DateTimetable = day.Date;
        }

        private Schedule CreateNewSchedule(int classId, int subjectId, int teacherId, ScheduleDayDto day, ScheduleDTO lessonDto)
        {
            return new Schedule
            {
                ClassId = classId,
                SubjectId = subjectId,
                TeacherId = teacherId,
                DayOfWeek = (short)day.DayOfWeek,
                LessonNumber = (short)lessonDto.Lesson_Number,
                Room = lessonDto.Room,
                StartTime = lessonDto.Start_Time,
                EndTime = lessonDto.End_Time,
                DateTimetable = day.Date
            };
        }

        private List<OperationResult> CheckTimeConflicts(List<ScheduleDTO> newLessons, List<Schedule> existingSchedules)
        {
            var conflicts = new List<OperationResult>();

            for (int i = 0; i < newLessons.Count; i++)
            {
                for (int j = i + 1; j < newLessons.Count; j++)
                {
                    if (newLessons[i].Start_Time < newLessons[j].End_Time &&
                        newLessons[i].End_Time > newLessons[j].Start_Time)
                    {
                        conflicts.Add(OperationResult.Fail(
                            $"Внутреннее пересечение времени между уроками " +
                            $"{newLessons[i].Lesson_Number} и {newLessons[j].Lesson_Number}"));
                    }
                }
            }

            var orderedLessons = newLessons.OrderBy(l => l.Lesson_Number).ToList();
            for (int i = 0; i < orderedLessons.Count - 1; i++)
            {
                if (orderedLessons[i].End_Time > orderedLessons[i + 1].Start_Time)
                {
                    conflicts.Add(OperationResult.Fail(
                        $"Урок {orderedLessons[i].Lesson_Number} ({orderedLessons[i].End_Time}) " +
                        $"не может заканчиваться позже начала урока {orderedLessons[i + 1].Lesson_Number} " +
                        $"({orderedLessons[i + 1].Start_Time})"));
                }
            }
            foreach (var newLesson in newLessons)
            {
                foreach (var existing in existingSchedules)
                {
                    if (existing.Id != newLesson.Id &&
                        newLesson.Start_Time < existing.EndTime &&
                        newLesson.End_Time > existing.StartTime)
                    {
                        conflicts.Add(OperationResult.Fail(
                            $"Найдено пересечение времени с существующим уроком"));
                    }
                }
            }

            return conflicts;
        }

        private OperationResult ValidateSchedule(List<ScheduleDayDto> scheduleDays)
            {
                foreach (var day in scheduleDays)
                {
                    if (day.Date == default)
                        return OperationResult.Fail("Не указана дата для одного из дней");

                    if (day.Lessons == null || !day.Lessons.Any())
                        return OperationResult.Fail($"Не указаны уроки для дня {day.Date}");

                    foreach (var lesson in day.Lessons)
                    {
                        if (string.IsNullOrEmpty(lesson.Subject))
                            return OperationResult.Fail($"Не указан предмет для урока в день {day.Date}");

                        if (string.IsNullOrEmpty(lesson.Teacher))
                            return OperationResult.Fail($"Не указан учитель для урока {lesson.Subject}");

                        if (string.IsNullOrEmpty(lesson.ClassName))
                            return OperationResult.Fail($"Не указан класс для урока {lesson.Subject}");

                        if (lesson.Lesson_Number < 1 || lesson.Lesson_Number > 7)
                            return OperationResult.Fail($"Некорректный номер урока ({lesson.Lesson_Number})");

                        if (lesson.Start_Time >= lesson.End_Time)
                            return OperationResult.Fail($"Некорректное время урока ({lesson.Start_Time}-{lesson.End_Time})");
                    }
                }
                return OperationResult.Success();
            }
    }
}