# FullName Property Tests - Quick Reference

## File Location
`ContosoUniversity.Tests/Models/PersonFullNameTests.cs`

## Test Count
**25 comprehensive unit tests** | **All passing** ✅

## Property Details
```csharp
// ContosoUniversity.Core/Models/Person.cs
public string FullName => LastName + ", " + FirstMidName;
```

## Test Summary by Category

| Category | Count | Examples |
|----------|-------|----------|
| Basic Functionality | 3 | Normal case, base class, case preservation |
| Special Characters | 5 | Apostrophes, hyphens, periods, numerics |
| Empty Values | 3 | Empty first, empty last, both empty |
| Whitespace | 4 | Leading, trailing, internal, only whitespace |
| Property Behavior | 3 | Idempotency, reactivity, read-only check |
| Multi-Instance | 3 | Multiple students, cross-class consistency |
| International | 2 | Accented chars, Unicode support |
| Format Validation | 2 | Exact separator format, single chars |

## Most Important Tests

### Core Functionality
```csharp
✅ FullName_WithValidFirstAndLastName_ReturnsFormattedString
   // "Smith, John"

✅ FullName_WithSpecialCharactersInNames_ReturnsCorrectlyFormatted
   // "O'Sullivan-McGill, Marie-Anne"

✅ FullName_MultipleStudentInstances_AllReturnCorrectFormat
   // Tests 3 different students
```

### Edge Cases
```csharp
✅ FullName_WithEmptyFirstName_ReturnsFormattedString
   // "Smith, "

✅ FullName_WithEmptyLastName_ReturnsFormattedString
   // ", John"

✅ FullName_WithLeadingWhitespaceInNames_ReturnsFormattedString
   // "  Smith,   John" (preserves whitespace)
```

### Property Verification
```csharp
✅ FullName_IsComputedProperty_CannotBeSet
   // Verifies property is read-only via reflection

✅ FullName_AfterPropertyChange_ReturnsUpdatedValue
   // Confirms property updates when FirstMidName/LastName change

✅ FullName_VerifyExactSeparatorFormat_HasCommaAndSpace
   // Ensures separator is exactly ", " (comma + space)
```

## Quick Test Execution

### Run All FullName Tests
```bash
dotnet test ContosoUniversity.Tests/ --filter "PersonFullNameTests"
```

### Run Single Test
```bash
dotnet test ContosoUniversity.Tests/ --filter "FullName_WithValidFirstAndLastName_ReturnsFormattedString"
```

### Expected Output
```
Test Run Successful.
Total tests: 25
     Passed: 25
     Failed: 0
Total time: < 500ms
```

## Test Class Hierarchy

```
PersonFullNameTests (xUnit test class)
├── Tests the Person model's FullName property
├── Uses TestPerson (concrete implementation of abstract Person)
└── Covers Student class via inheritance
```

## Key Testing Patterns Used

### 1. Simple Assertion
```csharp
Assert.Equal("Smith, John", person.FullName);
```

### 2. Contains Check
```csharp
Assert.Contains(", ", result);
```

### 3. String Split Verification
```csharp
var parts = result.Split(", ");
Assert.Equal(2, parts.Length);
```

### 4. Property Reflection
```csharp
var property = typeof(TestPerson).GetProperty(nameof(TestPerson.FullName));
Assert.False(property.CanWrite);  // Verify read-only
```

## What Each Test Verifies

| Test | Verifies | Format Expected |
|------|----------|-----------------|
| ValidFirstAndLastName | Normal case | "LastName, FirstMidName" |
| MixedCaseNames | Case preservation | Exactly as provided |
| ApostrophesInNames | Special char handling | "O'Brien, Mary" |
| HyphensInNames | Hyphenated names | "Dupont-Smith, Jean" |
| EmptyFirstName | Edge case | "Smith, " |
| EmptyLastName | Edge case | ", John" |
| BothNamesEmpty | Extreme edge case | ", " |
| LeadingWhitespace | Space preservation | "  Smith,   John" |
| TrailingWhitespace | Space preservation | "Smith  , John  " |
| InternalWhitespace | Multi-word names | "Van Smith, John Paul" |
| OnlyWhitespace | Whitespace only | "   ,    " |
| VeryLongNames | No truncation | Complete string |
| SingleCharacter | Min length | "S, J" |
| MultipleStudents | Each instance | Correct format per student |
| PersonBase | Inheritance | Works on base class |
| CalledMultipleTimes | Idempotency | Same value every time |
| AfterPropertyChange | Reactivity | Updates on value change |
| NumericCharacters | Edge case chars | "Smith 2nd, John 3rd" |
| PeriodsInNames | Abbreviations | "Smith-Jones, J.R." |
| InternationalChars | Accented chars | "García, José" |
| ExactSeparator | Format validation | Exactly ", " verified |
| ComputedProperty | Read-only | CanWrite = false |
| StudentAndPerson | Consistency | Same behavior |
| UnicodeCharacters | Unicode support | "Müller, François" |

## Test Data Examples

### Test Person
```csharp
new TestPerson 
{
    FirstMidName = "John",
    LastName = "Smith",
    FullName => "Smith, John"
}
```

### Test Student
```csharp
new Student 
{
    FirstMidName = "Alexander",
    LastName = "Carson",
    EnrollmentDate = new DateTime(2013, 9, 1),
    FullName => "Carson, Alexander"
}
```

## Usage in Views

```html
<!-- Students/Index.cshtml -->
@foreach (var item in Model)
{
    <td>
        @Html.DisplayFor(modelItem => item.FullName)
    </td>
}
```

## Important Notes

### ✅ What the Property Does
- Concatenates LastName + ", " + FirstMidName
- Returns computed string (no database storage)
- Inherits to all Person-derived classes
- Read-only (computed property)

### ⚠️ Edge Cases Discovered
- **No trimming**: `"  Smith,   John"` preserves spaces
- **No validation**: Accepts any string values
- **Handles empty**: `""` results in just the separator
- **Handles whitespace**: Preserves spaces exactly as provided

### 🔄 Reactive Behavior
```csharp
var person = new TestPerson 
{ 
    FirstMidName = "John", 
    LastName = "Smith" 
};
Assert.Equal("Smith, John", person.FullName);

person.FirstMidName = "Jane";
Assert.Equal("Smith, Jane", person.FullName);  // Updates!
```

## Running in IDE

### Visual Studio
1. Open Test Explorer (Test → Windows → Test Explorer)
2. Search for "PersonFullNameTests"
3. Click "Run" or "Run & Debug"

### Visual Studio Code
```bash
dotnet test --filter "PersonFullNameTests" --logger "console;verbosity=detailed"
```

## Failed Test Troubleshooting

If a test fails:
1. Check the assertion message in output
2. Verify FirstMidName and LastName values
3. Check for whitespace issues
4. Confirm property returns computed value
5. Look for special character encoding issues

## All Tests Pass Criteria

✅ Solution builds: `dotnet build`  
✅ All 81 tests pass (including 25 FullName tests)  
✅ No warnings or errors  
✅ Test coverage: FullName property comprehensively tested  
✅ Execution time: < 500ms for all 25 tests  

---

**Last Updated**: 2026-03-06  
**Status**: Production Ready ✅  
**Framework**: xUnit 2.5.3 | .NET 8.0
