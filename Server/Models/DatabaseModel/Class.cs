using System;
using System.Collections.Generic;

namespace Server.DatabaseModel;

public partial class Class
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int? ClassTeacherId { get; set; }

    public int AcademicYear { get; set; }

    public virtual ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();

    public virtual Person? ClassTeacher { get; set; }

    public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();

    public virtual ICollection<SubjectTeacher> SubjectTeachers { get; set; } = new List<SubjectTeacher>();
}
