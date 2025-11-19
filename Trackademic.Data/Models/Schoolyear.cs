using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Schoolyear
{
    public long Id { get; set; }

    public string YearName { get; set; } = null!;

    public DateOnly? DateStarted { get; set; }

    public DateOnly? DateEnded { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
}
