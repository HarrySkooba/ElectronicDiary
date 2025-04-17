using Server.Models.DatabaseModel;

namespace Server.Models.Model
{
    public class ClassModel
    {
        public int Idclass { get; set; }

        public int SchoolId { get; set; }

        public string Name { get; set; } = null!;

        public int? ClassTeacherId { get; set; }

        public int AcademicYear { get; set; }

        public ClassModel(Class classes)
        {
            if (classes != null)
            {
                Idclass = classes.Id;
                SchoolId = classes.SchoolId;
                Name = classes.Name;
                ClassTeacherId = classes.ClassTeacherId;
                AcademicYear = classes.AcademicYear;
            }
            else
            {
                Idclass = -1;
                SchoolId = -1;
                Name = "Unknown";
                ClassTeacherId = -1;
                AcademicYear = -1;
            }
        }
    }
}
