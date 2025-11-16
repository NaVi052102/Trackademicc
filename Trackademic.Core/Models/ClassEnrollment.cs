using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Classenrollment
    {
        [Key]
        public long Id { get; set; }

        [Column("student_id")]
        public long StudentId { get; set; }

        [Column("class_id")]
        public long ClassId { get; set; }

        [Column("enrollment_date")]
        public DateOnly EnrollmentDate { get; set; }

        public virtual Class Class { get; set; } = null!;

        public virtual Grade? Grade { get; set; }

        public virtual Student Student { get; set; } = null!;
    }
}