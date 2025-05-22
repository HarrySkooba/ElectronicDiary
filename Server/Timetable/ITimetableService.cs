using Server.Models.DTO;
using Server.Utils;

namespace Server.Timetable
{
    public interface ITimetableService
    {
        Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetStudentSchedule();
        Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetTeacherSchedule();
        Task<OperationResult> CreateOrUpdateSchedule(List<ScheduleDayDto> scheduleDays);
    }
}
