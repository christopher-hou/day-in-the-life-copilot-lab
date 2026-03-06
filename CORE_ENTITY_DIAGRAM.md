# ContosoUniversity Domain Model - Visual Diagrams

## Quick Reference: Entity Listing

| Entity | Type | Key Properties | Navigation | Aggregate Root |
|--------|------|-----------------|------------|-----------------|
| **Person** | Abstract Base | ID, LastName, FirstMidName | - | - |
| **Student** | Entity (extends Person) | EnrollmentDate | Enrollments[*] | ✓ Yes |
| **Instructor** | Entity (extends Person) | HireDate | CourseAssignments[*], OfficeAssignment | ✓ Yes |
| **Course** | Entity | CourseID, Title, Credits | Department, Enrollments[*], CourseAssignments[*] | ✓ Yes |
| **Department** | Entity | DepartmentID, Name, Budget | Administrator, Courses[*] | ✓ Yes |
| **Enrollment** | Junction/Weak Entity | CourseID, StudentID | Student, Course | ✓ (Weak) |
| **CourseAssignment** | Junction | InstructorID, CourseID | Instructor, Course | ✗ No |
| **OfficeAssignment** | Entity | InstructorID | Instructor | ✓ Yes |
| **Notification** | Event/Audit Log | Id, EntityType, Operation | - | ✓ Yes |

---

## Relationship Diagrams

### 1. Student ↔ Course Many-to-Many (via Enrollment)

```
┌──────────────┐                    ┌──────────────┐
│   STUDENT    │                    │    COURSE    │
├──────────────┤                    ├──────────────┤
│ ID (PK)      │                    │ CourseID(PK) │
│ EnrollDate   │                    │ Title        │
│ LastName     │                    │ Credits 0-5  │
│ FirstMidName │                    │ DeptID (FK)  │
└──────────────┘                    └──────────────┘
       │                                    │
       │ StudentID (FK)    CourseID (FK)   │
       │                                    │
       └────────────┬─────────────────────┘
                    │
             ┌──────▼──────────┐
             │   ENROLLMENT    │
             ├─────────────────┤
             │ EnrollmentID(PK)│
             │ StudentID (FK)  │
             │ CourseID (FK)   │
             │ Grade: A|B|C|D|F│
             └─────────────────┘

Cardinality: Student (1) --- (*) Enrollment --- (1) Course
Many Students → One Enrollment per (Student, Course) pair → One Course
```

**Key Properties:**
- **Payload**: Grade (A, B, C, D, F or null)
- **Composite Key**: (StudentID, CourseID)
- **Usage**: Track student course registrations and performance

---

### 2. Instructor ↔ Course Many-to-Many (via CourseAssignment)

```
┌──────────────┐                    ┌──────────────┐
│ INSTRUCTOR   │                    │    COURSE    │
├──────────────┤                    ├──────────────┤
│ ID (PK)      │                    │ CourseID(PK) │
│ HireDate     │                    │ Title        │
│ LastName     │                    │ Credits      │
│ FirstMidName │                    │ DeptID (FK)  │
└──────────────┘                    └──────────────┘
       │                                    │
       │ InstructorID (FK)  CourseID (FK)  │
       │                                    │
       └────────────┬─────────────────────┘
                    │
         ┌──────────▼────────────┐
         │ COURSEASSIGNMENT      │
         ├──────────────────────┤
         │ InstructorID (PK, FK)│
         │ CourseID (PK, FK)    │
         └──────────────────────┘

Cardinality: Instructor (1) --- (*) CourseAssignment --- (1) Course
Many Instructors → One per (Instructor, Course) → One Course
```

**Key Properties:**
- **Payload**: None (pure junction table)
- **Composite Key**: (InstructorID, CourseID)
- **Usage**: Track course teaching assignments

---

### 3. Department → Course One-to-Many

```
┌──────────────────────────────────┐
│         DEPARTMENT               │
├──────────────────────────────────┤
│ DepartmentID (PK)                │
│ Name (3-50 chars) ✓ validate     │
│ Budget (0-10M) ✓ validate        │
│ StartDate ✓ max 5 yrs future     │
│ DepartmentType: Academic|Admin   │
│ InstructorID (FK, optional)      │
│ RowVersion (concurrency)         │
└──────────────────────────────────┘
            │ (1)
            │ DepartmentID (FK)
            │ (*)
            ▼
┌──────────────────────────────┐
│         COURSE               │
├──────────────────────────────┤
│ CourseID (PK)                │
│ Title (3-50 chars)           │
│ Credits (0-5)                │
│ DepartmentID (FK)            │
│ TeachingMaterialImagePath    │
└──────────────────────────────┘

Cardinality: Department (1) --- (*) Course
One Department → Many Courses
Cascade: Delete department → Delete courses
```

