using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Department
{
    public long Id { get; set; }

    public string DeptName { get; set; } = null!;

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
