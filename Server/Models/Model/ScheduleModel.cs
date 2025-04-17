using Server.Models.DatabaseModel;

namespace Server.Models.Model
{
    public class ScheduleModel
    {
        public int Idschedule { get; set; }

        public int ClassId { get; set; }

        public int SubjectId { get; set; }

        public int TeacherId { get; set; }

        public short DayOfWeek { get; set; }

        public short LessonNumber { get; set; }

        public string? Room { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public ScheduleModel(Schedule schedule) 
        { 
            if (schedule != null)
            {
                Idschedule = schedule.Id;
                ClassId = schedule.ClassId;
                SubjectId = schedule.SubjectId;
                TeacherId = schedule.TeacherId;
                DayOfWeek = schedule.DayOfWeek;
                LessonNumber = schedule.LessonNumber;
                Room = schedule.Room;
                StartTime = schedule.StartTime;
                EndTime = schedule.EndTime;
            }
            else
            {
                Idschedule = -1;
                ClassId = -1;
                SubjectId = -1;
                TeacherId = -1;
                DayOfWeek = -1;
                LessonNumber = -1;
                Room = "Unknown";
                StartTime = TimeOnly.Parse("00:00");
                EndTime = TimeOnly.Parse("00:00");
            }
        }
    }
}
