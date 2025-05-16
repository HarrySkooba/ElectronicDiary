using Server.Models.DTO;

namespace Server.Timetable
{
    public interface ITimetableService
    {
        Task<List<ScheduleDTO>> GetStudentSchedule();
        Task<List<ScheduleDTO>> GetTeacherSchedule();
        Task<ScheduleDTO> CreateOrUpdateSchedule(ScheduleEditDTO scheduleDto);
        Task<bool> ValidateScheduleLimits(int classId);
    }
}
