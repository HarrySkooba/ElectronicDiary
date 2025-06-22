using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.AdminPanel;
using Server.Attributes;
using Server.Models.DTO;

namespace Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : Controller
    {
        private IAdminPanelService _adminPanelService;

        public AdminController(IAdminPanelService adminPanelService)
        {
            _adminPanelService = adminPanelService;
        }

        [HttpGet("persons")]
        [AuthorizeRole("Admin")]
        public List<PersonAdminDTO> GetPersons()
        {
            List<PersonAdminDTO> people = new List<PersonAdminDTO>();
            _adminPanelService.GetPersons().ForEach(result =>
            {
                people.Add(new PersonAdminDTO(result));
            });
            return people;
        }

        [HttpGet("users")]
        [AuthorizeRole("Admin")]
        public List<UserAdminDTO> GetUsers()
        {
            List<UserAdminDTO> user = new List<UserAdminDTO>();
            _adminPanelService.GetUsers().ForEach(result =>
            {
                user.Add(new UserAdminDTO(result));
            });
            return user;
        }

        [HttpGet("school")]
        [AuthorizeRole("Admin")]
        public async Task<ActionResult<SchoolAdminDTO>> GetSchool()
        {
            try
            {
                var schoolDto = await _adminPanelService.GetSchool();
                return Ok(schoolDto);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("potential-directors")]
        [AuthorizeRole("Admin")]
        public List<DirectorsAdminDTO> GetPotentialDirectors()
        {
            List<DirectorsAdminDTO> director = new List<DirectorsAdminDTO>();
            _adminPanelService.GetPotentialDirectors().ForEach(result =>
            {
                director.Add(new DirectorsAdminDTO(result));
            });
            return director;
        }

        [HttpGet("roles")]
        [AuthorizeRole("Admin")]
        public List<RoleAdminDTO> GetRoles()
        {
            List<RoleAdminDTO> role = new List<RoleAdminDTO>();
            _adminPanelService.GetRoles().ForEach(result =>
            {
                role.Add(new RoleAdminDTO(result));
            });
            return role;
        }

        [HttpPost("adduser")]
        [AuthorizeRole("Admin")]
        public async Task<ActionResult> AddUser(AddUserDTO request)
        {
            try
            {
                await _adminPanelService.AddUser(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateuser/{userId}")]
        [AuthorizeRole("Admin")]
        public async Task<ActionResult> UpdateUser(AddUserDTO request, int userId)
        {
            try
            {
                await _adminPanelService.UpdateUser(request, userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("addperson")]
        [AuthorizeRole("Admin")]
        public async Task<ActionResult> AddPerson(AddPersonDTO request)
        {
            try
            {
                await _adminPanelService.AddPerson(request);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateperson/{personId}")]
        [AuthorizeRole("Admin")]
        public async Task<ActionResult> UpdatePerson(AddPersonDTO request, int personId)
        {
            try
            {
                await _adminPanelService.UpdatePerson(request, personId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateschool/{schoolId}")]
        [AuthorizeRole("Admin")]
        public async Task<ActionResult> UpdateSchool(UpdateSchoolDTO request, int schoolId)
        {
            try
            {
                await _adminPanelService.UpdateSchool(request, schoolId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("classes")]
        [AuthorizeRole("Admin")]
        public List<AllClassesAdminDTO> GetClasses()
        {
            List<AllClassesAdminDTO> classes = new List<AllClassesAdminDTO>();
            _adminPanelService.GetClasses().ForEach(result =>
            {
                classes.Add(new AllClassesAdminDTO(result));
            });
            return classes;
        }

        [HttpGet("students")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> GetStudents()
        {
            try
            {
                var students = await _adminPanelService.GetStudents();
                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("selected_class/{classId}")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> GetSelectedClasses(int classId)
        {
            try
            {
                var classes = await _adminPanelService.GetSelectedClass(classId);
                return Ok(classes);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("teachers")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> GetTeachers()
        {
            try
            {
                var teachers = await _adminPanelService.GetTeachers();
                return Ok(teachers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("students/{classId}")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> GetClassStudents(int classId)
        {
            try
            {
                var students = await _adminPanelService.GetAllStudentsInClass(classId);
                return Ok(students);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("updateclass/{classId}")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> UpdateClass(int classId, [FromBody] UpdateClassDto model)
        {
            try
            {
                await _adminPanelService.UpdateClass(model, classId);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("addclass")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> AddClass([FromBody] AddClassDto model)
        {
            try
            {
                var newClass = await _adminPanelService.AddClass(model);
                return Ok(newClass);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("add-students")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> AddStudents(int classId, [FromBody] List<int> personIds)
        {
            try
            {
                var (addedCount, invalidStudents, alreadyInClassStudents, inOtherClassesStudents) =
                    await _adminPanelService.AddStudentsToClass(classId, personIds);

                var response = new
                {
                    AddedCount = addedCount,
                    InvalidStudents = invalidStudents,
                    AlreadyInClassStudents = alreadyInClassStudents,
                    InOtherClassesStudents = inOtherClassesStudents,
                    Message = addedCount > 0
                        ? $"Успешно добавлено {addedCount} учеников"
                        : "Нет новых учеников для добавления"
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("уже привязаны к другим классам"))
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("remove-student")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> RemoveStudentFromClass(int classId, int studentId)
        {
            try
            {
                var result = await _adminPanelService.RemoveStudentFromClass(classId, studentId);

                if (!result)
                {
                    return NotFound(new { Error = "Ученик не найден в указанном классе" });
                }

                return Ok(new { Message = "Ученик успешно удален из класса" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpGet("subject-teachers")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> GetAllSubjectTeachers()
        {
            try
            {
                var subjectTeachers = await _adminPanelService.GetAllSubjectTeachers();
                return Ok(subjectTeachers);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("add-subject-teacher")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> AddSubjectTeacher([FromBody] AddSubjectTeacherDTO request)
        {
            try
            {
                await _adminPanelService.AddSubjectTeacher(request);
                return Ok(new { Message = "Связь учитель-предмет-класс успешно добавлена" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-subject-teacher/{subjectTeacherId}")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> UpdateSubjectTeacher(int subjectTeacherId, [FromBody] AddSubjectTeacherDTO request)
        {
            try
            {
                await _adminPanelService.UpdateSubjectTeacher(request, subjectTeacherId);
                return Ok(new { Message = "Связь учитель-предмет-класс успешно обновлена" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("delete-subject-teacher/{subjectTeacherId}")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> DeleteSubjectTeacher(int subjectTeacherId)
        {
            try
            {
                await _adminPanelService.DeleteSubjectTeacher(subjectTeacherId);
                return Ok(new { Message = "Связь учитель-предмет-класс успешно удалена" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("subjects")]
        [AuthorizeRole("Admin")]
        public List<SubjectAdminDTO> GetSybjects()
        {
            List<SubjectAdminDTO> subjects = new List<SubjectAdminDTO>();
            _adminPanelService.GetSubjects().ForEach(result =>
            {
                subjects.Add(new SubjectAdminDTO(result));
            });
            return subjects;
        }

        [HttpGet("schedule")]
        [AuthorizeRole("Admin")]
        public async Task<IActionResult> GetStudentSchedule([FromQuery] string weekStart, [FromQuery] int classId)
        {
            try
            {
                if (!DateOnly.TryParse(weekStart, out var weekStartDate))
                {
                    return BadRequest("Неверный формат даты. Используйте yyyy-MM-dd");
                }

                var schedule = await _adminPanelService.GetStudentSchedule(weekStartDate, classId);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
