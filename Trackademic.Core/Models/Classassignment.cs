using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Trackademic.Core.Models
{
    public partial class Classassignment
    {
        [Key]
        public long Id { get; set; }

        [Column("class_id")]
        public long ClassId { get; set; }

        [Column("teacher_id")]
        public long TeacherId { get; set; }

        public virtual Class Class { get; set; } = null!;

        public virtual Teacher Teacher { get; set; } = null!;
    }
}