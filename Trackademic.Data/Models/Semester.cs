using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Semester
{
    public long Id { get; set; }

    public long SchoolYearId { get; set; }

    public string SemesterName { get; set; } = null!;

    public DateOnly? DateStarted { get; set; }

    public DateOnly? DateEnded { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Schoolyear SchoolYear { get; set; } = null!;
}
