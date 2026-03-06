# ContosoUniversity.Core - Quick Reference Guide

## Entity Inventory

### Domain Entities (10 total)

#### **Aggregate Roots** (4)
1. **Department** - Academic/Administrative/Research departments
2. **Student** - Enrolled learners  
3. **Instructor** - Teaching staff
4. **Course** - Academic offerings
5. **Enrollment** - Student course registrations (weak aggregate)
6. **OfficeAssignment** - Instructor office locations
7. **Notification** - Audit/event log

#### **Abstract Base**
1. **Person** - Shared properties for Student & Instructor

#### **Enums** (2)
1. **Grade** - A, B, C, D, F
2. **EntityOperation** - Create, Read, Update, Delete

---

## Relationship Quick Map

```
Student ──────(enrolls in)────── Course
  │ 1:*                             │ 1:*
  │                                 │
  └────── Enrollment ──────────────┘
           ├─ StudentID (FK)
           ├─ CourseID (FK)
           └─ Grade (A|B|C|D|F|null)


Instructor ──(teaches)─── Course
   │ 1:*                    │ 1:*
   │                        │
   └─ CourseAssignment ────┘
      ├─ InstructorID (FK)
      └─ CourseID (FK)


Department ────(owns)──── Course
   │ 1:*
   └─ Courses[]
   
   (0..1) may have Administrator: Instructor


Instructor ──(has 0..1)── OfficeAssignment
            Shared PK ensures one-to-one


Person (abstract)
  ├─ Student (EnrollmentDate + Enrollments)
  └─ Instructor (HireDate + CourseAssignments + OfficeAssignment)
```

---

## Entity Properties Cheat Sheet

### Department
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| DepartmentID | int | PK, auto | - |
| Name | string(50) | Required, 3-50 chars, [DeptNameValidation] | No: Test, Demo, Sample, Temp |
| Budget | decimal | Required, 0-10M, [DeptBudgetValidation] | Type-dependent ranges |
| StartDate | datetime2 | Required, [FutureDate(5)] | Max 5 yrs future |
| DepartmentType | string | "Academic", "Administrative", "Research" | Default: "Academic" |
| InstructorID | int | FK (nullable) | Optional administrator |
| RowVersion | byte[] | [Timestamp] | Optimistic concurrency |

### Course
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| CourseID | int | PK, NOT auto-generated | Manually assigned |
| Title | string(50) | Required, 3-50 chars | - |
| Credits | int | 0-5 | - |
| DepartmentID | int | FK, NOT NULL | Required department |
| TeachingMaterialImagePath | string(255) | Optional | - |

### Student
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| ID | int | PK (inherited) | From Person |
| LastName | string(50) | Required | From Person |
| FirstMidName | string(50) | Required | From Person, mapped to FirstName in DB |
| EnrollmentDate | datetime2 | Required, [Range 1753-9999] | Student-specific |
| Enrollments | ICollection | 0..* relationship | Navigation to Enrollment |

### Instructor
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| ID | int | PK (inherited) | From Person |
| LastName | string(50) | Required | From Person |
| FirstMidName | string(50) | Required | From Person |
| HireDate | datetime2 | Required, [Range 1753-9999] | Instructor-specific |
| CourseAssignments | ICollection | 0..* relationship | Many courses |
| OfficeAssignment | single | 0..1 relationship | Optional office |

### Enrollment
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| EnrollmentID | int | PK, auto | - |
| StudentID | int | FK (composite key) | Required |
| CourseID | int | FK (composite key) | Required |
| Grade | Grade? | A, B, C, D, F, null | Nullable for "No grade" |

### CourseAssignment
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| InstructorID | int | PK (part), FK | - |
| CourseID | int | PK (part), FK | - |

### OfficeAssignment
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| InstructorID | int | PK=FK | Shared PK pattern |
| Location | string(50) | Required | - |

### Notification
| Field | Type | Constraints | Special |
|-------|------|-------------|---------|
| Id | int | PK, auto | - |
| EntityType | string(100) | Required | e.g., "Student", "Course" |
| EntityId | string(50) | Required | e.g., "42" |
| Operation | string(20) | Required | CREATE, UPDATE, DELETE |
| Message | string(256) | Required | Human-readable text |
| CreatedAt | datetime2 | Required | Operation timestamp |
| CreatedBy | string(100) | Optional | Username who performed it |
| IsRead | bool | - | Default false |
| ReadAt | datetime2 | Optional | When marked as read |

---

## Validation Rules Summary

