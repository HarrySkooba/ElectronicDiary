using Server.Models.DatabaseModel;

namespace Server.Models.Model
{
    public class HomeworkModel
    {
        public int Idhomework { get; set; }

        public int SubjectId { get; set; }

        public int ClassId { get; set; }

        public int TeacherId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime? CreatedAt { get; set; }

        public HomeworkModel(Homework homework) 
        {
            if(homework != null)
            {
               Idhomework = homework.Id;
               SubjectId = homework.SubjectId;
               ClassId = homework.ClassId;
               TeacherId = homework.TeacherId;
               Title = homework.Title;
               Description = homework.Description;
               DueDate = homework.DueDate;
               CreatedAt = homework.CreatedAt;
            }
            else
            {
                Idhomework = -1;
                SubjectId = -1;
                ClassId = -1;
                TeacherId = -1;
                Title = "Unknown";
                Description = "Unknown";
                DueDate = DateTime.Parse("01.01.2000");
                CreatedAt = null;
            }
        }
    }
}
