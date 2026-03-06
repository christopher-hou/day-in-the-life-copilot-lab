# ContosoUniversity.Core Project Analysis

## Project Overview
The **ContosoUniversity.Core** project is a Domain-Driven Design (DDD) implementation that encapsulates all domain models, business logic, validation rules, and service interfaces for a university management system. It follows .NET 8.0 best practices with nullable reference types enabled.

**Key Characteristics:**
- **Target Framework:** .NET 8.0
- **Architecture:** Clean Architecture / DDD
- **Key Dependencies:** Microsoft.EntityFrameworkCore 8.0.0
- **Structure:** Models, Interfaces, Validation, ViewModels

---

## 1. Domain Models/Entities

### 1.1 Core Entity Hierarchy

#### **Person (Abstract Base Class)**
- **Purpose:** Base class for individuals in the university system
- **Fields:**
  - `ID` (int) - Primary key
  - `LastName` (string, Required, 50 chars max)
  - `FirstMidName` (string, Required, 50 chars max) - Mapped to `FirstName` in DB
  - `FullName` (computed property) - Read-only derived property: `"LastName, FirstMidName"`
  
- **Validation Rules:**
  - `LastName` and `FirstMidName` are required and have 50-character limits
  - `FirstMidName` has a custom error message

**Inheritance:**
```
Person (abstract)
  ├── Student
  └── Instructor
```

---

#### **Student (extends Person)**
- **Purpose:** Represents a student enrolled in the university
- **Fields:**
  - `EnrollmentDate` (DateTime, Required)
    - Range: 1753-01-01 to 9999-12-31
    - Format: yyyy-MM-dd
    - Mapped to `datetime2` in database
  
- **Navigation Properties:**
  - `Enrollments` → Collection of `Enrollment` entities (0..*)
  
- **Business Rules:**
  - Students must have valid enrollment dates between 1753 and 9999
  - A student can be enrolled in multiple courses

---

#### **Instructor (extends Person)**
- **Purpose:** Represents a teaching staff member
- **Fields:**
  - `HireDate` (DateTime, Required)
    - Range: 1753-01-01 to 9999-12-31
    - Format: yyyy-MM-dd
    - Mapped to `datetime2` in database
  
- **Navigation Properties:**
  - `CourseAssignments` → Collection of `CourseAssignment` entities (0..*)
  - `OfficeAssignment` → Single `OfficeAssignment` entity (0..1, optional)
  
- **Business Rules:**
  - Instructors must have valid hire dates between 1753 and 9999
  - An instructor can teach multiple courses
  - An instructor can have at most one office assignment

---

#### **Course**
- **Purpose:** Represents an academic course offered by a department
- **Fields:**
  - `CourseID` (int, Required) - Primary key, NOT auto-generated
  - `Title` (string, Required, 3-50 chars)
  - `Credits` (int) - Range: 0-5
  - `DepartmentID` (int) - Foreign key to Department
  - `TeachingMaterialImagePath` (string, Optional, 255 chars max)
  
- **Navigation Properties:**
  - `Department` → Single `Department` entity (Required)
  - `Enrollments` → Collection of `Enrollment` entities (0..*)
  - `CourseAssignments` → Collection of `CourseAssignment` entities (0..*)
  
- **Business Rules:**
  - CourseID is manually assigned (not auto-generated)
  - Credits must be between 0 and 5
  - Title must be 3-50 characters long
  - Each course must belong to exactly one department

---

#### **Department**
- **Purpose:** Represents an academic or administrative department
- **Fields:**
  - `DepartmentID` (int) - Primary key, auto-generated
  - `Name` (string, Required, 3-50 chars)
  - `Budget` (decimal, Required) - Range: 0 to 10,000,000
    - Stored as SQL `money` type
  - `StartDate` (DateTime, Required)
    - Format: yyyy-MM-dd
    - Validation: Cannot be more than 5 years in the future
  - `InstructorID` (int, Optional) - Foreign key to administrator
  - `DepartmentType` (string, Optional) - Default: "Academic"
    - Valid values: "Academic", "Administrative", "Research"
  - `RowVersion` (byte[], Optional) - Timestamp for concurrency control
  
- **Navigation Properties:**
  - `Administrator` → Single `Instructor` entity (0..1, optional)
  - `Courses` → Collection of `Course` entities (0..*)
  
