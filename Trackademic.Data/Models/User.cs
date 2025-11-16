using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class User
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string UserType { get; set; } = null!;

    public virtual Admin? Admin { get; set; }

    public virtual Student? Student { get; set; }

    public virtual Teacher? Teacher { get; set; }
}
