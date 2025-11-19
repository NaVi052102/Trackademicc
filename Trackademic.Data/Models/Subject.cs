using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Subject
{
    public long Id { get; set; }

    public long DepartmentId { get; set; }

    public string SubjectCode { get; set; } = null!;

    public string SubjectName { get; set; } = null!;

    public int? CreditUnits { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual Department Department { get; set; } = null!;
}
