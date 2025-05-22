using Server.Models.DTO;
using Server.Utils;

namespace Server.Lessons
{
    public interface IGradeService
    {
        Task CreateLessonFromScheduleAsync(Schedule schedule);
        Task UpdateLessonFromScheduleAsync(Schedule schedule);
        Task<List<LessonViewDto>> GetStudentLessonsAsync(DateOnly? startDate = null, DateOnly? endDate = null);
        Task<List<LessonViewDto>> GetTeacherLessonsAsync(int? classId = null, int? subjectId = null,
            DateOnly? startDate = null, DateOnly? endDate = null);
        Task<OperationResult> UpdateGradesAsync(UpdateGradesDTO updateDto);
    }
}
