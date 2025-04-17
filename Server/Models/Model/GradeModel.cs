using Server.Models.DatabaseModel;

namespace Server.Models.Model
{
    public class GradeModel
    {
        public int Idgrade { get; set; }

        public int StudentId { get; set; }

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        public decimal GradeValue { get; set; }

        public short GradeType { get; set; }

        public DateOnly Date { get; set; }

        public string? Comment { get; set; }

        public GradeModel(Grade grade) 
        { 
            if(grade != null)
            {
                Idgrade = grade.Id;
                StudentId = grade.StudentId;
                SubjectId = grade.SubjectId;
                TeacherId = grade.TeacherId;
                GradeValue = grade.GradeValue;
                GradeType = grade.GradeType;
                Date = grade.Date;
                Comment = grade.Comment;
            }
            else
            {
                Idgrade = -1;
                StudentId = -1;
                SubjectId = -1;
                TeacherId = -1;
                GradeValue= -1;
                GradeType = -1;
                Date = DateOnly.Parse("01.01.2000");
                Comment = "Unknown";
            }
        }
    }
}
