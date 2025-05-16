namespace Server.Models.DTO
{
    public class ScheduleDTO
    {
        public int Id { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
        public int Day_Of_Week { get; set; }
        public int Lesson_Number { get; set; }
        public string Room { get; set; } = string.Empty;
        public TimeOnly Start_Time { get; set; }
        public TimeOnly End_Time { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }

    public class ScheduleEditDTO
    {
        public int? Id { get; set; } 
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int TeacherId { get; set; }
        public int DayOfWeek { get; set; } 
        public int LessonNumber { get; set; } 
        public string? Room { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
