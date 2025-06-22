using Server.Models.DTO;
using Server.Utils;

namespace Server.Timetable
{
    public interface ITimetableService
    {
        Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetStudentSchedule(DateOnly? weekStartDate = null);
        Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetTeacherSchedule(DateOnly? weekStartDate = null);
        Task<OperationResult> CreateOrUpdateSchedule(List<ScheduleDayDto> scheduleDays);
        Task<OperationResult> CopyScheduleWeekAsync(DateOnly sourceWeekStart, DateOnly targetWeekStart);
    }
}
