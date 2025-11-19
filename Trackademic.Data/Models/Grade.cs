using System;
using System.Collections.Generic;

namespace Trackademic.Data.Models;

public partial class Grade
{
    public long Id { get; set; }

    public long EnrollmentId { get; set; }

    public decimal? MidtermGrade { get; set; }

    public decimal? FinalGrade { get; set; }

    public decimal? FinalScore { get; set; }

    public virtual Classenrollment Enrollment { get; set; } = null!;
}
