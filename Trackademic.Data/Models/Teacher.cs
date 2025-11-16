using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Teacher
{
    public long Id { get; set; }

    public string TeacherId { get; set; } = null!;

    public long DepartmentId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? ContactNumber { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Address { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public virtual ICollection<Classassignment> Classassignments { get; set; } = new List<Classassignment>();

    public virtual Department Department { get; set; } = null!;

    public virtual User IdNavigation { get; set; } = null!;
}
