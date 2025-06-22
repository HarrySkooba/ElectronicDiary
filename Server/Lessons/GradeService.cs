using Microsoft.EntityFrameworkCore;
using Server.Context;
using Server.DatabaseModel;
using Server.Models.DTO;
using Server.Utils;

namespace Server.Lessons
{
    public class GradeService : IGradeService
    {
        private readonly ElectronicDiaryContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GradeService(ElectronicDiaryContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<List<LessonViewDto>> GetStudentLessonsAsync(DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user?.Person == null)
                throw new KeyNotFoundException("Ученик не найден");

            var classId = await _context.ClassStudents
                .Where(cs => cs.StudentId == user.Person.Id)
                .Select(cs => cs.ClassId)
                .FirstOrDefaultAsync();

            if (classId == 0)
                return new List<LessonViewDto>();

            var query = _context.Lessons
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Subject)
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Teacher)
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Class)
                .Include(l => l.Grades)
                    .ThenInclude(g => g.Student)
                .Where(l => l.Schedule.ClassId == classId);

            if (startDate.HasValue)
                query = query.Where(l => l.Date >= startDate.Value.ToDateTime(TimeOnly.MinValue));

            if (endDate.HasValue)
                query = query.Where(l => l.Date <= endDate.Value.ToDateTime(TimeOnly.MinValue));

            var lessons = await query
                .OrderBy(l => l.Date)
                .ThenBy(l => l.Schedule.LessonNumber)
                .ToListAsync();

            return lessons.Select(l => new LessonViewDto
            {
                Id = l.Id,
                Date = DateOnly.FromDateTime(l.Date),
                Subject = l.Schedule.Subject.Name,
                Teacher = FormatTeacherName(l.Schedule.Teacher),
                ClassName = l.Schedule.Class.Name,
                Homework = l.Homework,
                Grades = l.Grades.Select(g => new GradeViewDto
                {
                    StudentId = g.StudentId,
                    StudentName = FormatStudentName(g.Student),
                    Score = g.Score,
                    WasPresent = g.WasPresent,
                    Comment = g.Comment
                }).ToList()
            }).ToList();
        }

        public async Task<List<LessonViewDto>> GetTeacherLessonsAsync(int? classId = null, int? subjectId = null, DateOnly? startDate = null, DateOnly? endDate = null)
        {
            var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (string.IsNullOrEmpty(login))
                throw new UnauthorizedAccessException("Пользователь не авторизован");

            var user = await _context.Users
                .Include(u => u.Person)
                .FirstOrDefaultAsync(u => u.Login == login);

            if (user?.Person == null)
                throw new KeyNotFoundException("Учитель не найден");

            var query = _context.Lessons
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Subject)
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Teacher)
                .Include(l => l.Schedule)
                    .ThenInclude(s => s.Class)
                .Include(l => l.Grades)
                    .ThenInclude(g => g.Student)
                .Where(l => l.Schedule.TeacherId == user.Person.Id);

            if (classId.HasValue)
                query = query.Where(l => l.Schedule.ClassId == classId.Value);

            if (subjectId.HasValue)
                query = query.Where(l => l.Schedule.SubjectId == subjectId.Value);

            if (startDate.HasValue)
                query = query.Where(l => l.Date >= startDate.Value.ToDateTime(TimeOnly.MinValue));

            if (endDate.HasValue)
                query = query.Where(l => l.Date <= endDate.Value.ToDateTime(TimeOnly.MinValue));

            var lessons = await query
                .OrderBy(l => l.Date)
                .ThenBy(l => l.Schedule.LessonNumber)
                .ToListAsync();

            return lessons.Select(l => new LessonViewDto
            {
                Id = l.Id,
                Date = DateOnly.FromDateTime(l.Date),
                Subject = l.Schedule.Subject.Name,
                Teacher = FormatTeacherName(l.Schedule.Teacher),
                ClassName = l.Schedule.Class.Name,
                Homework = l.Homework,
                Grades = l.Grades.Select(g => new GradeViewDto
                {
                    StudentId = g.StudentId,
                    StudentName = FormatStudentName(g.Student),
                    Score = g.Score,
                    WasPresent = g.WasPresent,
                    Comment = g.Comment
                }).ToList()
            }).ToList();
        }

        public async Task CreateLessonFromScheduleAsync(Schedule schedule)
        {
            var existingLesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.ScheduleId == schedule.Id);

            if (existingLesson != null) return;

            var lesson = new Lesson
            {
                ScheduleId = schedule.Id,
                Date = schedule.DateTimetable.ToDateTime(TimeOnly.MinValue),
                Homework = string.Empty
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            var students = await _context.ClassStudents
                .Where(cs => cs.ClassId == schedule.ClassId)
                .Select(cs => cs.StudentId)
                .ToListAsync();

            foreach (var studentId in students)
            {
                _context.Grades.Add(new Grade
                {
                    LessonId = lesson.Id,
                    StudentId = studentId,
                    WasPresent = true, 
                    Score = null,      
                    Comment = string.Empty
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateLessonFromScheduleAsync(Schedule schedule)
        {
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.ScheduleId == schedule.Id);

            if (lesson == null)
            {
                await CreateLessonFromScheduleAsync(schedule);
                return;
            }

            lesson.Date = schedule.DateTimetable.ToDateTime(TimeOnly.MinValue);

            var existingStudentIds = await _context.Grades
                .Where(g => g.LessonId == lesson.Id)
                .Select(g => g.StudentId)
                .ToListAsync();

            var currentStudentIds = await _context.ClassStudents
                .Where(cs => cs.ClassId == schedule.ClassId)
                .Select(cs => cs.StudentId)
                .Distinct()
                .ToListAsync();

            var newStudentIds = currentStudentIds.Except(existingStudentIds).ToList();

            foreach (var studentId in newStudentIds)
            {
                _context.Grades.Add(new Grade
                {
                    LessonId = lesson.Id,
                    StudentId = studentId,
                    WasPresent = true,
                    Score = null
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<OperationResult> UpdateGradesAsync(UpdateGradesDTO updateDto)
        {
            try
            {
                var login = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                var teacher = await _context.Users
                    .Include(u => u.Person)
                    .FirstOrDefaultAsync(u => u.Login == login);

                if (teacher?.Person == null)
                    return OperationResult.Fail("Учитель не найден");

                var lesson = await _context.Lessons
                    .Include(l => l.Schedule)
                    .FirstOrDefaultAsync(l => l.Id == updateDto.LessonId &&
                                            l.Schedule.TeacherId == teacher.Person.Id);

                if (lesson == null)
                    return OperationResult.Fail("Урок не найден или у вас нет прав");

                if (!string.IsNullOrEmpty(updateDto.Homework))
                {
                    lesson.Homework = updateDto.Homework;
                    _context.Lessons.Update(lesson);
                }

                foreach (var gradeDto in updateDto.Grades)
                {
                    var studentExists = await _context.Persons
                        .AnyAsync(p => p.Id == gradeDto.StudentId);

                    if (!studentExists)
                        continue;

                    if (gradeDto.Score.HasValue && (gradeDto.Score < 1 || gradeDto.Score > 5))
                        continue;

                    var grade = await _context.Grades
                        .FirstOrDefaultAsync(g => g.LessonId == updateDto.LessonId &&
                                                g.StudentId == gradeDto.StudentId);

                    if (grade == null)
                    {
                        grade = new Grade
                        {
                            LessonId = updateDto.LessonId,
                            StudentId = gradeDto.StudentId,
                            Score = (short?)gradeDto.Score, 
                            WasPresent = gradeDto.WasPresent,
                            Comment = gradeDto.Comment
                        };
                        _context.Grades.Add(grade);
                    }
                    else
                    {
                        grade.Score = (short?)gradeDto.Score; 
                        grade.WasPresent = gradeDto.WasPresent;
                        grade.Comment = gradeDto.Comment;
                        _context.Grades.Update(grade);
                    }
                }

                await _context.SaveChangesAsync();
                return OperationResult.Success("Оценки успешно обновлены");
            }
            catch (Exception ex)
            {
                return OperationResult.Fail($"Ошибка при обновлении оценок: {ex.Message}");
            }
        }

        private string FormatTeacherName(Person teacher)
        {
            return $"{teacher.LastName} {teacher.FirstName[0]}." +
                   (!string.IsNullOrEmpty(teacher.MiddleName) ? $"{teacher.MiddleName[0]}." : "");
        }

        private string FormatStudentName(Person student)
        {
            return $"{student.LastName} {student.FirstName}" +
                   (!string.IsNullOrEmpty(student.MiddleName) ? $" {student.MiddleName}" : "");
        }
    }
}


