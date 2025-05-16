using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Attributes;
using Server.Models.DTO;
using Server.Profile;
using Server.Timetable;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimetableController : Controller
    {
        private readonly ITimetableService _timetableService;
        private readonly ElectronicDiaryContext _context;

        public TimetableController(ITimetableService timetableService, ElectronicDiaryContext electronicDiaryContext)
        {
            _timetableService = timetableService;
            _context = electronicDiaryContext;
        }

        [HttpGet("schedule")]
        [AuthorizeRole("Student", "Parent")]
        public async Task<ActionResult<ScheduleDTO>> GetScheduleStudent()
        {
            var schedule = await _timetableService.GetStudentSchedule();
            return Ok(schedule);
        }

        [HttpGet("schedule_teacher")]
        [AuthorizeRole("Director", "Teacher", "Admin")]
        public async Task<ActionResult<ScheduleDTO>> GetScheduleTeacher()
        {
            var schedule = await _timetableService.GetTeacherSchedule();
            return Ok(schedule);
        }

        [HttpPost("edit")]
        [AuthorizeRole("Admin", "Director")]
        public async Task<IActionResult> CreateOrUpdateSchedule([FromBody] ScheduleEditDTO scheduleDto)
        {
            try
            {
                var timeConflict = await _context.Schedules
                    .AnyAsync(s => s.ClassId == scheduleDto.ClassId &&
                                  s.DayOfWeek == scheduleDto.DayOfWeek &&
                                  s.Id != scheduleDto.Id &&
                                  ((s.StartTime < scheduleDto.EndTime && s.EndTime > scheduleDto.StartTime)));

                if (timeConflict)
                    return BadRequest("Время урока пересекается с существующим расписанием");

                var result = await _timetableService.CreateOrUpdateSchedule(scheduleDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return ex switch
                {
                    ArgumentException => BadRequest(ex.Message),
                    KeyNotFoundException => NotFound(ex.Message),
                    InvalidOperationException => BadRequest(ex.Message),
                    _ => StatusCode(500, "Ошибка сервера")
                };
            }
        }
    }
}
