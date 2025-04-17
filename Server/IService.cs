using Server.Models.DatabaseModel;

namespace Server
{
    public interface IService
    {
        
        User GetUserByLogin(string username, string password);
        Role GetRoleByLogin(int idrole);
        List<User> GetAllUsers();
        List<Role> GetAllRoles();
        List<Person> GetAllPersons();
        List<ClassStudent> GetAllClassStudents();
        List<Class> GetAllClasses();
        List<Grade> GetAllGrades();
        List<Homework> GetAllHomeworks();
        List<Schedule> GetAllSchedules();
        List<School> GetAllSchools();   
        List<Subject> GetAllSubjects();
        List<SubjectTeacher> GetAllSubjectTeachers();
        
    }
}
