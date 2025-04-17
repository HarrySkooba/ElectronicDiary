
using Server.Models.DatabaseModel;

namespace Server.Models.ModelDTO
{
    public class SubjectTeacherModel
    {
        public int Idsubjectteacher { get; set; }

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        public int ClassId { get; set; }

        public int AcademicYear { get; set; }

        public SubjectTeacherModel(SubjectTeacher subjectTeacher) 
        {
            if (subjectTeacher != null)
            {
                Idsubjectteacher = subjectTeacher.Id;
                SubjectId = subjectTeacher.SubjectId;
                ClassId = subjectTeacher.ClassId;
                TeacherId = subjectTeacher.TeacherId;
                AcademicYear = subjectTeacher.AcademicYear;
            }
            else 
            {
                Idsubjectteacher = -1;
                SubjectId = -1;
                ClassId = -1;
                TeacherId = -1;
                AcademicYear = -1;
            }
        }
    }
}
