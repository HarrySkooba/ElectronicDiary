using Microsoft.EntityFrameworkCore;
using Server.Models.Context;
using Server.Models.DatabaseModel;

namespace Server
{
    public class Service : IService
    {
        
        private readonly ElectronicDiaryContext _context;
        public Service(ElectronicDiaryContext context) 
        { 
            _context = context;
        }

        public User GetUserByLogin(string username, string password) => _context.Users.FirstOrDefault(u => u.Login == username && u.PasswordHash == password);
        public Role GetRoleByLogin(int idrole) => _context.Roles.FirstOrDefault(r => r.Id == idrole);
        public List<User> GetAllUsers()
        {
            return _context.Users.Include(u => u.Role).Include(x => x.Person).ToList();
        }
        public List<Role> GetAllRoles()
        {
            return _context.Roles.ToList();
        }
        public List<Person> GetAllPersons()
        {
            return _context.Persons.ToList();
        }
        public List<ClassStudent> GetAllClassStudents()
        {
            return _context.ClassStudents.Include(z => z.Class).Include(x => x.Student).ToList();
        }
        public List<Class> GetAllClasses()
        {
            return _context.Classes.Include(z => z.School).Include(x => x.ClassTeacher).ToList();
        }
        public List<Grade> GetAllGrades()
        {
            return _context.Grades.Include(z => z.Student).Include(x => x.Subject).Include(x => x.Teacher).ToList();
        }
        public List<Homework> GetAllHomeworks()
        {
            return _context.Homeworks.Include(z => z.Subject).Include(x => x.Class).Include(x => x.Teacher).ToList();
        }
        public List<Schedule> GetAllSchedules()
        {
            return _context.Schedules.Include(z => z.Subject).Include(x => x.Class).Include(x => x.Teacher).ToList();
        }
        public List<School> GetAllSchools()
        {
            return _context.Schools.Include(s => s.Director).ToList();
        }
        public List<Subject> GetAllSubjects()
        {
            return _context.Subjects.ToList();
        }
        public List<SubjectTeacher> GetAllSubjectTeachers()
        {
            return _context.SubjectTeachers.Include(z => z.Subject).Include(x => x.Class).Include(x => x.Teacher).ToList();
        }
        
    }
}
