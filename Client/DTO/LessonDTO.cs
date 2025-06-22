
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;

namespace Client.DTO
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

        public bool IsModified { get; set; } 
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
        public List<GradeUpdateDTO> Grades { get; set; } = new();
    }

    public class GradeUpdateDTO
    {
        public int StudentId { get; set; }
        public int? Score { get; set; }
        public bool WasPresent { get; set; }
        public string Comment { get; set; } = string.Empty;
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
        [NotMapped]
        public bool IsModified { get; set; }
    }

    public class GradeViewDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int? Score { get; set; }
        public bool WasPresent { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class OperationResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;

        public static OperationResult Success(string message = "")
            => new() { IsSuccess = true, Message = message };

        public static OperationResult Fail(string message)
            => new() { IsSuccess = false, Message = message };
    }
}