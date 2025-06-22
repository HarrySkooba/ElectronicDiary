using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Server.Attributes;
using Server.Models.DTO;
using Server.Profile;

namespace Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet("profile")]
        [AuthorizeRole("Teacher", "Student", "Parent", "Director")]
        public async Task<ActionResult<ProfileResponseDTO>> GetProfile()
        {
            var profile = await _profileService.GetProfileInfo();
            return Ok(profile);
        }

        [HttpGet("school")]
        [AuthorizeRole("Teacher", "Student", "Parent", "Director")]
        public async Task<ActionResult<SchoolResponseDTO>> GetSchool()
        {
            var school = await _profileService.GetSchoolInfo();
            return Ok(school);
        }

        [HttpGet("class")]
        [AuthorizeRole("Student", "Parent")]
        public async Task<ActionResult<ClassResponseDTO>> GetClass()
        {
            var classes = await _profileService.GetClassInfo();
            return Ok(classes);
        }
    }
}