| Entity | Field | Validation | Error Message |
|--------|-------|-----------|---------------|
| Person | LastName | Required, ≤50 chars | - |
| Person | FirstMidName | Required, ≤50 chars | "First name cannot be longer than 50 characters." |
| Student | EnrollmentDate | Required, [1753-9999] | "Enrollment date must be between 1753 and 9999" |
| Instructor | HireDate | Required, [1753-9999] | "Hire date must be between 1753 and 9999" |
| Course | CourseID | Required, NOT auto-generated | - |
| Course | Title | Required, 3-50 chars | - |
| Course | Credits | 0-5 range | - |
| Department | Name | Required, 3-50, [DeptName] | "Department name must start with a letter." / Cannot contain Test, Demo, Sample, Temp |
| Department | Budget | 0-10M, [DeptBudget] | "Budget for {Type} must be between {Min} and {Max}" |
| Department | StartDate | Required, [FutureDate(5)] | "Date cannot be more than 5 years in the future." |
| OfficeAssignment | Location | Required, ≤50 chars | - |
| Notification | EntityType | Required, ≤100 chars | - |
| Notification | EntityId | Required, ≤50 chars | - |
| Notification | Operation | Required, ≤20 chars | - |
| Notification | Message | Required, ≤256 chars | - |

---

## Business Rules by Aggregate

### Department Aggregate
- ✓ Name must start with letter and avoid reserved words
- ✓ Budget is constrained by type (Academic/Admin/Research)
- ✓ Start date can't exceed 5 years in future (planning constraint)
- ✓ Can have at most one administrator (Instructor)
- ✓ Optimistic concurrency control via RowVersion
- ✓ All courses belong to exactly one department

### Student Aggregate
- ✓ Must have valid enrollment date (1753-9999)
- ✓ Can enroll in multiple courses
- ✓ Each enrollment has optional grade (A/B/C/D/F or null)
- ✓ Cascading delete: remove student → remove enrollments

### Instructor Aggregate
- ✓ Must have valid hire date (1753-9999)
- ✓ Can teach multiple courses (many-to-many via CourseAssignment)
- ✓ Can have at most one office (one-to-one via shared PK)
- ✓ Can administer at most one department
- ✓ OfficeAssignment is optional

### Course Aggregate
- ✓ CourseID is manually assigned (not auto-generated)
- ✓ Must have title (3-50 chars) and exactly one department
- ✓ Credits range 0-5 (academic constraint)
- ✓ Can have multiple instructors teaching it
- ✓ Can have multiple student enrollments
- ✓ Can have teaching material image path
- ✓ Cascading delete: remove course → remove enrollments & assignments

---

## Service Interfaces

### IRepository<T>
```csharp
// Generic data access abstraction
Task<IEnumerable<T>> GetAllAsync()
IQueryable<T> GetQueryable()                          // For LINQ, pagination
Task<IEnumerable<T>> FindAsync(Expression...)        // By predicate
Task<T?> GetByIdAsync(int id)
Task<T> AddAsync(T entity)
Task UpdateAsync(T entity)
Task DeleteAsync(T entity)
Task SaveChangesAsync()                              // Persist all changes
```

**Used for:** Department, Course, Student, Instructor, Enrollment, Notification

### INotificationService
```csharp
// Domain event/audit system
Task SendNotificationAsync(string entityType, string entityId, 
                          EntityOperation operation, string? userName = null)
Task SendNotificationAsync(string entityType, string entityId, string? displayName,
                          EntityOperation operation, string? userName = null)
Task<Notification?> ReceiveNotificationAsync()
Task MarkAsReadAsync(int notificationId)
```

**Purpose:** Audit trail, event notifications

### IFileStorageService
```csharp
// File management for teaching materials
Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
Task<bool> DeleteFileAsync(string fileUrl)
string GetFileUrl(string fileName)
```

**Used for:** Course.TeachingMaterialImagePath

---

## Architecture Patterns Used

| Pattern | Location | Purpose |
|---------|----------|---------|
| **Single Table Inheritance** | Person → Student/Instructor | Polymorphic domain entities |
| **Composite Keys** | Enrollment, CourseAssignment | Natural keys for junction tables |
| **Shared Primary Key** | OfficeAssignment | True one-to-one enforcement |
| **Nullable Foreign Keys** | Department.InstructorID | Optional relationships |
| **Enum Types** | Grade, EntityOperation | Type-safe constants |
| **Computed Properties** | Person.FullName | Derived domain data |
| **Custom Validators** | Department, various | Complex business rules |
| **Repository Pattern** | IRepository<T> | Data access abstraction |
| **Timestamp Versioning** | Department.RowVersion | Optimistic concurrency |
| **Weak Aggregates** | Enrollment | Dependent entities |

---

## Navigation Patterns

### Load a Student with All Enrollments
```csharp
var student = await studentRepo.GetByIdAsync(1);
// student.Enrollments is populated
// foreach enrollment: enrollment.Course available
```

### Load a Course with Instructors
```csharp
var course = await courseRepo.GetByIdAsync(1001);
// course.CourseAssignments populated
// foreach assignment: assignment.Instructor available
```

### Load a Department with Courses and Students
```csharp
var dept = await deptRepo.GetByIdAsync(1);
// dept.Courses populated
// foreach course: course.Enrollments populated
//    foreach enrollment: enrollment.Student available
```