- **Custom Validation:**
  - `[DepartmentNameValidation]` - Name must start with a letter, cannot contain: "Test", "Demo", "Sample", "Temp"
  - `[FutureDateValidation(5)]` - StartDate cannot be more than 5 years in the future
  - Budget ranges by type:
    - Academic: $50,000 - $5,000,000
    - Administrative: $100,000 - $10,000,000
    - Research: $200,000 - $20,000,000

- **Business Rules:**
  - Each department must have a name starting with a letter
  - A department can have at most one administrator (an instructor)
  - A department must have a valid start date
  - Budget constraints vary by department type
  - Optimistic concurrency control via `RowVersion`

---

#### **Enrollment**
- **Purpose:** Join entity representing a student's enrollment in a course
- **Fields:**
  - `EnrollmentID` (int) - Primary key, auto-generated
  - `CourseID` (int) - Foreign key to Course (Required)
  - `StudentID` (int) - Foreign key to Student (Required)
  - `Grade` (Grade?, Optional) - Enum: A, B, C, D, F
  
- **Navigation Properties:**
  - `Course` → Single `Course` entity (Required)
  - `Student` → Single `Student` entity (Required)
  
- **Business Rules:**
  - An enrollment links a student to a specific course
  - Grade is optional (null means "No grade")
  - Only valid grades are A, B, C, D, F
  - This is a junction table enabling the many-to-many relationship between Student and Course

---

#### **CourseAssignment**
- **Purpose:** Join entity representing an instructor's assignment to teach a course
- **Fields:**
  - `InstructorID` (int) - Part of composite primary key
  - `CourseID` (int) - Part of composite primary key
  
- **Navigation Properties:**
  - `Instructor` → Single `Instructor` entity (Required)
  - `Course` → Single `Course` entity (Required)
  
- **Business Rules:**
  - An instructor can teach multiple courses
  - A course can be taught by multiple instructors
  - Composite primary key: (InstructorID, CourseID)

---

#### **OfficeAssignment**
- **Purpose:** Represents an office location assigned to an instructor (1:1 relationship)
- **Fields:**
  - `InstructorID` (int) - Primary key (same as FK)
  - `Location` (string, Required, 50 chars max)
  
- **Navigation Properties:**
  - `Instructor` → Single `Instructor` entity (Required)
  
- **Business Rules:**
  - One instructor can have at most one office
  - One office location is assigned to at most one instructor
  - True one-to-one relationship using shared primary key

---

#### **Notification**
- **Purpose:** Domain event/audit entity tracking system operations
- **Fields:**
  - `Id` (int) - Primary key
  - `EntityType` (string, Required, 100 chars) - Type of entity (e.g., "Student", "Course")
  - `EntityId` (string, Required, 50 chars) - ID of the affected entity
  - `Operation` (string, Required, 20 chars) - CREATE, UPDATE, or DELETE
  - `Message` (string, Required, 256 chars) - Human-readable notification message
  - `CreatedAt` (DateTime, Required) - Timestamp of the operation
  - `CreatedBy` (string, Optional, 100 chars) - Username of who performed the operation
  - `IsRead` (bool) - Flag indicating if notification has been read
  - `ReadAt` (DateTime?, Optional) - Timestamp when notification was marked as read
  
- **Business Rules:**
  - Immutable once created (except `IsRead` and `ReadAt`)
  - Tracks all CREATE, UPDATE, DELETE operations on entities
  - Can be marked as read independently
  - Serves as an audit log and event notification system

---

### 1.2 Enums

#### **Grade**
```csharp
public enum Grade
{
    A, B, C, D, F
}
```
- Valid grades for course enrollments
- Represents academic performance levels

#### **EntityOperation**
```csharp
public enum EntityOperation
{
    Create,
    Read,
    Update,
    Delete
}
```
- Represents database operation types
- Used by notification system for audit logging

---

## 2. Entity Relationships Map

### 2.1 Relationship Summary

| From | To | Type | Cardinality | Notes |
|------|----|----|-------------|-------|
| Student | Enrollment | One-to-Many | 1:* | A student enrolls in multiple courses |
| Course | Enrollment | One-to-Many | 1:* | A course has multiple enrolled students |
| Instructor | CourseAssignment | One-to-Many | 1:* | An instructor teaches multiple courses |
| Course | CourseAssignment | One-to-Many | 1:* | A course can be taught by multiple instructors |
| Department | Course | One-to-Many | 1:* | Each course belongs to exactly one department |
| Instructor | OfficeAssignment | One-to-One | 1:0..1 | An instructor may have one office |
| Department | Instructor (Admin) | Zero-or-One-to-One | 0..1:0..1 | A department may have an administrator (who is an instructor) |
| Person | Student | Inheritance | - | Single table inheritance |
| Person | Instructor | Inheritance | - | Single table inheritance |

