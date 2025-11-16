using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Schoolyear
    {
        [Key]
        public long Id { get; set; }

        [Column("year_name")]
        public string YearName { get; set; } = null!;

        [Column("date_started")]
        public DateOnly? DateStarted { get; set; }

        [Column("date_ended")]
        public DateOnly? DateEnded { get; set; }

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

        public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    }
}