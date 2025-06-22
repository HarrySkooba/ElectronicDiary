using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Server.Context;
using Server.DatabaseModel;
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
        private readonly string _connectionString;

        public TimetableService(
        ElectronicDiaryContext context,
        IHttpContextAccessor httpContextAccessor,
        IGradeService gradeService,
        IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _gradeService = gradeService;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetStudentSchedule(DateOnly? weekStartDate = null)
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

            var classId = await _context.ClassStudents
                .Where(cs => cs.StudentId == user.PersonId)
                .Select(cs => cs.ClassId)
                .FirstOrDefaultAsync();

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

        public async Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetTeacherSchedule(DateOnly? weekStartDate = null)
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

            var schedules = await _context.Schedules
        .Where(s => s.TeacherId == user.Person.Id &&
                   (!weekStartDate.HasValue ||
                    (s.DateTimetable >= weekStartDate.Value &&
                     s.DateTimetable <= weekStartDate.Value.AddDays(6))))
        .Include(s => s.Subject)
        .Include(s => s.Class)
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
                    Date = s.DateTimetable,
                    IsCurrentDay = s.DateTimetable == DateOnly.FromDateTime(DateTime.Now)
                })
                .ToListAsync();

            return schedules
                .GroupBy(s => s.Date)
                .ToDictionary(g => g.Key, g => g.ToList());
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
                    foreach (var classGroup in day.Lessons.GroupBy(l => l.ClassName))
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
                        Console.WriteLine($"Lesson ID: {lessonDto.Id}, Date in DTO: {day.Date}");
                        var classEntity = await _context.Classes
                            .FirstOrDefaultAsync(c => c.Name == lessonDto.ClassName);
                        var subject = await _context.Subjects
                            .FirstOrDefaultAsync(s => s.Name == lessonDto.Subject);
                        var teacher = await GetTeacherAsync(lessonDto.Teacher);

                        if (teacher == null)
                            return OperationResult.Fail($"Учитель {lessonDto.Teacher} не найден");

                        var existingSchedule = lessonDto.Id > 0
                            ? await _context.Schedules.FirstOrDefaultAsync(s => s.Id == lessonDto.Id)
                            : await _context.Schedules.FirstOrDefaultAsync(s =>
                                s.ClassId == classEntity.Id &&
                                s.DateTimetable == day.Date &&
                                s.LessonNumber == lessonDto.Lesson_Number);

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

                return OperationResult.Success("Расписание успешно сохранено");
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

        public async Task<OperationResult> CopyScheduleWeekAsync(DateOnly sourceWeekStart, DateOnly targetWeekStart)
        {
            if (sourceWeekStart.DayOfWeek != DayOfWeek.Monday ||
                targetWeekStart.DayOfWeek != DayOfWeek.Monday)
            {
                return OperationResult.Fail("Обе даты должны быть понедельниками");
            }

            try
            {
                var sourceDates = Enumerable.Range(0, 5)
                    .Select(i => sourceWeekStart.AddDays(i))
                    .ToList();

                var existingSchedules = await _context.Schedules
                    .Where(s => sourceDates.Contains(s.DateTimetable))
                    .AsNoTracking()
                    .ToListAsync();

                if (!existingSchedules.Any())
                    return OperationResult.Fail("Не найдено расписания для копирования");

                var newSchedules = existingSchedules.Select(s => new Schedule
                {
                    ClassId = s.ClassId,
                    SubjectId = s.SubjectId,
                    TeacherId = s.TeacherId,
                    DayOfWeek = s.DayOfWeek,
                    LessonNumber = s.LessonNumber,
                    Room = s.Room,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    DateTimetable = targetWeekStart.AddDays((int)s.DateTimetable.DayOfWeek - 1)
                }).ToList();

                var validationErrors = await ValidateSchedules(newSchedules);
                if (validationErrors.Any())
                {
                    return OperationResult.Fail(
                        "Ошибки валидации:\n" + string.Join("\n", validationErrors));
                }

                await BulkInsertSchedulesAsync(newSchedules);

                foreach (var newSchedule in newSchedules)
                {
                    var createdSchedule = await _context.Schedules
                        .FirstOrDefaultAsync(s =>
                            s.ClassId == newSchedule.ClassId &&
                            s.SubjectId == newSchedule.SubjectId &&
                            s.TeacherId == newSchedule.TeacherId &&
                            s.DateTimetable == newSchedule.DateTimetable &&
                            s.LessonNumber == newSchedule.LessonNumber);

                    if (createdSchedule != null)
                    {
                        await _gradeService.CreateLessonFromScheduleAsync(createdSchedule);
                    }
                }

                return OperationResult.Success(
                    $"Успешно скопировано {newSchedules.Count} уроков");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Критическая ошибка: {ex.Message}");
            }
        }

        private async Task<List<string>> ValidateSchedules(List<Schedule> schedules)
        {
            var errors = new List<string>();
            var existingClasses = await _context.Classes
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var s in schedules)
            {
                if (!existingClasses.Contains(s.ClassId))
                    errors.Add($"Не найден класс ID: {s.ClassId}");

            }

            return errors;
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
                    if (newLessons[i].Date == newLessons[j].Date &&
                        newLessons[i].Start_Time < newLessons[j].End_Time &&
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
                if (orderedLessons[i].Date == orderedLessons[i + 1].Date &&
                    orderedLessons[i].End_Time > orderedLessons[i + 1].Start_Time)
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
                    if (existing.DateTimetable == newLesson.Date &&
                        existing.Id != newLesson.Id &&
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

        private async Task BulkInsertSchedulesAsync(List<Schedule> schedules)
        {
            const int batchSize = 50;
            var insertedCount = 0;

            while (insertedCount < schedules.Count)
            {
                var batch = schedules.Skip(insertedCount).Take(batchSize).ToList();

                try
                {
                    await _context.Database.BeginTransactionAsync();

                    foreach (var item in batch)
                    {
                        var exists = await _context.Schedules.AnyAsync(s =>
                            s.ClassId == item.ClassId &&
                            s.SubjectId == item.SubjectId &&
                            s.TeacherId == item.TeacherId &&
                            s.DateTimetable == item.DateTimetable &&
                            s.LessonNumber == item.LessonNumber);

                        if (!exists)
                        {
                            _context.Schedules.Add(item);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await _context.Database.CommitTransactionAsync();
                    insertedCount += batch.Count;
                }
                catch (Exception ex)
                {
                    await _context.Database.RollbackTransactionAsync();
                    throw new Exception($"Ошибка при вставке записей {insertedCount}-{insertedCount + batchSize}. " +
                                      $"Успешно вставлено: {insertedCount}. Ошибка: {ex.Message}");
                }
            }
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

        private static string FormatTeacherName(Person teacher)
        {
            return $"{teacher.LastName} {teacher.FirstName[0]}." +
                   (!string.IsNullOrEmpty(teacher.MiddleName) ? $"{teacher.MiddleName[0]}." : "");
        }
    }
}