### 2.2 Detailed Relationship Analysis

#### **Many-to-Many: Student ↔ Course (via Enrollment)**
```
Student (1) ──── (many) Enrollment (many) ──── (1) Course
```
- **Type:** Implicit many-to-many through `Enrollment` join table
- **Payload:** The `Enrollment` entity carries the `Grade` field
- **Navigation:** 
  - From Student: `student.Enrollments` → enumerate courses and grades
  - From Course: `course.Enrollments` → enumerate enrolled students
- **Business Logic:** Represents "Student X is enrolled in Course Y with Grade Z"

#### **Many-to-Many: Instructor ↔ Course (via CourseAssignment)**
```
Instructor (1) ──── (many) CourseAssignment (many) ──── (1) Course
```
- **Type:** Implicit many-to-many through `CourseAssignment` join table
- **Payload:** No additional data (pure join table)
- **Navigation:**
  - From Instructor: `instructor.CourseAssignments` → enumerate taught courses
  - From Course: `course.CourseAssignments` → enumerate teaching instructors
- **Business Logic:** Represents "Instructor X teaches Course Y"

#### **One-to-Many: Department → Course**
```
Department (1) ──── (many) Course
```
- **Type:** Traditional one-to-many
- **Foreign Key:** `Course.DepartmentID` → `Department.DepartmentID`
- **Navigation:**
  - From Department: `department.Courses`
  - From Course: `course.Department`
- **Business Logic:** Each course belongs to exactly one department

#### **One-to-Zero-or-One: Department → Instructor (Administrator)**
```
Department (0..1) ──── (1) Instructor
```
- **Type:** Optional foreign key relationship
- **Foreign Key:** `Department.InstructorID` (nullable) → `Instructor.ID`
- **Navigation:**
  - From Department: `department.Administrator` (nullable)
  - From Instructor: No reverse navigation (not collection)
- **Business Logic:** A department may have an instructor as its administrator; an instructor can administer at most one department

#### **One-to-Zero-or-One: Instructor → OfficeAssignment**
```
Instructor (1) ────(0..1) OfficeAssignment
```
- **Type:** Shared primary key one-to-one relationship
- **Design:** `OfficeAssignment.InstructorID` is both primary key AND foreign key
- **Navigation:**
  - From Instructor: `instructor.OfficeAssignment` (nullable)
  - From OfficeAssignment: `officeAssignment.Instructor`
- **Business Logic:** Each instructor may have zero or one office; each office belongs to exactly one instructor

#### **Inheritance: Person → Student | Instructor**
```
Person (Abstract)
  ├── Student
  └── Instructor
```
- **Type:** Single-table inheritance (STI)
- **EF Core Implementation:** Discriminator column in the database
- **Shared Properties:** `ID`, `LastName`, `FirstMidName`, `FullName`
- **Specialized Properties:**
  - Student: `EnrollmentDate`, `Enrollments`
  - Instructor: `HireDate`, `CourseAssignments`, `OfficeAssignment`

---

## 3. Domain-Driven Design (DDD) Analysis

### 3.1 Aggregates Identified

#### **Aggregate: Department (Aggregate Root)**
- **Root Entity:** `Department`
- **Children:** 
  - `Courses` (owned by department, but referenced by enrollments)
  - Optional `Administrator` (reference to Instructor, not owned)
- **Invariants:**
  - Department must have valid name, budget, and start date
  - Budget ranges enforce business constraints per department type
  - Start date cannot be excessive future date
  - Only one administrator per department
- **Boundary:** Department controls course assignments at the department level
- **Lifetime:** Created/deleted as a unit (cascade delete to courses)

#### **Aggregate: Student (Aggregate Root)**
- **Root Entity:** `Student`
- **Children:** `Enrollments` (owned by student)
- **Invariants:**
  - Student must have valid enrollment date
  - Can enroll in multiple courses
  - Grades are managed through enrollment lifecycle
- **Boundary:** Student lifecycle manages all their enrollments
- **Lifetime:** Created/deleted (enrollments cascade delete)

