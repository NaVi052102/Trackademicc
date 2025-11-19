using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Studentguardian
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public string GuardianName { get; set; } = null!;

    public string? ContactNumber { get; set; }

    public string? Address { get; set; }

    public string? Relationship { get; set; }

    public virtual Student Student { get; set; } = null!;
}
