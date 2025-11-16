using Microsoft.EntityFrameworkCore;
using Trackademic.Core.Models; // <-- The correct path to your models

namespace Trackademic.Data
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
        {
        }

        // --- Core Tables ---
        public DbSet<Department> Departments { get; set; }
        public DbSet<Schoolyear> Schoolyears { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Subject> Subjects { get; set; }

        // --- User and Profile Tables ---
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Student> Students { get; set; }

        // --- Class and Enrollment Tables ---
        public DbSet<Class> Classes { get; set; }
        public DbSet<Classassignment> ClassAssignments { get; set; }
        public DbSet<Classenrollment> ClassEnrollments { get; set; }

        // --- Grading Table ---
        public DbSet<Grade> Grades { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Set Table Names Explicitly ---
            modelBuilder.Entity<Department>().ToTable("departments");
            modelBuilder.Entity<Schoolyear>().ToTable("schoolyears");
            modelBuilder.Entity<Semester>().ToTable("semesters");
            modelBuilder.Entity<Subject>().ToTable("subjects");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Admin>().ToTable("admins");
            modelBuilder.Entity<Teacher>().ToTable("teachers");
            modelBuilder.Entity<Student>().ToTable("students");
            modelBuilder.Entity<Class>().ToTable("classes");
            modelBuilder.Entity<Classassignment>().ToTable("classassignment");
            modelBuilder.Entity<Classenrollment>().ToTable("classenrollment");
            modelBuilder.Entity<Grade>().ToTable("grades");

            // --- Configure Relationships (THIS SECTION IS NOW FIXED) ---

            // User -> Admin (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Admin)
                .WithOne(a => a.User)
                .HasForeignKey<Admin>(a => a.Id); // Was a.id

            // User -> Teacher (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Teacher)
                .WithOne(t => t.User)
                .HasForeignKey<Teacher>(t => t.Id); // Was t.id

            // User -> Student (One-to-One)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.Id); // Was s.id

            // Grade -> ClassEnrollment (One-to-One)
            modelBuilder.Entity<Grade>()
                .HasOne(g => g.ClassEnrollment)
                .WithOne(ce => ce.Grade)
                .HasForeignKey<Grade>(g => g.EnrollmentId); // Was g.enrollment_id

            // ClassEnrollment -> Student (Many-to-One)
            modelBuilder.Entity<Classenrollment>()
                .HasOne(ce => ce.Student)
                .WithMany(s => s.ClassEnrollments)
                .HasForeignKey(ce => ce.StudentId); // Was ce.student_id

            // ClassEnrollment -> Class (Many-to-One)
            modelBuilder.Entity<Classenrollment>()
                .HasOne(ce => ce.Class)
                .WithMany(c => c.ClassEnrollments)
                .HasForeignKey(ce => ce.ClassId); // Was ce.class_id

            // ClassAssignment -> Teacher (Many-to-One)
            modelBuilder.Entity<Classassignment>()
                .HasOne(ca => ca.Teacher)
                .WithMany(t => t.ClassAssignments)
                .HasForeignKey(ca => ca.TeacherId); // Was ca.teacher_id

            // ClassAssignment -> Class (Many-to-One)
            modelBuilder.Entity<Classassignment>()
                .HasOne(ca => ca.Class)
                .WithMany(c => c.ClassAssignments)
                .HasForeignKey(ca => ca.ClassId); // Was ca.class_id
        }
    }
}