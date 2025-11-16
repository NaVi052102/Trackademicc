using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Grade
    {
        [Key]
        public long Id { get; set; }

        [Column("enrollment_id")]
        public long EnrollmentId { get; set; }

        [Column("midterm_grade")]
        public decimal? MidtermGrade { get; set; }

        [Column("final_grade")]
        public decimal? FinalGrade { get; set; }

        [Column("final_score")]
        public decimal? FinalScore { get; set; }

        // --- THIS IS THE FIX ---
        // Renamed 'Enrollment' to 'ClassEnrollment' to match the DbContext
        public virtual Classenrollment ClassEnrollment { get; set; } = null!;
    }
}