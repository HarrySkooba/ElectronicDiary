using System;
using System.Collections.Generic;

namespace Server.DatabaseModel;

public partial class Schedule
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int SubjectId { get; set; }

    public int TeacherId { get; set; }

    public short DayOfWeek { get; set; }

    public short LessonNumber { get; set; }

    public string? Room { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public DateOnly DateTimetable { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Subject Subject { get; set; } = null!;

    public virtual Person Teacher { get; set; } = null!;
}
