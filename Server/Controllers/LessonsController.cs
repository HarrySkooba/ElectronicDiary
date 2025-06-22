using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Attributes;
using Server.DatabaseModel;
using Server.Lessons;
using Server.Models.DTO;

namespace Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : Controller
    {
        private readonly IGradeService _gradeService;
        private readonly ILogger<LessonsController> _logger;

        public LessonsController(IGradeService gradeService, ILogger<LessonsController> logger)
        {
            _gradeService = gradeService;
            _logger = logger;
        }

        [HttpPost("from-schedule")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> CreateFromSchedule([FromBody] Schedule schedule)
        {
            try
            {
                await _gradeService.CreateLessonFromScheduleAsync(schedule);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании занятия из расписания");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("from-schedule")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> UpdateFromSchedule([FromBody] Schedule schedule)
        {
            try
            {
                await _gradeService.UpdateLessonFromScheduleAsync(schedule);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении занятия из расписания");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPut("update-grades")]
        [AuthorizeRole("Teacher", "Director")]
        public async Task<IActionResult> UpdateGrades([FromBody] UpdateGradesDTO updateDto)
        {
            try
            {
                var result = await _gradeService.UpdateGradesAsync(updateDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении оценки для ученика");
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpGet("student")]
        [AuthorizeRole("Student", "Parent")]
        public async Task<IActionResult> GetForStudent(
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            try
            {
                var lessons = await _gradeService.GetStudentLessonsAsync(startDate, endDate);
                return Ok(lessons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении занятий для ученика");
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("teacher")]
        [AuthorizeRole("Teacher", "Director")]
        public async Task<IActionResult> GetForTeacher(
            [FromQuery] int? classId = null,
            [FromQuery] int? subjectId = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null)
        {
            try
            {
                var lessons = await _gradeService.GetTeacherLessonsAsync(classId, subjectId, startDate, endDate);
                return Ok(lessons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении занятий для учителя");
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
