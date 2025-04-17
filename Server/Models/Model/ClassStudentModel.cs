using Server.Models.DatabaseModel;

namespace Server.Models.Model
{
    public class ClassStudentModel
    {
        public int Idclassstudent { get; set; }

        public int ClassId { get; set; }

        public int StudentId { get; set; }

        public DateOnly EnrollmentDate { get; set; }

        public DateOnly? LeaveDate { get; set; }

        public ClassStudentModel(ClassStudent classStudent) 
        { 
            if (classStudent != null)
            {
                Idclassstudent = classStudent.Id;
                ClassId = classStudent.ClassId;
                StudentId = classStudent.StudentId;
                EnrollmentDate = classStudent.EnrollmentDate;
                LeaveDate = classStudent.LeaveDate;
            }
            else
            {
                Idclassstudent = -1;
                ClassId = -1;
                StudentId = -1;
                EnrollmentDate = DateOnly.Parse("01.01.2000");
                LeaveDate = DateOnly.Parse("01.01.2000");
            }
        }
    }
}
