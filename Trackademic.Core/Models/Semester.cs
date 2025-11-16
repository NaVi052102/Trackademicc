using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Semester
    {
        [Key]
        public long Id { get; set; }

        [Column("school_year_id")]
        public long SchoolYearId { get; set; }

        [Column("semester_name")]
        public string SemesterName { get; set; } = null!;

        [Column("date_started")]
        public DateOnly? DateStarted { get; set; }

        [Column("date_ended")]
        public DateOnly? DateEnded { get; set; }

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

        public virtual Schoolyear SchoolYear { get; set; } = null!;
    }
}