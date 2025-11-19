using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Trackademic.Core.Models;

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
            entity.HasKey(e => e.Id).HasName("PK__admins__3213E83F2EBA4795");
            entity.ToTable("admins");
            entity.HasIndex(e => e.Email, "UQ__admins__AB6E61648A8E8CD1").IsUnique();

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

            // Changed 'd.IdNavigation' to 'd.User'
            entity.HasOne(d => d.User).WithOne(p => p.Admin)
                .HasForeignKey<Admin>(d => d.Id)
                .HasConstraintName("FK__admins__id__5AEE82B9");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classes__3213E83F978E8A4E");
            entity.ToTable("classes");
            entity.HasIndex(e => new { e.SubjectId, e.SchoolYearId, e.SemesterId, e.ClassSection }, "UQ__classes__9B6332F6D4474B46").IsUnique();

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
                .HasConstraintName("FK__classes__school___6A30C649");

            entity.HasOne(d => d.Semester).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__semeste__6B24EA82");

            entity.HasOne(d => d.Subject).WithMany(p => p.Classes)
                .HasForeignKey(d => d.SubjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classes__subject__693CA210");
        });

        modelBuilder.Entity<Classassignment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classass__3213E83F5D2B9450");
            entity.ToTable("classassignment");
            entity.HasIndex(e => new { e.ClassId, e.TeacherId }, "UQ__classass__0DCE9EF0A72C861D").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id");

            // Changed 'p.Classassignments' to 'p.ClassAssignments'
            entity.HasOne(d => d.Class).WithMany(p => p.ClassAssignments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classassi__class__6EF57B66");

            // Changed 'p.Classassignments' to 'p.ClassAssignments'
            entity.HasOne(d => d.Teacher).WithMany(p => p.ClassAssignments)
                .HasForeignKey(d => d.TeacherId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classassi__teach__6FE99F9F");
        });

        modelBuilder.Entity<Classenrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__classenr__3213E83F1AF53F34");
            entity.ToTable("classenrollment");
            entity.HasIndex(e => new { e.StudentId, e.ClassId }, "UQ__classenr__55EC41036F45BF06").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClassId).HasColumnName("class_id");
            entity.Property(e => e.EnrollmentDate).HasColumnName("enrollment_date");
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            // Changed 'p.Classenrollments' to 'p.ClassEnrollments'
            entity.HasOne(d => d.Class).WithMany(p => p.ClassEnrollments)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classenro__class__74AE54BC");

            // Changed 'p.Classenrollments' to 'p.ClassEnrollments'
            entity.HasOne(d => d.Student).WithMany(p => p.ClassEnrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__classenro__stude__73BA3083");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__departme__3213E83F2BB4B61C");
            entity.ToTable("departments");
            entity.HasIndex(e => e.DeptName, "UQ__departme__C7D39AE1A5C183D9").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeptName)
                .HasMaxLength(100)
                .HasColumnName("dept_name");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__grades__3213E83F5C505AAB");
            entity.ToTable("grades");
            entity.HasIndex(e => e.EnrollmentId, "UQ__grades__6D24AA7B8AA0CFB4").IsUnique();

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

            // Changed 'd.Enrollment' to 'd.ClassEnrollment'
            entity.HasOne(d => d.ClassEnrollment).WithOne(p => p.Grade)
                .HasForeignKey<Grade>(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__grades__enrollme__787EE5A0");
        });

        modelBuilder.Entity<Schoolyear>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__schoolye__3213E83F8A848080");
            entity.ToTable("schoolyears");
            entity.HasIndex(e => e.YearName, "UQ__schoolye__252258BE4EE6A49E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateEnded).HasColumnName("date_ended");
            entity.Property(e => e.DateStarted).HasColumnName("date_started");
            entity.Property(e => e.YearName)
                .HasMaxLength(50)
                .HasColumnName("year_name");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__semester__3213E83F5590C6E6");
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
            entity.HasKey(e => e.Id).HasName("PK__students__3213E83F0F8054CE");
            entity.ToTable("students");
            entity.HasIndex(e => e.StudentNumber, "UQ__students__0E749A79BD14F1B8").IsUnique();
            entity.HasIndex(e => e.Email, "UQ__students__AB6E6164C7C72C6E").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ContactNumber)
                .HasMaxLength(20)
                .HasColumnName("contact_number");
            entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
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
            entity.Property(e => e.StudentNumber)
                .HasMaxLength(20)
                .HasColumnName("student_number");

            // Changed 'd.IdNavigation' to 'd.User'
            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.Id)
                .HasConstraintName("FK__students__id__656C112C");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__subjects__3213E83F269CFAD9");
            entity.ToTable("subjects");
            entity.HasIndex(e => e.SubjectCode, "UQ__subjects__CEACD920B6B3340C").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DepartmentId).HasColumnName("department_id");
            entity.Property(e => e.SubjectCode)
                .HasMaxLength(10)
                .HasColumnName("subject_code");
            entity.Property(e => e.SubjectName)
                .HasMaxLength(100)
                .HasColumnName("subject_name");

            entity.HasOne(d => d.Department).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__subjects__depart__534D60F1");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__teachers__3213E83FD348AC3E");
            entity.ToTable("teachers");
            entity.HasIndex(e => e.TeacherId, "UQ__teachers__03AE777F1B4A2F2E").IsUnique();
            entity.HasIndex(e => e.Email, "UQ__teachers__AB6E6164232CCE09").IsUnique();

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
                .HasConstraintName("FK__teachers__depart__60A75C0F");

            // Changed 'd.IdNavigation' to 'd.User'
            entity.HasOne(d => d.User).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.Id)
                .HasConstraintName("FK__teachers__id__5FB337D6");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__users__3213E83FB2E57A42");
            entity.ToTable("users");
            entity.HasIndex(e => e.Username, "UQ__users__F3DBC57261C03770").IsUnique();

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