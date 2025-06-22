using System;
using System.Collections.Generic;

namespace Server.DatabaseModel;

public partial class Grade
{
    public int Id { get; set; }

    public int LessonId { get; set; }

    public int StudentId { get; set; }

    public short? Score { get; set; }

    public bool WasPresent { get; set; }

    public string? Comment { get; set; }

    public virtual Lesson Lesson { get; set; } = null!;

    public virtual Person Student { get; set; } = null!;
}
