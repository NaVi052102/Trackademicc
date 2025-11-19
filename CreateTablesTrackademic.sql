Use Trackademic;

/*
 * Trackademic Database Schema - T-SQL (Microsoft SQL Server)
 * UPDATED: Added fields for Student Profile, Grades, Class Cards, and Guardians.
 */

-- ----------------------------------------------------------------------
-- 1. CORE ACADEMIC STRUCTURE
-- ----------------------------------------------------------------------

CREATE TABLE departments (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    dept_name NVARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE schoolyears (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    year_name NVARCHAR(50) UNIQUE NOT NULL,
    date_started DATE,
    date_ended DATE
);

CREATE TABLE semesters (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    school_year_id BIGINT NOT NULL,
    semester_name NVARCHAR(50) NOT NULL,
    date_started DATE,
    date_ended DATE,
    FOREIGN KEY (school_year_id) REFERENCES schoolyears (id)
);

CREATE TABLE subjects (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    department_id BIGINT NOT NULL,
    subject_code NVARCHAR(10) UNIQUE NOT NULL,
    subject_name NVARCHAR(100) NOT NULL,
    
    -- NEW FIELDS
    credit_units INT DEFAULT 3, -- Defaulting to 3 units if not specified
    image_url NVARCHAR(MAX),    -- For the Class Card visual
    
    FOREIGN KEY (department_id) REFERENCES departments (id)
);

-- ----------------------------------------------------------------------
-- 2. CORE USERS AND PROFILES
-- ----------------------------------------------------------------------

CREATE TABLE users (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    username NVARCHAR(255) UNIQUE NOT NULL,
    password_hash NVARCHAR(MAX) NOT NULL,
    user_type NVARCHAR(20) NOT NULL,
    CONSTRAINT CK_user_type CHECK (user_type IN ('Admin', 'Teacher', 'Student'))
);

CREATE TABLE admins (
    id BIGINT PRIMARY KEY,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    email NVARCHAR(100) UNIQUE NOT NULL,
    FOREIGN KEY (id) REFERENCES users (id) ON DELETE CASCADE
);

CREATE TABLE teachers (
    id BIGINT PRIMARY KEY,
    teacher_id NVARCHAR(20) UNIQUE NOT NULL,
    department_id BIGINT NOT NULL,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    email NVARCHAR(100) UNIQUE NOT NULL,
    contact_number NVARCHAR(20),
    date_of_birth DATE,
    address NVARCHAR(MAX),
    profile_picture_url NVARCHAR(MAX),
    FOREIGN KEY (id) REFERENCES users (id) ON DELETE CASCADE,
    FOREIGN KEY (department_id) REFERENCES departments (id)
);

CREATE TABLE students (
    id BIGINT PRIMARY KEY,
    student_number NVARCHAR(20) UNIQUE NOT NULL,
    first_name NVARCHAR(100) NOT NULL,
    last_name NVARCHAR(100) NOT NULL,
    email NVARCHAR(100) UNIQUE,
    contact_number NVARCHAR(20),
    date_of_birth DATE,
    address NVARCHAR(MAX),
    profile_picture_url NVARCHAR(MAX),
    
    -- NEW FIELDS
    gender NVARCHAR(20),
    year_level NVARCHAR(20),    -- Using String to allow "Grade 11", "1st Year", etc.
    course_program NVARCHAR(100),
    
    FOREIGN KEY (id) REFERENCES users (id) ON DELETE CASCADE
);

-- NEW TABLE: Student Guardians
CREATE TABLE studentguardians (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    student_id BIGINT NOT NULL,
    guardian_name NVARCHAR(100) NOT NULL,
    contact_number NVARCHAR(20),
    address NVARCHAR(MAX),
    relationship NVARCHAR(50), -- Optional: Father, Mother, etc.
    FOREIGN KEY (student_id) REFERENCES students (id) ON DELETE CASCADE
);

-- ----------------------------------------------------------------------
-- 3. CLASS DEFINITION, ASSIGNMENTS, AND ENROLLMENT
-- ----------------------------------------------------------------------

CREATE TABLE classes (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    subject_id BIGINT NOT NULL,
    school_year_id BIGINT NOT NULL,
    semester_id BIGINT NOT NULL,
    class_section NVARCHAR(50) NOT NULL,
    FOREIGN KEY (subject_id) REFERENCES subjects (id),
    FOREIGN KEY (school_year_id) REFERENCES schoolyears (id),
    FOREIGN KEY (semester_id) REFERENCES semesters (id),
    UNIQUE (subject_id, school_year_id, semester_id, class_section)
);

CREATE TABLE classassignment (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    class_id BIGINT NOT NULL,
    teacher_id BIGINT NOT NULL,
    FOREIGN KEY (class_id) REFERENCES classes (id),
    FOREIGN KEY (teacher_id) REFERENCES teachers (id),
    UNIQUE (class_id, teacher_id)
);

CREATE TABLE classenrollment (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    student_id BIGINT NOT NULL,
    class_id BIGINT NOT NULL,
    enrollment_date DATE NOT NULL,
    
    -- NEW FIELD
    enrollment_status NVARCHAR(20) NOT NULL DEFAULT 'Enrolled',
    CONSTRAINT CK_enrollment_status CHECK (enrollment_status IN ('Enrolled', 'Completed', 'Dropped')),
    
    FOREIGN KEY (student_id) REFERENCES students (id),
    FOREIGN KEY (class_id) REFERENCES classes (id),
    UNIQUE (student_id, class_id)
);

-- ----------------------------------------------------------------------
-- 4. GRADING
-- ----------------------------------------------------------------------

CREATE TABLE grades (
    id BIGINT PRIMARY KEY NOT NULL IDENTITY(1,1),
    enrollment_id BIGINT UNIQUE NOT NULL,
    midterm_grade DECIMAL(5, 2),
    final_grade DECIMAL(5, 2),
    final_score DECIMAL(5, 2),
    FOREIGN KEY (enrollment_id) REFERENCES classenrollment (id)
);