using Microsoft.AspNetCore.Mvc;
using Server.Models.Model;
using Server.Models.ModelDTO;

namespace Server.Controllers
{
    
    [Route("api/[controller]")]
    [ApiController]
    public class GetController : Controller
    {
        private readonly IService _Service;

        public GetController(IService userService)
        {
            _Service = userService;
        }

        [HttpGet("GetUsers")]
        public List<UserModel> GetAllUsers()
        {
            List<UserModel> userModels = new List<UserModel>();
            _Service.GetAllUsers().ForEach(user =>
            {
                userModels.Add(new UserModel(user));
            });
            return userModels;
        }
        [HttpGet("GetSubjectTeachers")]
        public List<SubjectTeacherModel> GetAllSubjectTeachers()
        {
            List<SubjectTeacherModel> subjectTeacherModels = new List<SubjectTeacherModel>();
            _Service.GetAllSubjectTeachers().ForEach(subjectTeacher =>
            {
                subjectTeacherModels.Add(new SubjectTeacherModel(subjectTeacher));
            });
            return subjectTeacherModels;
        }
        [HttpGet("GetSubjects")]
        public List<SubjectModel> GetAllSubjects()
        {
            List<SubjectModel> subjectModels = new List<SubjectModel>();
            _Service.GetAllSubjects().ForEach(subject =>
            {
                subjectModels.Add(new SubjectModel(subject));
            });
            return subjectModels;
        }
        [HttpGet("GetSchools")]
        public List<SchoolModel> GetAllSchools()
        {
            List<SchoolModel> schoolModels = new List<SchoolModel>();
            _Service.GetAllSchools().ForEach(school =>
            {
                schoolModels.Add(new SchoolModel(school));
            });
            return schoolModels;
        }
        [HttpGet("GetSchedules")]
        public List<ScheduleModel> GetAllSchedules()
        {
            List<ScheduleModel> scheduleModels = new List<ScheduleModel>();
            _Service.GetAllSchedules().ForEach(schedule =>
            {
                scheduleModels.Add(new ScheduleModel(schedule));
            });
            return scheduleModels;
        }
        [HttpGet("GetRoles")]
        public List<RoleModel> GetAllRoles() 
        {
            List<RoleModel> roleModels = new List<RoleModel>();
            _Service.GetAllRoles().ForEach(role =>
            {
                roleModels.Add(new RoleModel(role));
            });
            return roleModels;
        }
        [HttpGet("GetPersons")]
        public List<PersonModel> GetAllPersons()
        {
            List<PersonModel> personModels = new List<PersonModel>();
            _Service.GetAllPersons().ForEach(person =>
            {
                personModels.Add(new PersonModel(person));
            });
            return personModels;
        }
        [HttpGet("GetHomeworks")]
        public List<HomeworkModel> GetAllHomeworks()
        {
            List<HomeworkModel> homeworkModels = new List<HomeworkModel>();
            _Service.GetAllHomeworks().ForEach(homework =>
            {
                homeworkModels.Add(new HomeworkModel(homework));
            });
            return homeworkModels;
        }
        [HttpGet("GetGrades")]
        public List<GradeModel> GetAllGrades()
        {
            List<GradeModel> gradeModels = new List<GradeModel>();
            _Service.GetAllGrades().ForEach(grade =>
            {
                gradeModels.Add(new GradeModel(grade));
            });
            return gradeModels;
        }
        [HttpGet("GetClassStudents")]
        public List<ClassStudentModel> GetAllClassStudents()
        {
            List<ClassStudentModel> classStudentModels = new List<ClassStudentModel>();
            _Service.GetAllClassStudents().ForEach(classstudent =>
            {
                classStudentModels.Add(new ClassStudentModel(classstudent));
            });
            return classStudentModels;
        }
        [HttpGet("GetClasses")]
        public List<ClassModel> GetAllClasses()
        {
            List<ClassModel> classModels = new List<ClassModel>();
            _Service.GetAllClasses().ForEach(classes =>
            {
                classModels.Add(new ClassModel(classes));
            });
            return classModels;
        }
    }  
}
