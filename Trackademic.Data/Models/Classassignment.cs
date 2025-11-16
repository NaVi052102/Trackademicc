using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Classassignment
{
    public long Id { get; set; }

    public long ClassId { get; set; }

    public long TeacherId { get; set; }

    public virtual Class Class { get; set; } = null!;

    public virtual Teacher Teacher { get; set; } = null!;
}
