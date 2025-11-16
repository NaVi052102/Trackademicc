using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Admin
    {
        [Key] // Identifies this as the Primary Key
        [ForeignKey("User")] // Links it to the User table
        public long Id { get; set; }

        [Column("first_name")] // Maps this property to the 'first_name' column
        public string FirstName { get; set; } = null!;

        [Column("last_name")] // Maps this property to the 'last_name' column
        public string LastName { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        // --- THIS IS THE FIX ---
        // Renamed 'IdNavigation' to 'User' to be clear and match the DbContext
        public virtual User User { get; set; } = null!;
    }
}