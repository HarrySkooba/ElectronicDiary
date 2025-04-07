using System;
using System.Collections.Generic;

namespace Server.Models.DatabaseModel;

public partial class Grade
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int SubjectId { get; set; }

    public int TeacherId { get; set; }

    public decimal GradeValue { get; set; }

    public short GradeType { get; set; }

    public DateOnly Date { get; set; }

    public string? Comment { get; set; }

    public virtual Person Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Person Teacher { get; set; } = null!;
}
