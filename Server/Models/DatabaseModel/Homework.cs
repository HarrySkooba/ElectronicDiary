using System;
using System.Collections.Generic;

namespace Server;

public partial class Homework
{
    public int Id { get; set; }

    public int SubjectId { get; set; }

    public int ClassId { get; set; }

    public int TeacherId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;

    public virtual Person Teacher { get; set; } = null!;
}