#### **Aggregate: Instructor (Aggregate Root)**
- **Root Entity:** `Instructor`
- **Children:** 
  - `CourseAssignments` (owned references)
  - `OfficeAssignment` (owned, 0..1)
- **Invariants:**
  - Instructor must have valid hire date
  - Can teach multiple courses
  - Can have at most one office
  - Can administer at most one department
- **Boundary:** Instructor lifecycle manages assignments and office
- **Lifetime:** Created/deleted (cascade delete of assignments)

#### **Aggregate: Course (Aggregate Root)**
- **Root Entity:** `Course`
- **Children:** `Enrollments` (referenced, not owned - owned by Student)
- **Invariants:**
  - Course must have title, credits (0-5), and department
  - Cannot exist without a department
- **Note:** Enrollments are NOT owned by Course; they are co-owned with Student
- **Boundary:** Course data, instructors assigned via CourseAssignment
- **Lifetime:** Created/deleted (cascade delete of enrollments and assignments)

#### **Aggregate: Enrollment (Aggregate Root)**
- **Root Entity:** `Enrollment`
- **Type:** Weak aggregate (depends on Student and Course)
- **Invariants:**
  - Must have valid student and course
  - Grade must be null or one of {A, B, C, D, F}
- **Lifetime:** Created/deleted within Student aggregate operations

#### **Aggregate: OfficeAssignment (Aggregate Root)**
- **Root Entity:** `OfficeAssignment`
- **Type:** Micro-aggregate
- **Invariants:**
  - Must have valid location string
  - One-to-one with instructor
- **Ownership:** Owned by Instructor aggregate

### 3.2 Value Objects

The current design does **NOT explicitly implement value objects** but could benefit from:

**Potential Value Objects:**
1. **Grade** (as-is) - Enum representing student performance
2. **EntityOperation** (as-is) - Enum representing operation types
3. **FullName** - Could be extracted as a value object (currently computed property)
4. **Location** - Could be a value object for office address
5. **DepartmentBudget** - Could be a value object (decimal with validation)

### 3.3 Domain Services

#### **Interfaces Acting as Service Contracts:**

**IRepository<T>**
- **Purpose:** Data access abstraction for all aggregates
- **Responsibility:** Create, Read, Update, Delete operations
- **Methods:**
  - `GetAllAsync()` - Retrieve all entities
  - `GetQueryable()` - Get queryable for filtering/pagination
  - `FindAsync(Expression)` - Find by predicate
  - `GetByIdAsync(id)` - Get by primary key
  - `AddAsync(entity)` - Create new entity
  - `UpdateAsync(entity)` - Modify existing entity
  - `DeleteAsync(entity)` - Remove entity
  - `SaveChangesAsync()` - Persist changes
- **Abstraction Level:** Application Service layer (Infrastructure dependency)

**INotificationService**
- **Purpose:** Domain event/notification system
- **Responsibility:** Publish domain events and track notifications
- **Methods:**
  - `SendNotification(...)` - Synchronous notification
  - `SendNotificationAsync(...)` - Asynchronous notification
  - `ReceiveNotification()` - Synchronous read
  - `ReceiveNotificationAsync()` - Asynchronous read
  - `MarkAsRead(id)` - Mark notification as read
  - Overloads support with/without entity display names
- **Pattern:** Observer/Pub-Sub pattern for domain events

**IFileStorageService**
- **Purpose:** Manage course teaching material images
- **Responsibility:** File upload/download operations
- **Methods:**
  - `UploadFileAsync(stream, fileName, contentType)` - Store file
  - `DeleteFileAsync(fileUrl)` - Remove file
  - `GetFileUrl(fileName)` - Generate access URL
- **Usage:** Store paths in `Course.TeachingMaterialImagePath`

---

## 4. Business Rules & Domain Invariants

### 4.1 Cross-Cutting Domain Invariants

| Invariant | Enforcement | Impact |
|-----------|-------------|--------|
| Enrollments must have valid Student and Course | Foreign keys, NOT NULL | Data integrity |
| CourseID is manually assigned | `DatabaseGeneratedOption.None` | Business logic controls ID assignment |
| Credits range 0-5 | `[Range(0, 5)]` | Academic constraint |
| Department StartDate ≤ 5 years future | `[FutureDateValidation(5)]` | Business planning constraint |
| Department budget by type | Custom validator `DepartmentBudgetValidation` | Financial control |
| Department name validation | `[DepartmentNameValidation]` | Naming standards |
| Dates between 1753-9999 | `[Range(...)]` with custom error | Historical record validity |
| OfficeAssignment 1:1 with Instructor | Shared PK | Physical resource constraint |
| Department may have one Administrator | Optional FK | Management structure |
| Grade only A, B, C, D, F | Enum constraint | Academic standard |

