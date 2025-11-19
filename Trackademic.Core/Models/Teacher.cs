using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Teacher
    {
        [Key]
        [ForeignKey("User")]
        public long Id { get; set; }

        [Column("teacher_id")]
        public string TeacherId { get; set; } = null!;

        [Column("department_id")]
        public long DepartmentId { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = null!;

        [Column("last_name")]
        public string LastName { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("contact_number")]
        public string? ContactNumber { get; set; }

        [Column("date_of_birth")]
        public DateOnly? DateOfBirth { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("profile_picture_url")]
        public string? ProfilePictureUrl { get; set; }

        // FIX
        // Renamed 'Classassignments' to 'ClassAssignments' to match DbContext
        public virtual ICollection<Classassignment> ClassAssignments { get; set; } = new List<Classassignment>();

        public virtual Department Department { get; set; } = null!;

        // FIX
        // Renamed 'IdNavigation' to 'User' to match DbContext
        public virtual User User { get; set; } = null!;
    }
}