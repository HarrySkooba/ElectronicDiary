using System;
using System.Collections.Generic;

namespace Server;

public partial class Lesson
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public DateTime Date { get; set; }

    public string? Homework { get; set; }

    public virtual ICollection<Grade> Grades { get; set; } = new List<Grade>();

    public virtual Schedule Schedule { get; set; } = null!;
}
