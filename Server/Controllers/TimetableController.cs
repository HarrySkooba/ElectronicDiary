using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Attributes;
using Server.Context;
using Server.Models.DTO;
using Server.Timetable;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TimetableController : Controller
    {
        private readonly ITimetableService _timetableService;

        public TimetableController(ITimetableService timetableService, ElectronicDiaryContext electronicDiaryContext)
        {
            _timetableService = timetableService;
        }

        [HttpGet("schedule")]
        [AuthorizeRole("Student", "Parent", "Admin")]
        public async Task<IActionResult> GetStudentSchedule([FromQuery] string weekStart)
        {
            try
            {
                if (!DateOnly.TryParse(weekStart, out var weekStartDate))
                {
                    return BadRequest("Неверный формат даты. Используйте yyyy-MM-dd");
                }

                var schedule = await _timetableService.GetStudentSchedule(weekStartDate);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("schedule_teacher")]
        [AuthorizeRole("Director", "Teacher")]
        public async Task<IActionResult> GetTeacherSchedule([FromQuery] DateOnly? weekStart)
        {
            try
            {
                var schedule = await _timetableService.GetTeacherSchedule(weekStart);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("edit")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> UpdateSchedule([FromBody] List<ScheduleDayDto> schedule)
        {
            var result = await _timetableService.CreateOrUpdateSchedule(schedule);
            if (result.IsSuccess)
                return Ok(result.Message);

            return BadRequest(result.Message);
        }

        [HttpPost("copy-week")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> CopyScheduleWeek([FromBody] CopyScheduleWeekRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _timetableService.CopyScheduleWeekAsync(request.SourceWeekStart, request.TargetWeekStart);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("time-slots")]
        public IActionResult GetTimeSlots()
        {
            var timeSlots = new List<ClassTimeSlotDTO>();
            var currentTime = new TimeOnly(8, 0);

            for (int lessonNumber = 1; lessonNumber <= 7; lessonNumber++)
            {
                var endTime = currentTime.AddMinutes(45);

                TimeSpan breakDuration = lessonNumber switch
                {
                    1 => TimeSpan.FromMinutes(30),
                    _ => TimeSpan.FromMinutes(10)
                };

                timeSlots.Add(new ClassTimeSlotDTO
                {
                    LessonNumber = lessonNumber,
                    StartTime = currentTime,
                    EndTime = endTime,
                    BreakAfterDuration = lessonNumber < 7 ? breakDuration : null
                });

                currentTime = endTime.Add(breakDuration);
            }

            return Ok(timeSlots);
        }

        public class CopyScheduleWeekRequest
        {
            [Required]
            public DateOnly SourceWeekStart { get; set; }

            [Required]
            public DateOnly TargetWeekStart { get; set; }
        }
    }
}