**Key Properties:**
- **Foreign Key**: Course.DepartmentID → Department.DepartmentID
- **Constraint**: Course MUST have a department (NOT NULL FK)
- **Usage**: Organize courses by academic department

---

### 4. Department ← Instructor One-to-Zero-or-One (Administrator)

```
┌──────────────────────────────────┐
│         DEPARTMENT               │
├──────────────────────────────────┤
│ DepartmentID (PK)                │
│ Name                             │
│ Budget                           │
│ StartDate                        │
│ InstructorID (FK, optional)      │◄─────┐
│ RowVersion                       │      │ (0..1)
└──────────────────────────────────┘      │
                                          │ InstructorID (FK)
                                    ┌─────────────────────┐
                                    │   INSTRUCTOR        │
                                    ├─────────────────────┤
                                    │ ID (PK)             │
                                    │ HireDate            │
                                    │ LastName            │
                                    │ FirstMidName        │
                                    └─────────────────────┘

Cardinality: Department (0..1) --- (1) Instructor
Zero or One Department → One Instructor can administer it
One Instructor → Can administer at most one department
```

**Key Properties:**
- **Foreign Key**: Department.InstructorID → Instructor.ID (NULLABLE)
- **Constraint**: Optional (FK can be NULL)
- **Usage**: Track department administrator

---

### 5. Instructor → OfficeAssignment True One-to-One

```
┌──────────────────────┐
│    INSTRUCTOR        │
├──────────────────────┤
│ ID (PK)              │
│ HireDate             │
│ LastName             │
│ FirstMidName         │
└──────────────────────┘
       │ (1)
       │ InstructorID (shared PK)
       │ (0..1)
       ▼
┌──────────────────────────┐
│ OFFICEASSIGNMENT         │
├──────────────────────────┤
│ InstructorID (PK=FK)     │  ◄─── Shared primary key!
│ Location (50 chars)      │
└──────────────────────────┘

Cardinality: Instructor (1) --- (0..1) OfficeAssignment
One Instructor → Zero or One Office
```

**Key Properties:**
- **Design Pattern**: Shared Primary Key (InstructorID is PK in both)
- **Constraint**: One-to-one via database design (not by business logic)
- **Optional**: Instructor may not have an office
- **Usage**: Assign office locations to instructors

---

### 6. Person Inheritance Hierarchy (Single Table Inheritance)

```
                    ┌──────────────────────┐
                    │      PERSON          │
                    │   (Abstract Base)    │
                    ├──────────────────────┤
                    │ ID (PK)              │
                    │ LastName (50 chars)  │
                    │ FirstMidName (50)    │
                    │ FullName (computed)  │
                    │ Discriminator (STI)  │
                    └──────────────────────┘
                           │
                ┌──────────┴──────────┐
                │                     │
           ┌────▼─────────┐    ┌──────▼────────┐
           │   STUDENT    │    │  INSTRUCTOR   │
           ├──────────────┤    ├───────────────┤
           │ EnrollmentDt │    │ HireDate      │
           │ Enrollments[]│    │ CourseAssign[]│
           │              │    │ OfficeAssign  │
           └──────────────┘    └───────────────┘

Inheritance: Single Table Inheritance (STI)
All rows in PERSON table (Discriminator column indicates type)
```

**Key Properties:**
- **Pattern**: Single Table Inheritance
- **Discriminator**: Column in PERSON table (implicit in EF Core)
- **Shared Fields**: ID, LastName, FirstMidName
- **Specialized Fields**:
  - Student: EnrollmentDate, collection of Enrollments
  - Instructor: HireDate, collection of CourseAssignments, OfficeAssignment

---

