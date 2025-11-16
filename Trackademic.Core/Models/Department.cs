using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Department
    {
        [Key]
        public long Id { get; set; }

        [Column("dept_name")] // Maps this property to the 'dept_name' column
        public string DeptName { get; set; } = null!;

        public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

        public virtual ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
    }
}