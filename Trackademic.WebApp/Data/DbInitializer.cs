using BCrypt.Net;
using Trackademic.Core.Models;
using Trackademic.Data.Data;

namespace Trackademic.WebApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(TrackademicDbContext context)
        {
            // Ensure the database exists
            context.Database.EnsureCreated();

            // Look for any users. If users exist, we assume the DB is already seeded.
            //if (context.Users.Any())
            //{
                //return;
            //}

            // ---------------------------------------------------------
            // 1. Create Departments (Required for Teachers)
            // ---------------------------------------------------------
            var deptCCS = new Department { DeptName = "College of Computer Studies" };
            var deptEduc = new Department { DeptName = "College of Education" };

            context.Departments.AddRange(deptCCS, deptEduc);
            context.SaveChanges(); // Save to generate IDs

            // ---------------------------------------------------------
            // 2. Create a STUDENT
            // Login: S-2024-001
            // Password: password123
            // ---------------------------------------------------------
            var studentUser = new User
            {
                Username = "student_user",
                // We MUST hash the password for the login logic to work
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                UserType = "Student"
            };
            context.Users.Add(studentUser);
            context.SaveChanges(); // Save to get the User ID

            var studentProfile = new Student
            {
                Id = studentUser.Id, // Links to the User table
                StudentNumber = "S-2024-001", // <--- This is the SchoolID for login
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@student.trackademic.edu",
                DateOfBirth = DateOnly.Parse("2000-01-01")
            };
            context.Students.Add(studentProfile);

            // ---------------------------------------------------------
            // 3. Create a TEACHER
            // Login: T-2024-001
            // Password: password123
            // ---------------------------------------------------------
            var teacherUser = new User
            {
                Username = "teacher_user",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                UserType = "Teacher"
            };
            context.Users.Add(teacherUser);
            context.SaveChanges();

            var teacherProfile = new Teacher
            {
                Id = teacherUser.Id, // Links to the User table
                TeacherId = "T-2024-001", // <--- This is the SchoolID for login
                DepartmentId = deptCCS.Id,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@trackademic.edu",
                DateOfBirth = DateOnly.Parse("1985-05-15")
            };
            context.Teachers.Add(teacherProfile);

            // ---------------------------------------------------------
            // 4. Create an ADMIN
            // Login: admin
            // Password: admin123
            // ---------------------------------------------------------
            var adminUser = new User
            {
                Username = "admin", // <--- This is the Username for login
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                UserType = "Admin"
            };
            context.Users.Add(adminUser);
            context.SaveChanges();

            var adminProfile = new Admin
            {
                Id = adminUser.Id,
                FirstName = "Super",
                LastName = "Admin",
                Email = "admin@trackademic.edu"
            };
            context.Admins.Add(adminProfile);

            // =========================================================
            // 5. NEW: Create Teacher "Smth" (Limitless Civilian)
            // Login: T-2025-SMTH
            // Password: password123
            // =========================================================
            var smthUser = new User
            {
                Username = "smth_teacher",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                UserType = "Teacher"
            };
            context.Users.Add(smthUser);
            context.SaveChanges(); // Save to get the ID

            var smthProfile = new Teacher
            {
                Id = smthUser.Id,
                TeacherId = "T-2025-SMTH", // Use this ID to log in
                DepartmentId = deptEduc.Id, // Assigning to Education dept for variety
                FirstName = "Smth",
                LastName = "Civilian",
                Email = "limitlesscivilian@gmail.com", // <--- The email you requested
                DateOfBirth = DateOnly.Parse("1990-01-01")
            };
            context.Teachers.Add(smthProfile);

            // Final Save
            context.SaveChanges();
        }
    }
}