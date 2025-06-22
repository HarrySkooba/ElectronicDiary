using System;
using System.Collections.Generic;

namespace Server.DatabaseModel;

public partial class School
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public int DirectorId { get; set; }

    public string? Website { get; set; }

    public virtual Person Director { get; set; } = null!;

    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}
