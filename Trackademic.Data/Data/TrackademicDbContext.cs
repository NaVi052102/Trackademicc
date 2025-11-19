using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Trackademic.Data.Models;

namespace Trackademic.Data.Data;

public partial class TrackademicDbContext : DbContext
{
    public TrackademicDbContext()
    {
    }

    public TrackademicDbContext(DbContextOptions<TrackademicDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Classassignment> Classassignments { get; set; }

    public virtual DbSet<Classenrollment> Classenrollments { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<Schoolyear> Schoolyears { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Studentguardian> Studentguardians { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=Trackademic;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__admins__3213E83F1BB9842D");

            entity.ToTable("admins");

            entity.HasIndex(e => e.Email, "UQ__admins__AB6E6164234738DA").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.Id)
                .HasConstraintName("FK__admins__id__5BE2A6F2");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classes__3213E83FF2674D10");

            entity.ToTable("classes");

            entity.HasIndex(e => new { e.SubjectId, e.SchoolYearId, e.SemesterId, e.ClassSection }, "UQ__classes__9B6332F6CA419BDE").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassSection)
                .HasMaxLength(50)
                .HasColumnName("class_section");
            entity.Property(e => e.SchoolYearId).HasColumnName("school_year_id");
            entity.Property(e => e.SemesterId).HasColumnName("semester_id");
            entity.Property(e => e.SubjectId).HasColumnName("subject_id");

            entity.HasOne(d => d.SchoolYear).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SchoolYearId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__school___6E01572D");

            entity.HasOne(d => d.Semester).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__semeste__6EF57B66");

            entity.HasOne(d => d.Subject).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__subject__6D0D32F4");
        });

        modelBuilder.Entity<Classassignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classass__3213E83F8E9147C8");

            entity.ToTable("classassignment");

            entity.HasIndex(e => new { e.ClassId, e.TeacherId }, "UQ__classass__0DCE9EF02D963F83").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");

            entity.HasOne(d => d.Class).WithMany(p => p.Classassignments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classassi__class__72C60C4A");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Classassignments)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classassi__teach__73BA3083");
        });

        modelBuilder.Entity<Classenrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classenr__3213E83F2C197F74");

            entity.ToTable("classenrollment");

            entity.HasIndex(e => new { e.StudentId, e.ClassId }, "UQ__classenr__55EC4103EB895D64").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
            entity.Property(e => e.EnrollmentStatus)
                .HasMaxLength(20)
                .HasDefaultValue("Enrolled")
                .HasColumnName("enrollment_status");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Class).WithMany(p => p.Classenrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classenro__class__7A672E12");

            entity.HasOne(d => d.Student).WithMany(p => p.Classenrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classenro__stude__797309D9");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__departme__3213E83FFCDF7DF3");

            entity.ToTable("departments");

            entity.HasIndex(e => e.DeptName, "UQ__departme__C7D39AE1E5B7F9D1").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeptName)
                .HasMaxLength(100)
                .HasColumnName("dept_name");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__grades__3213E83F66A955B6");

            entity.ToTable("grades");

            entity.HasIndex(e => e.EnrollmentId, "UQ__grades__6D24AA7BA526EC83").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.FinalGrade)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("final_grade");
            entity.Property(e => e.FinalScore)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("final_score");
            entity.Property(e => e.MidtermGrade)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("midterm_grade");

            entity.HasOne(d => d.Enrollment).WithOne(p => p.Grade)
                .HasForeignKey<Grade>(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__grades__enrollme__7E37BEF6");
        });

        modelBuilder.Entity<Schoolyear>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__schoolye__3213E83FAC35063C");

            entity.ToTable("schoolyears");

            entity.HasIndex(e => e.YearName, "UQ__schoolye__252258BEE65D1809").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateEnded).HasColumnName("date_ended");
            entity.Property(e => e.DateStarted).HasColumnName("date_started");
            entity.Property(e => e.YearName)
                .HasMaxLength(50)
                .HasColumnName("year_name");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__semester__3213E83F2EC2B606");

            entity.ToTable("semesters");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateEnded).HasColumnName("date_ended");
            entity.Property(e => e.DateStarted).HasColumnName("date_started");
            entity.Property(e => e.SchoolYearId).HasColumnName("school_year_id");
            entity.Property(e => e.SemesterName)
                .HasMaxLength(50)
                .HasColumnName("semester_name");

            entity.HasOne(d => d.SchoolYear).WithMany(p => p.Semesters)
                .HasForeignKey(d => d.SchoolYearId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__semesters__schoo__4F7CD00D");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__students__3213E83FB0E1A278");

            entity.ToTable("students");

            entity.HasIndex(e => e.StudentNumber, "UQ__students__0E749A79F04C09D5").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__students__AB6E61647E176D75").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .HasColumnName("contact_number");
            entity.Property(e => e.CourseProgram)
                .HasMaxLength(100)
                .HasColumnName("course_program");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.ProfilePictureUrl).HasColumnName("profile_picture_url");
            entity.Property(e => e.StudentNumber)
                .HasMaxLength(20)
                .HasColumnName("student_number");
            entity.Property(e => e.YearLevel)
                .HasMaxLength(20)
                .HasColumnName("year_level");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.Id)
                .HasConstraintName("FK__students__id__66603565");
        });

        modelBuilder.Entity<Studentguardian>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__studentg__3213E83F3D78241B");

            entity.ToTable("studentguardians");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .HasColumnName("contact_number");
            entity.Property(e => e.GuardianName)
                .HasMaxLength(100)
                .HasColumnName("guardian_name");
            entity.Property(e => e.Relationship)
                .HasMaxLength(50)
                .HasColumnName("relationship");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(d => d.Student).WithMany(p => p.Studentguardians)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__studentgu__stude__693CA210");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subjects__3213E83F3BAC5544");

            entity.ToTable("subjects");

            entity.HasIndex(e => e.SubjectCode, "UQ__subjects__CEACD920ED475AA6").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreditUnits)
                .HasDefaultValue(3)
                .HasColumnName("credit_units");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.SubjectCode)
                .HasMaxLength(10)
                .HasColumnName("subject_code");
            entity.Property(e => e.SubjectName)
                .HasMaxLength(100)
                .HasColumnName("subject_name");

            entity.HasOne(d => d.Department).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__subjects__depart__5441852A");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__teachers__3213E83F890A7A5F");

            entity.ToTable("teachers");

            entity.HasIndex(e => e.TeacherId, "UQ__teachers__03AE777F57FC656C").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__teachers__AB6E6164AAF62060").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .HasColumnName("contact_number");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.ProfilePictureUrl).HasColumnName("profile_picture_url");
            entity.Property(e => e.TeacherId)
                .HasMaxLength(20)
                .HasColumnName("teacher_id");

            entity.HasOne(d => d.Department).WithMany(p => p.Teachers)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__teachers__depart__619B8048");

            entity.HasOne(d => d.IdNavigation).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.Id)
                .HasConstraintName("FK__teachers__id__60A75C0F");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FAFE342CE");

            entity.ToTable("users");

            entity.HasIndex(e => e.Username, "UQ__users__F3DBC57267EE967E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.UserType)
                .HasMaxLength(20)
                .HasColumnName("user_type");
            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
