using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Student
{
    public long Id { get; set; }

    public string StudentNumber { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Email { get; set; }

    public string? ContactNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public string? Gender { get; set; }

    public string? YearLevel { get; set; }

    public string? CourseProgram { get; set; }

    public virtual ICollection<Classenrollment> Classenrollments { get; set; } = new List<Classenrollment>();

    public virtual User IdNavigation { get; set; } = null!;

    public virtual ICollection<Studentguardian> Studentguardians { get; set; } = new List<Studentguardian>();
}
