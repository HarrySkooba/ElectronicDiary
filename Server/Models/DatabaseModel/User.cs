using System;
using System.Collections.Generic;

namespace Server.DatabaseModel;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public int PersonId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public byte[] Salt { get; set; } = null!;

    public virtual Person Person { get; set; } = null!;

    public virtual Role Role { get; set; } = null!;
}
