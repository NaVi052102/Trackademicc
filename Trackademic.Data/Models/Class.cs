using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Class
{
    public long Id { get; set; }

    public long SubjectId { get; set; }

    public long SchoolYearId { get; set; }

    public long SemesterId { get; set; }

    public string ClassSection { get; set; } = null!;

    public virtual ICollection<Classassignment> Classassignments { get; set; } = new List<Classassignment>();

    public virtual ICollection<Classenrollment> Classenrollments { get; set; } = new List<Classenrollment>();

    public virtual Schoolyear SchoolYear { get; set; } = null!;

    public virtual Semester Semester { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
