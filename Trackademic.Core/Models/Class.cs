using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Class
    {
        [Key]
        public long Id { get; set; }

        [Column("subject_id")]
        public long SubjectId { get; set; }

        [Column("school_year_id")]
        public long SchoolYearId { get; set; }

        [Column("semester_id")]
        public long SemesterId { get; set; }

        [Column("class_section")]
        public string ClassSection { get; set; } = null!;

        // --- FIX 1 ---
        // Renamed 'Classassignments' to 'ClassAssignments' (capital A)
        public virtual ICollection<Classassignment> ClassAssignments { get; set; } = new List<Classassignment>();

        // --- FIX 2 ---
        // Renamed 'Classenrollments' to 'ClassEnrollments' (capital E)
        public virtual ICollection<Classenrollment> ClassEnrollments { get; set; } = new List<Classenrollment>();

        public virtual Schoolyear SchoolYear { get; set; } = null!;

        public virtual Semester Semester { get; set; } = null!;

        public virtual Subject Subject { get; set; } = null!;
    }
}