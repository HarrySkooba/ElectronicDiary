using System;
using System.Collections.Generic;

namespace Server.Models.DatabaseModel;

public partial class School
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? Phone { get; set; }

    public int? DirectorId { get; set; }

    public string? Website { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Person? Director { get; set; }
}