### 4.2 Feature-Specific Invariants

#### **Department Management**
- Budget must align with department type (Academic/Administrative/Research)
- Name cannot contain reserved words: Test, Demo, Sample, Temp
- Name must start with a letter
- Start date is a planning date (up to 5 years future allowed)
- Concurrency control via RowVersion timestamp

#### **Student Management**
- EnrollmentDate captures when the student started
- Students accumulate Enrollments as they register for courses
- Cannot enroll without valid enrollment date

#### **Instructor Management**
- HireDate records when instructor joined the institution
- Can teach multiple courses (many-to-many via CourseAssignment)
- Can have 0..1 office assignment
- Can administer 0..1 department

#### **Course Management**
- CourseID is externally assigned (not auto-generated) - business-defined ID
- Must belong to exactly one department
- Can have 0..many instructors assigned
- Can have 0..many student enrollments

#### **Notification/Audit System**
- Every significant operation (Create, Update, Delete) can trigger notification
- Notifications are immutable once created (audit trail)
- Can be marked as read independently
- Tracks who performed the operation and when

---

## 5. Validation Architecture

### 5.1 Data Annotation Validation

The Core project uses **Microsoft.ComponentModel.DataAnnotations** for declarative validation:

**Attribute Categories:**

1. **Basic Constraints:**
   - `[Required]` - Non-nullable fields
   - `[StringLength(max)]` - String length limits
   - `[StringLength(min, max)]` - Min-max range
   - `[Range(min, max)]` - Numeric range validation

2. **Display & Formatting:**
   - `[Display(Name = "...")]` - UI labels
   - `[DataType(DataType.*)]` - Semantic type hints
   - `[DisplayFormat(...)]` - Output formatting
   - `[Column(TypeName = "...")]` - Database column hints

3. **Custom Validators:**
   - `[FutureDateValidationAttribute]` - Date range constraint (max years in future)
   - `[DepartmentNameValidationAttribute]` - Pattern + reserved words validation
   - `[DepartmentBudgetValidationAttribute]` - Cross-property budget validation

### 5.2 Custom Validation Attributes (In CustomValidationAttributes.cs)

#### **FutureDateValidationAttribute**
```
Validates: DateTime fields
Rule: Date cannot exceed N years in the future
Usage: Department.StartDate, Instructor.HireDate, Student.EnrollmentDate
Parameter: maxYearsInFuture (e.g., 5 years)
```

#### **DepartmentNameValidationAttribute**
```
Validates: Department.Name
Rules:
  1. Must start with a letter
  2. Cannot contain: "Test", "Demo", "Sample", "Temp" (case-insensitive)
```

#### **DepartmentBudgetValidationAttribute**
```
Validates: Department.Budget
Depends on: Department.DepartmentType
Ranges:
  Academic: $50,000 - $5,000,000
  Administrative: $100,000 - $10,000,000
  Research: $200,000 - $20,000,000
```

---

## 6. Entity Relationship Diagram (ERD)

