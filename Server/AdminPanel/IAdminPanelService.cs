using Server.DatabaseModel;
using Server.Models.DTO;
using static Server.AdminPanel.AdminPanelService;

namespace Server.AdminPanel
{
    public interface IAdminPanelService
    {
        List<Person> GetPersons();
        List<User> GetUsers();
        Task<SchoolAdminDTO> GetSchool();
        List<Person> GetPotentialDirectors();
        List<Role> GetRoles();
        Task AddUser(AddUserDTO request);
        Task UpdateUser(AddUserDTO request, int userId);
        Task AddPerson(AddPersonDTO request);
        Task UpdatePerson(AddPersonDTO request, int personId);
        Task UpdateSchool(UpdateSchoolDTO request, int schoolId);
        List<Class> GetClasses();
        Task<List<StudentAdminDTO>> GetStudents();
        Task<ClassesAdminDTO> GetSelectedClass(int classId);
        Task<List<TeacherAdminDTO>> GetTeachers();
        Task<List<StudentAdminDTO>> GetAllStudentsInClass(int classId);
        Task UpdateClass(UpdateClassDto model, int classId);
        Task<ClassesAdminDTO> AddClass(AddClassDto model);
        Task<(int addedCount, List<string> invalidStudents, List<string> alreadyInClassStudents, List<string> inOtherClassesStudents)> AddStudentsToClass(int classId, List<int> personIds);
        Task<bool> RemoveStudentFromClass(int classId, int studentId);
        Task<List<SubjectTeacherDTO>> GetAllSubjectTeachers();
        Task AddSubjectTeacher(AddSubjectTeacherDTO request);
        Task UpdateSubjectTeacher(AddSubjectTeacherDTO request, int subjectTeacherId);
        Task DeleteSubjectTeacher(int subjectTeacherId);
        List<Subject> GetSubjects();
        Task<Dictionary<DateOnly, List<ScheduleDTO>>> GetStudentSchedule(DateOnly? weekStartDate = null, int classId = 0);
    }
}