## Complete Database Schema Diagram

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                 DATABASE SCHEMA                                 │
└─────────────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────┐
│ PERSON                   │         Discriminator-based inheritance
│ (Single Table STI)       │         Maps both Student & Instructor
├──────────────────────────┤
│ ID: int PK               │◄───────┐
│ LastName: nvarchar(50)✓  │        │
│ FirstMidName: nvarchar   │        │
│ EnrollmentDate: datetime2│        │ Student-specific
│ HireDate: datetime2      │        │ Instructor-specific
│ Discriminator: nvarchar  │        │
└──────────────────────────┘        │
                                     │
                    ┌────────────────┴────────────────┐
                    │                                 │
            ┌───────▼──────────┐           ┌─────────▼────────┐
            │ STUDENT (VIEW)   │           │INSTRUCTOR (VIEW) │
            └──────────────────┘           └──────────────────┘


┌──────────────────────────┐
│ DEPARTMENT               │
├──────────────────────────┤
│ DepartmentID: int PK     │◄─────────────────┐
│ Name: nvarchar(50)✓      │                  │
│ Budget: money✓           │        ┌─────────┤ Department.InstructorID (FK)
│ StartDate: datetime2✓    │        │         │
│ DepartmentType: nvarchar │        │         │
│ InstructorID: int FK (0,1)├──────┐│         │
│ RowVersion: rowversion   │      ││         │
└──────────────────────────┘      ││    ┌────▼─────────────────┐
         │                        ││    │ PERSON               │
         │ (1)                    ││    │ (INSTRUCTOR)         │
         │ DepartmentID (FK)      ││    ├─────────────────────┤
         │ (*)                    ││    │ ID: int PK           │
         │                        ││    │ ... (via inheritance)│
    ┌────▼──────────────┐        ││    └──────────┬──────────┘
    │ COURSE            │        ││               │
    ├───────────────────┤        ││               │ FK
    │ CourseID: int PK  │        ││    ┌──────────┘
    │  (NOT auto-gen)   │        ││    │
    │ Title: nvarchar   │        ││    │ Optional Admin
    │ Credits: int (0-5)│        ││    │
    │ DeptID: int FK ───┼────────┘│    │
    │ TeachingMaterial  │         │    │
    └────────┬──────────┘         │    │
             │ (1)                │    │
             │ CourseID (FK)      │    │
             │ (*)                │    │ (1)
             │                    │    │ InstructorID (FK)
    ┌────────▼──────────────────┐ │    │
    │ ENROLLMENT                │ │ ┌──┴─────────────────┐
    ├───────────────────────────┤ │ │ OFFICEASSIGNMENT  │
    │ EnrollmentID: int PK      │ │ ├───────────────────┤
    │ StudentID: int FK ────┐   │ │ │ InstructorID: int │
    │ CourseID: int FK ─────┼───┤ │ │  PK=FK            │
    │ Grade: enum (null ok) │   │ │ │ Location: nvarchar│
    └───────────────────────┼───┘ │ └──────────┬────────┘
                    (*)            │ (0..1)    │
                    │              │           │
                    │              │     Shared PK ensures 1:0..1
                    │              │
                    ├──────────────┴───────────┘
                    │
           ┌────────▼──────────────────┐
           │ NOTIFICATION (Audit Log)  │
           ├───────────────────────────┤
           │ Id: int PK                │
           │ EntityType: nvarchar(100) │
           │ EntityId: nvarchar(50)    │
           │ Operation: nvarchar(20)   │
           │ Message: nvarchar(256)    │
           │ CreatedAt: datetime2      │
           │ CreatedBy: nvarchar(100)  │
           │ IsRead: bit               │
           │ ReadAt: datetime2 (null)  │
           └───────────────────────────┘
```

---

## Navigation Path Examples

### Get a Student's Course Grades

```csharp
// Path: Student → Enrollments → Course + Grade
var student = await studentRepo.GetByIdAsync(1);
var enrollments = student.Enrollments;  // Collection of Enrollments

foreach (var enrollment in enrollments)
{
    var course = enrollment.Course;      // Navigate to Course
    var grade = enrollment.Grade;        // Get Grade (A, B, C, D, F, or null)
    Console.WriteLine($"{course.Title}: {grade}");
}
```

### Get a Course's Instructors

```csharp
// Path: Course → CourseAssignments → Instructor
var course = await courseRepo.GetByIdAsync(1001);
var instructors = course.CourseAssignments
    .Select(ca => ca.Instructor)
    .ToList();

