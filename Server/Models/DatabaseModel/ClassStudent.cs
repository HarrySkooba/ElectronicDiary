using System;
using System.Collections.Generic;

namespace Server.Models.DatabaseModel;

public partial class ClassStudent
{
    public int Id { get; set; }

    public int ClassId { get; set; }

    public int StudentId { get; set; }

    public DateOnly EnrollmentDate { get; set; }

    public DateOnly? LeaveDate { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Person Student { get; set; } = null!;
}