### Check if Instructor Has Office
```csharp
var instructor = await instructorRepo.GetByIdAsync(1);
if (instructor.OfficeAssignment != null)
{
    var location = instructor.OfficeAssignment.Location;
}
```

---

## Data Annotation Attributes Used

### Built-in
- `[Required]` - Non-nullable fields
- `[StringLength(max)]` - String validation
- `[StringLength(min, max)]` - Range validation
- `[Range(min, max)]` - Numeric validation
- `[Display(Name = "...")]` - UI labels
- `[DataType(DataType.*)]` - Semantic hints
- `[DisplayFormat(...)]` - Formatting
- `[Column(TypeName = "...")]` - DB hints
- `[Key]` - Primary key
- `[ForeignKey(...)]` - FK relationship
- `[Timestamp]` - Concurrency control

### Custom (ContosoUniversity.Core.Validation)
- `[DepartmentNameValidation]` - Pattern + reserved words
- `[FutureDateValidation(years)]` - Max future date range
- `[DepartmentBudgetValidation]` - Type-dependent ranges

---

## Cardinality Summary

| Relationship | Cardinality | Pattern | Notes |
|--------------|-------------|---------|-------|
| Department ↔ Course | 1:* | 1 dept → many courses | Ownership |
| Student ↔ Enrollment | 1:* | 1 student → many enrollments | Ownership |
| Course ↔ Enrollment | 1:* | 1 course → many enrollments | Tracking |
| Student ↔ Course | *:* | Via Enrollment junction | Many-to-many |
| Instructor ↔ Course | *:* | Via CourseAssignment | Many-to-many |
| Instructor ↔ Office | 1:0..1 | Via shared PK | Exclusive assignment |
| Department ↔ Admin | 0..1:1 | Nullable FK | Optional |
| Person ↔ Student | 1:1 | Inheritance (STI) | Specialization |
| Person ↔ Instructor | 1:1 | Inheritance (STI) | Specialization |

---

## Query Examples

### Get All Students with Their Enrollments
```csharp
var students = await studentRepo.GetAllAsync();
// students[i].Enrollments[] available if eager-loaded
```

### Find Courses by Department
```csharp
var courses = await courseRepo.FindAsync(c => c.DepartmentID == 1);
```

### Get Instructor's Courses
```csharp
var instructor = await instructorRepo.GetByIdAsync(1);
var courses = instructor.CourseAssignments.Select(ca => ca.Course);
```

### Find Students Enrolled in Specific Course
```csharp
var course = await courseRepo.GetByIdAsync(1001);
var students = course.Enrollments.Select(e => e.Student);
```

---

## Concurrency & Integrity

### Optimistic Concurrency
- **Mechanism**: Department.RowVersion ([Timestamp])
- **Behavior**: DB auto-updates on change
- **Exception**: ConcurrencyException on SaveChangesAsync() if RowVersion differs

### Cascade Operations
- **Delete Department** → Deletes all its Courses
- **Delete Course** → Deletes all Enrollments & CourseAssignments
- **Delete Student** → Deletes all Enrollments
- **Delete Instructor** → Deletes CourseAssignments & OfficeAssignment

### Constraints
- Courses require Department (NOT NULL FK)
- Enrollments require Student & Course (NOT NULL FKs)
- OfficeAssignment is one-to-one via shared PK

---

## Technology Stack

- **Framework**: .NET 8.0
- **ORM**: Entity Framework Core 8.0.0
- **Validation**: System.ComponentModel.DataAnnotations
- **Architecture**: Clean Architecture / Domain-Driven Design
- **Features**: Nullable reference types enabled, async-all-the-way

---

## File Structure

```
ContosoUniversity.Core/
├── Models/
│   ├── Person.cs (abstract base)
│   ├── Student.cs (extends Person)
│   ├── Instructor.cs (extends Person)
│   ├── Course.cs
│   ├── Department.cs
│   ├── Enrollment.cs
│   ├── CourseAssignment.cs
│   ├── OfficeAssignment.cs
│   ├── Notification.cs
│   └── EntityOperation.cs (enum)
│   └── Grade.cs (enum)
├── Interfaces/
│   ├── IRepository.cs
│   ├── INotificationService.cs
│   └── IFileStorageService.cs
├── Validation/
│   └── CustomValidationAttributes.cs
│       ├── FutureDateValidationAttribute
│       ├── DepartmentBudgetValidationAttribute
│       └── DepartmentNameValidationAttribute
└── ViewModels/ (placeholder)
```

---

## Key Statistics

- **Entities**: 9 (Person abstract + 8 concrete)
- **Aggregates**: 7 (Department, Student, Instructor, Course, Enrollment, OfficeAssignment, Notification)
- **Enums**: 2 (Grade, EntityOperation)
- **Relationships**: 
  - 1:Many: 4
  - Many:Many: 2 (via junction tables)
  - 1:ZeroOrOne: 2
  - Inheritance: 2
- **Validation Attributes**: 20+ (built-in + custom)
- **Service Interfaces**: 3
- **Custom Validators**: 3