```
┌─────────────────────────────────────────────────────────────────┐
│                        PERSON (Abstract)                         │
│  ┌──────────────┬───────────────────────────────────────────┐   │
│  │ ID (PK)      │ LastName, FirstMidName                    │   │
│  │ FullName (computed)                                      │   │
│  └──────────────┴───────────────────────────────────────────┘   │
└──────────────────┬──────────────────────────┬────────────────────┘
                   │ inherits                  │ inherits
       ┌───────────▼──────┐        ┌──────────▼────────────────┐
       │    STUDENT       │        │    INSTRUCTOR            │
       ├──────────────────┤        ├──────────────────────────┤
       │ ID (FK→Person)   │        │ ID (FK→Person)           │
       │ EnrollmentDate ✓ │        │ HireDate ✓               │
       │                  │        │ 1:* CourseAssignment     │
       │ 1:* Enrollment   │        │ 0..1 OfficeAssignment    │
       │ (Student→Course) │        │ 0..1 Administrator of    │
       └────────┬─────────┘        │     Department           │
                │                  └────────┬──────────────────┘
                │                           │
                │ StudentID (FK)            │ InstructorID (FK)
                │                           │
       ┌────────▼────────────────┐  ┌──────▼──────────────┐
       │    ENROLLMENT           │  │ OFFICE ASSIGNMENT   │
       ├─────────────────────────┤  ├──────────────────────┤
       │ EnrollmentID (PK)       │  │ InstructorID (PK=FK) │
       │ StudentID (FK, PK)      │  │ Location (Required)  │
       │ CourseID (FK, PK)       │  │                      │
       │ Grade (A|B|C|D|F, null) │  └──────────────────────┘
       └────────┬────────────────┘
                │ CourseID (FK)
                │
       ┌────────▼─────────────────────────────────┐
       │         COURSE                           │
       ├──────────────────────────────────────────┤
       │ CourseID (PK, NOT auto-generated)        │
       │ Title (3-50 chars, Required)             │
       │ Credits (0-5)                            │
       │ DepartmentID (FK, Required)              │
       │ TeachingMaterialImagePath (Optional)     │
       │ 1:* Enrollment (Course→Student)         │
       │ 1:* CourseAssignment (Course→Instructor)│
       └────────┬────────────────────────────────┘
                │ DepartmentID (FK)
                │
       ┌────────▼──────────────────────────────────────┐
       │         DEPARTMENT                            │
       ├───────────────────────────────────────────────┤
       │ DepartmentID (PK)                             │
       │ Name (3-50 chars, Required) ✓ [DeptName]    │
       │ Budget (0-10M, Required) ✓ [DeptBudget]    │
       │ StartDate (Required) ✓ [FutureDate(5)]     │
       │ DepartmentType (Academic|Admin|Research)    │
       │ InstructorID (FK, Optional) → Administrator │
       │ RowVersion (Concurrency timestamp)          │
       │ 1:* Course                                   │
       └────────────────────────────────────────────────┘
                │ InstructorID (FK, optional)
                │
                └─────► INSTRUCTOR (Administrator)

Additional Entity:

       ┌────────────────────────────────────────────┐
       │      NOTIFICATION (Audit/Event Log)        │
       ├────────────────────────────────────────────┤
       │ Id (PK)                                     │
       │ EntityType (e.g., "Student")               │
       │ EntityId (e.g., "42")                      │
       │ Operation (CREATE|UPDATE|DELETE)           │
       │ Message (256 chars)                        │
       │ CreatedAt (datetime2)                      │
       │ CreatedBy (Optional username)              │
       │ IsRead (bool)                              │
       │ ReadAt (Optional datetime2)                │
       └────────────────────────────────────────────┘

Connection Legend:
  1:*  = One-to-Many
  0..1 = Zero or One
  ✓    = Has custom validation
  FK   = Foreign Key
  PK   = Primary Key
```

---

## 7. Relationship Cardinality Matrix

```
                  │ Person │ Student │ Instructor │ Course │ Enrollment │ CourseAssign │ Office │ Dept
──────────────────┼────────┼─────────┼────────────┼────────┼────────────┼──────────────┼────────┼──────
Person            │   -    │  1:∞    │    1:∞     │   -    │     -      │      -       │   -    │  -
Student           │        │   -     │     -      │  ∞:∞*  │   1:∞      │      -       │   -    │  -
Instructor        │        │         │     -      │  ∞:∞*  │     -      │   1:∞        │  1:0-1 │ 0-1:1
Course            │        │         │            │    -   │   1:∞      │   1:∞        │   -    │  ∞:1
Enrollment        │        │         │            │        │    -       │      -       │   -    │  -
CourseAssignment  │        │         │            │        │            │      -       │   -    │  -
Office            │        │         │            │        │            │             │   -    │  -
Department        │        │         │            │        │            │             │        │  -

* Indirect many-to-many through junction tables
```

---

## 8. Data Model Characteristics

### 8.1 Key Design Patterns

| Pattern | Implementation | Purpose |
|---------|-----------------|---------|
| **Single Table Inheritance (STI)** | Person → Student/Instructor | Polymorphic domain entities |
| **Junction Tables** | Enrollment, CourseAssignment | Many-to-many relationships |
| **Weak Aggregates** | Enrollment | Short-lived domain entities |
| **Shared Primary Key** | OfficeAssignment | True one-to-one relationship |
| **Nullable Foreign Keys** | Department.InstructorID | Optional relationships |
| **Timestamp Columns** | Department.RowVersion | Optimistic concurrency control |
| **Computed Properties** | Person.FullName | Domain-derived data |
| **Enum Types** | Grade, EntityOperation | Type-safe domain constants |

