using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Subject
    {
        [Key]
        public long Id { get; set; }

        [Column("department_id")]
        public long DepartmentId { get; set; }

        [Column("subject_code")]
        public string SubjectCode { get; set; } = null!;

        [Column("subject_name")]
        public string SubjectName { get; set; } = null!;

        public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

        public virtual Department Department { get; set; } = null!;
    }
}