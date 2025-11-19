using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Classenrollment
{
    public long Id { get; set; }

    public long StudentId { get; set; }

    public long ClassId { get; set; }

    public DateOnly EnrollmentDate { get; set; }

    public string EnrollmentStatus { get; set; } = null!;

    public virtual Class Class { get; set; } = null!;

    public virtual Grade? Grade { get; set; }

    public virtual Student Student { get; set; } = null!;
}
