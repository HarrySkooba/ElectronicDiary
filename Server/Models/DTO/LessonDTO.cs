namespace Server.Models.DTO
{
    public class LessonDTO
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public DateOnly Date { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Homework { get; set; } = string.Empty;
        public List<GradeDTO> Grades { get; set; } = new();
    }

    public class GradeDTO
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int? Score { get; set; }
        public bool WasPresent { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class UpdateGradesDTO
    {
        public int LessonId { get; set; }
        public string? Homework { get; set; }
        public List<GradeDTO> Grades { get; set; } = new();
    }
    public class LessonViewDto
    {
        public int Id { get; set; }
        public DateOnly Date { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Teacher { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string Homework { get; set; } = string.Empty;
        public List<GradeViewDto> Grades { get; set; } = new();
    }

    public class GradeViewDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int? Score { get; set; }
        public bool WasPresent { get; set; }
        public string Comment { get; set; } = string.Empty;
    }
}
