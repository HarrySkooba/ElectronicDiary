using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.DTO
{
    public class ScheduleDTO
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Teacher { get; set; }
        public DayOfWeek Day_Of_Week { get; set; }
        public int Lesson_Number { get; set; }
        public string Room { get; set; }
        public TimeOnly Start_Time { get; set; }
        public TimeOnly End_Time { get; set; }
        public string ClassName { get; set; }
        public DateOnly Date { get; set; }
        public bool IsCurrentDay { get; set; }
    }

    public class DayScheduleDTO
    {
        public string DayName { get; set; }
        public DateOnly Date { get; set; }
        public List<ScheduleDTO> Lessons { get; set; }
        public bool IsCurrentDay { get; set; }
    }

    public class CopyScheduleWeekRequest
    {
        public DateOnly SourceWeekStart { get; set; }
        public DateOnly TargetWeekStart { get; set; }
    }

    public class ScheduleResponseDTO
    {
        public Dictionary<DateOnly, List<ScheduleDTO>> Schedule { get; set; }
    }

    public class ScheduleDayDto
    {
        public DateOnly Date { get; set; }
        public int DayOfWeek { get; set; }
        public string DayName { get; set; } = string.Empty;
        public List<ScheduleDTO> Lessons { get; set; } = new();
    }

}