### 8.2 Database Mapping Specifics

**Column Type Mappings:**
- `DateTime` → `datetime2` (precision to microseconds)
- `decimal` (budget) → `money` (SQL Server financial type)
- `string` → `nvarchar(n)` (Unicode support)
- `byte[]` → `rowversion` (for concurrency)

**Key Generation:**
- Identity/Auto-increment: CourseID, EnrollmentID, DepartmentID, Notification.Id, OfficeAssignment.InstructorID
- Externally assigned: CourseID (business ID)
- Composite: CourseAssignment (InstructorID + CourseID)

---

## 9. Service Interfaces

### 9.1 Generic Repository Pattern

**IRepository<T>** (Generic interface for all aggregates)

```csharp
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    IQueryable<T> GetQueryable();              // For filtering/pagination
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(int id);
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task SaveChangesAsync();
}
```

**Usage Pattern:**
- Injected into application services
- Provides async-all-the-way access
- Supports LINQ querying via `GetQueryable()`
- Supports batch operations via `SaveChangesAsync()`

### 9.2 Notification Service

**INotificationService** - Event notification and audit logging

```csharp
public interface INotificationService
{
    // Overloaded methods for sync and async, with/without display names
    void SendNotification(string entityType, string entityId, 
                         EntityOperation operation, string? userName = null);
    Task SendNotificationAsync(string entityType, string entityId, 
                              EntityOperation operation, string? userName = null);
    
    // With display name (e.g., "Student John Smith")
    void SendNotification(string entityType, string entityId, string? entityDisplayName,
                         EntityOperation operation, string? userName = null);
    Task SendNotificationAsync(string entityType, string entityId, string? entityDisplayName,
                              EntityOperation operation, string? userName = null);
    
    // Read notifications
    Notification? ReceiveNotification();
    Task<Notification?> ReceiveNotificationAsync();
    
    // Mark as read
    void MarkAsRead(int notificationId);
    Task MarkAsReadAsync(int notificationId);
    
    void Dispose();
}
```

### 9.3 File Storage Service

**IFileStorageService** - Manage course teaching material files

```csharp
public interface IFileStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task<bool> DeleteFileAsync(string fileUrl);
    string GetFileUrl(string fileName);
}
```

**Usage:** Store teaching material images for courses

---

## 10. Summary & Key Takeaways

### Architecture Highlights

1. **Clean DDD Structure:**
   - Core layer focuses purely on domain models and business rules
   - No infrastructure dependencies (except EF Core for mapping)
   - Interfaces define contracts for external services

2. **Domain-Driven Aggregates:**
   - Department, Student, Instructor, Course are clear aggregate roots
   - Well-defined boundaries and invariants
   - Rich domain validation through custom attributes

3. **Rich Domain Models:**
   - Computed properties (FullName)
   - Value objects (enums)
   - Inheritance hierarchy for polymorphism

4. **Comprehensive Validation:**
   - Declarative validation via data annotations
   - Custom validators for complex business rules
   - Cross-property validation support

5. **Relationship Complexity:**
   - Multiple many-to-many relationships through junction tables
   - One-to-one sharing primary keys
   - Optional associations (nullable FKs)
   - STI inheritance for Person hierarchy

6. **Service Contracts:**
   - Generic IRepository<T> for data access abstraction
   - INotificationService for event/audit pattern
   - IFileStorageService for media management

7. **Concurrency & Audit:**
   - RowVersion on Department for optimistic concurrency
   - Notification entity for audit trail
   - CreatedBy/CreatedAt tracking

### Design Strengths

✅ Clear separation of concerns (Core has no infrastructure)
✅ Flexible repository pattern for data access
✅ Rich validation through custom attributes
✅ Support for async operations
✅ Well-modeled domain relationships
✅ Audit logging capability
✅ Strong typing with enums
✅ Computed properties for derived data

### Potential Improvements

💡 Extract explicit Value Objects (Location, FullName, DepartmentBudget)
💡 Add domain events to aggregates (IEventPublisher pattern)
💡 More granular result types (Result<T> pattern) for operations
💡 Specification pattern for complex queries
💡 Domain services for cross-aggregate operations
💡 Exception types for specific domain violations

