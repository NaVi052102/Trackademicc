using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Admin
{
    public long Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public virtual User IdNavigation { get; set; } = null!;
}