foreach (var instructor in instructors)
{
    Console.WriteLine($"Instructor: {instructor.FullName}");
}
```

### Get a Department's Courses and Students

```csharp
// Path: Department → Courses → Enrollments → Student
var department = await deptRepo.GetByIdAsync(1);

foreach (var course in department.Courses)
{
    Console.WriteLine($"Course: {course.Title}");
    
    foreach (var enrollment in course.Enrollments)
    {
        var student = enrollment.Student;
        Console.WriteLine($"  Student: {student.FullName}");
    }
}
```

### Get an Instructor's Office Location

```csharp
// Path: Instructor → OfficeAssignment → Location
var instructor = await instructorRepo.GetByIdAsync(1);

if (instructor.OfficeAssignment != null)
{
    Console.WriteLine($"Office: {instructor.OfficeAssignment.Location}");
}
else
{
    Console.WriteLine("No office assigned");
}
```

---

## Key Relationship Patterns

### Pattern 1: Many-to-Many with Payload
**Example**: Student ↔ Course via Enrollment

```
Student (1) ──┬── (*) Enrollment (*)── (1) Course
              │
         Contains Grade field
         Makes it a rich association
```

- Use junction table with additional properties
- Navigable from both sides
- Can carry domain data (Grade)

### Pattern 2: Many-to-Many Pure Junction
**Example**: Instructor ↔ Course via CourseAssignment

```
Instructor (1) ──┬── (*) CourseAssignment (*) ── (1) Course
                 │
         No additional properties
         Just pure linking
```

- Minimal junction table (only FKs)
- Navigable from both sides
- No domain-specific data in junction

### Pattern 3: Weak Aggregate
**Example**: Enrollment depends on both Student and Course

```
Student ──┐     ┌── Course
          │     │
          └──Enrollment──┘
```

- Has a composite key (StudentID, CourseID)
- Cannot exist independently
- Tightly bound to both parents

### Pattern 4: Optional One-to-One
**Example**: Instructor ↔ OfficeAssignment

```
Instructor (1) ──(0..1) OfficeAssignment
                   │
              Shared PK ensures uniqueness
```

- Enforced through shared primary key
- Optional (instructor may not have office)
- One-direction navigation (from Instructor)

### Pattern 5: Optional Foreign Key
**Example**: Department → Instructor (Administrator)

```
Department (0..1) ──(1) Instructor
       │
