using System;
using System.Collections.Generic;

namespace Server.Models.DatabaseModel;

public partial class SubjectTeacher
{
    public int Id { get; set; }

    public int SubjectId { get; set; }

    public int TeacherId { get; set; }

    public int ClassId { get; set; }

    public int AcademicYear { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Person Teacher { get; set; } = null!;
}
