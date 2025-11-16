using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Student
    {
        [Key] // Identifies this as the Primary Key
        [ForeignKey("User")] // Links it to the User table
        public long Id { get; set; }

        [Column("student_number")]
        public string StudentNumber { get; set; } = null!;

        [Column("first_name")]
        public string FirstName { get; set; } = null!;

        [Column("last_name")]
        public string LastName { get; set; } = null!;

        [Column("email")]
        public string? Email { get; set; }

        [Column("contact_number")]
        public string? ContactNumber { get; set; }

        [Column("date_of_birth")]
        public DateOnly? DateOfBirth { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        // --- FIX 1 ---
        // Renamed 'Classenrollments' to 'ClassEnrollments' (capital E) to match DbContext
        public virtual ICollection<Classenrollment> ClassEnrollments { get; set; } = new List<Classenrollment>();

        // --- FIX 2 ---
        // Renamed 'IdNavigation' to 'User' to match DbContext
        public virtual User User { get; set; } = null!;
    }
}