Nullable FK allows null
```

- FK is nullable
- Zero or one Department per Instructor
- One Instructor → Many Students (but one as admin of a Dept)

---

## Aggregate Boundary Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                     DEPARTMENT AGGREGATE                        │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────────┐     ┌──────────────────────┐     │
│  │   DEPARTMENT (Root)     │     │  COURSE 1            │     │
│  │  - Controls lifecycle   │     │  - Belongs to Dept   │     │
│  │  - Has invariants       │◄────┤  - Owned by Dept     │     │
│  │  - Enforces rules       │     │  - Enrollments       │     │
│  │  - May have Admin       │     │  - CourseAssign[*]  │     │
│  │  - Timestamp versioning │     └──────────────────────┘     │
│  └───┬────────────────────┘                                    │
│      │                           ┌──────────────────────┐     │
│      │                           │  COURSE 2            │     │
│      └──────────────────────────┤  ...                 │     │
│                                 └──────────────────────┘     │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                      STUDENT AGGREGATE                           │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────┐     ┌────────────────────┐           │
│  │  STUDENT (Root)      │     │  ENROLLMENT 1      │           │
│  │  - Controls lifecycle├────┤  - Links to Course1│           │
│  │  - Has EnrollmentDt  │     │  - Stores Grade    │           │
│  │  - Manages enroll.   │     │  - Weak aggregate  │           │
│  └──┬───────────────────┘     └────────────────────┘           │
│     │                                                            │
│     │                     ┌────────────────────┐               │
│     └────────────────────┤  ENROLLMENT 2      │               │
│                          │  - Links to Course2│               │
│                          │  - Stores Grade    │               │
│                          └────────────────────┘               │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                     INSTRUCTOR AGGREGATE                         │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────┐    ┌──────────────────┐              │
│  │ INSTRUCTOR (Root)    │    │ COURSEASSIGN 1   │              │
│  │ - Controls lifecycle ├───┤ - Teaches Course1│              │
│  │ - Has HireDate       │    │ - Pure link      │              │
│  │ - Manages courses    │    └──────────────────┘              │
│  │ - May have office    │                                      │
│  └──┬────────────────────┘    ┌──────────────────┐              │
│     │                         │ COURSEASSIGN 2   │              │
│     │                    ┌────┤ - Teaches Course2│              │
│     │                    │    └──────────────────┘              │
│     │            ┌───────┘                                      │
│     │            │                                              │
│     └───────┐    │   ┌──────────────────────┐                  │
│             ▼    └──┤ OFFICEASSIGNMENT     │                  │
│          (at most one office)  │ - Location       │                  │
│                    │ - 1:1 with Instr.   │                  │
│                    └──────────────────────┘                  │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                      COURSE AGGREGATE                            │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────┐    ┌──────────────────┐              │
│  │   COURSE (Root)      │    │ ENROLLMENT 1     │              │
│  │ - CourseID (manual)  ├───┤ - Student linked │              │
│  │ - Must have Dept     │    │ - Grade recorded │              │
│  │ - Credits 0-5        │    └──────────────────┘              │
│  │ - Title required     │                                      │
│  │ - Teaching material  │    ┌──────────────────┐              │
│  └──────────────────────┤    │ COURSEASSIGN 1   │              │
│                         ├───┤ - Instructor     │              │
│                         │    │ - Teaching link  │              │
│                         │    └──────────────────┘              │
│                         │                                      │
│                         ├───┐  ┌──────────────────┐           │
│                         │   └─┤ COURSEASSIGN 2   │           │
│                         │     │ - Another Instr. │           │
│                         │     └──────────────────┘           │
│                         └────────────┐ Belongs to DEPARTMENT  │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│                   NOTIFICATION AGGREGATE                         │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │ NOTIFICATION (Root)                                      │  │
│  │ - Immutable audit log entry                             │  │
│  │ - EntityType (e.g., "Student", "Course")               │  │
│  │ - EntityId (primary key of entity)                      │  │
│  │ - Operation (CREATE, UPDATE, DELETE)                   │  │
│  │ - Message (human readable)                             │  │
│  │ - CreatedAt, CreatedBy                                 │  │
│  │ - IsRead flag (can change independently)               │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

---

## Concurrency & Data Integrity

### Optimistic Concurrency Control

```
Department Table:
  - RowVersion: byte[] [Timestamp]
  - Database automatically updates on any change
  - EF Core compares RowVersion before SaveChangesAsync()
  - Throws ConcurrencyException if RowVersion differs
  
Usage Pattern:
  1. Load: dept = await repo.GetByIdAsync(1)  // RowVersion captured
  2. Modify: dept.Budget = 500000              // Change value
  3. Save: await repo.SaveChangesAsync()       // RowVersion checked
  4. If RowVersion changed in DB → ConcurrencyException
```

### Foreign Key Integrity

```
Cascade Rules:
  - Department → Courses: DELETE CASCADE
  - Student → Enrollments: DELETE CASCADE
  - Instructor → CourseAssignments: DELETE CASCADE
  - Instructor → OfficeAssignment: DELETE CASCADE
  - Department → Instructor (Admin): NO ACTION (optional FK)

Constraints:
  - Course.DepartmentID: NOT NULL → FK enforcement
  - Department.InstructorID: Nullable → Optional admin
  - Enrollment: NOT NULL for both FKs
```

---

## Summary Table: Relationship Types

| Entities | Type | Cardinality | Pattern | Notes |
|----------|------|-------------|---------|-------|
| Department ↔ Course | 1:Many | 1:* | Containment | Department owns courses |
| Student ↔ Enrollment | 1:Many | 1:* | Aggregation | Student owns enrollments |
| Course ↔ Enrollment | 1:Many | 1:* | Aggregation | Course tracks enrollments |
| Student ↔ Course | Many:Many | *:* | Junction (Enrollment) | Via junction table |
| Instructor ↔ Course | Many:Many | *:* | Junction (CourseAssignment) | Via junction table |
| Instructor ↔ Office | 1:ZeroOrOne | 1:0..1 | Shared PK | Exclusive assignment |
| Department ↔ Admin | ZeroOrOne:One | 0..1:1 | Nullable FK | Optional administrator |
| Person → Student | Inheritance | - | Single Table | STI pattern |
| Person → Instructor | Inheritance | - | Single Table | STI pattern |

