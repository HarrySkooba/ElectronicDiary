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
        public DateOnly Date { get; set; }
    }

    public class ScheduleDayDto
    {
        public DateOnly Date { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public List<ScheduleDTO> Lessons { get; set; } = new();
    }
}
