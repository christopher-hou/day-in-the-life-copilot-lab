using Xunit;
using ContosoUniversity.Core.Models;

namespace ContosoUniversity.Tests.Models
{
    /// <summary>
    /// Comprehensive unit tests for the FullName computed property in the Person model.
    /// FullName is inherited by Student and other Person-derived classes.
    /// 
    /// Format: "LastName, FirstMidName"
    /// Example: "Smith, John"
    /// </summary>
    public class PersonFullNameTests
    {
        /// <summary>
        /// Test: Normal case with valid first and last names
        /// Expected: Format should be "LastName, FirstMidName"
        /// </summary>
        [Fact]
        public void FullName_WithValidFirstAndLastName_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "John",
                LastName = "Smith"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Smith, John", result);
        }

        /// <summary>
        /// Test: Valid names with different cases
        /// Expected: Format should preserve case exactly as provided
        /// </summary>
        [Fact]
        public void FullName_WithMixedCaseNames_ReturnsCorrectlyFormatted()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "elizabeth",
                LastName = "ANDERSON"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("ANDERSON, elizabeth", result);
        }

        /// <summary>
        /// Test: Names containing apostrophes
        /// Expected: Format should handle special characters correctly
        /// </summary>
        [Fact]
        public void FullName_WithApostrophesInNames_ReturnsCorrectlyFormatted()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "Mary Anne",
                LastName = "O'Brien"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("O'Brien, Mary Anne", result);
        }

        /// <summary>
        /// Test: Names containing hyphens
        /// Expected: Format should handle hyphenated names correctly
        /// </summary>
        [Fact]
        public void FullName_WithHyphensInNames_ReturnsCorrectlyFormatted()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "Jean-Marie",
                LastName = "Dupont-Smith"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Dupont-Smith, Jean-Marie", result);
        }

        /// <summary>
        /// Test: Multiple special characters combined
        /// Expected: Format should handle multiple special characters
        /// </summary>
        [Fact]
        public void FullName_WithSpecialCharactersInNames_ReturnsCorrectlyFormatted()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "Marie-Anne",
                LastName = "O'Sullivan-McGill"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("O'Sullivan-McGill, Marie-Anne", result);
        }

        /// <summary>
        /// Test: Empty first name edge case
        /// Expected: Format should still include separator, resulting in "LastName, "
        /// </summary>
        [Fact]
        public void FullName_WithEmptyFirstName_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "",
                LastName = "Smith"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Smith, ", result);
        }

        /// <summary>
        /// Test: Empty last name edge case
        /// Expected: Format should still include separator, resulting in ", FirstMidName"
        /// </summary>
        [Fact]
        public void FullName_WithEmptyLastName_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "John",
                LastName = ""
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal(", John", result);
        }

        /// <summary>
        /// Test: Both names empty
        /// Expected: Format should return just the separator ", "
        /// </summary>
        [Fact]
        public void FullName_WithBothNamesEmpty_ReturnsJustSeparator()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "",
                LastName = ""
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal(", ", result);
        }

        /// <summary>
        /// Test: Names with leading whitespace
        /// Expected: Format should preserve whitespace as-is (no trimming)
        /// </summary>
        [Fact]
        public void FullName_WithLeadingWhitespaceInNames_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "  John",
                LastName = "  Smith"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("  Smith,   John", result);
        }

        /// <summary>
        /// Test: Names with trailing whitespace
        /// Expected: Format should preserve whitespace as-is (no trimming)
        /// </summary>
        [Fact]
        public void FullName_WithTrailingWhitespaceInNames_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "John  ",
                LastName = "Smith  "
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Smith  , John  ", result);
        }

        /// <summary>
        /// Test: Names with internal whitespace
        /// Expected: Format should preserve internal whitespace correctly
        /// </summary>
        [Fact]
        public void FullName_WithInternalWhitespaceInNames_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "John Paul",
                LastName = "Van Smith"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Van Smith, John Paul", result);
        }

        /// <summary>
        /// Test: Only whitespace in names
        /// Expected: Format should return whitespace as-is
        /// </summary>
        [Fact]
        public void FullName_WithOnlyWhitespaceInNames_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "   ",
                LastName = "   "
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("   ,    ", result);
        }

        /// <summary>
        /// Test: Very long names
        /// Expected: Format should handle long names without truncation
        /// </summary>
        [Fact]
        public void FullName_WithVeryLongNames_ReturnsCompleteFormattedString()
        {
            // Arrange
            var longFirstName = "Christopher Alexander";
            var longLastName = "Schwarzenegger-Wellington-Smith";
            var person = new TestPerson
            {
                FirstMidName = longFirstName,
                LastName = longLastName
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal($"{longLastName}, {longFirstName}", result);
        }

        /// <summary>
        /// Test: Single character names
        /// Expected: Format should handle single characters correctly
        /// </summary>
        [Fact]
        public void FullName_WithSingleCharacterNames_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "J",
                LastName = "S"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("S, J", result);
        }

        /// <summary>
        /// Test: Multiple Student instances with different names
        /// Expected: Each instance should return its own correctly formatted full name
        /// </summary>
        [Fact]
        public void FullName_MultipleStudentInstances_AllReturnCorrectFormat()
        {
            // Arrange
            var student1 = new Student
            {
                FirstMidName = "Alexander",
                LastName = "Carson",
                EnrollmentDate = new DateTime(2013, 9, 1)
            };

            var student2 = new Student
            {
                FirstMidName = "Meredith",
                LastName = "Alonso",
                EnrollmentDate = new DateTime(2012, 9, 1)
            };

            var student3 = new Student
            {
                FirstMidName = "Arturo",
                LastName = "Anand",
                EnrollmentDate = new DateTime(2013, 9, 1)
            };

            // Act
            var result1 = student1.FullName;
            var result2 = student2.FullName;
            var result3 = student3.FullName;

            // Assert
            Assert.Equal("Carson, Alexander", result1);
            Assert.Equal("Alonso, Meredith", result2);
            Assert.Equal("Anand, Arturo", result3);
        }

        /// <summary>
        /// Test: Person base class instances
        /// Expected: Base class should also have functioning FullName property
        /// </summary>
        [Fact]
        public void FullName_WithPersonBaseClass_ReturnsCorrectFormat()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "Michael",
                LastName = "Johnson"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Johnson, Michael", result);
        }

        /// <summary>
        /// Test: FullName returns same value on multiple accesses
        /// Expected: Property should be idempotent
        /// </summary>
        [Fact]
        public void FullName_CalledMultipleTimes_ReturnsSameValue()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "Patricia",
                LastName = "Brown"
            };

            // Act
            var result1 = person.FullName;
            var result2 = person.FullName;
            var result3 = person.FullName;

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
            Assert.Equal("Brown, Patricia", result1);
        }

        /// <summary>
        /// Test: FullName updates when properties change
        /// Expected: Property should reflect current values of FirstMidName and LastName
        /// </summary>
        [Fact]
        public void FullName_AfterPropertyChange_ReturnsUpdatedValue()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "Robert",
                LastName = "Davis"
            };

            var initialResult = person.FullName;

            // Act
            person.FirstMidName = "Richard";
            person.LastName = "Grant";
            var updatedResult = person.FullName;

            // Assert
            Assert.Equal("Davis, Robert", initialResult);
            Assert.Equal("Grant, Richard", updatedResult);
            Assert.NotEqual(initialResult, updatedResult);
        }

        /// <summary>
        /// Test: Numeric characters in names
        /// Expected: Format should handle numeric characters (though unusual for names)
        /// </summary>
        [Fact]
        public void FullName_WithNumericCharactersInNames_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "John 3rd",
                LastName = "Smith 2nd"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Smith 2nd, John 3rd", result);
        }

        /// <summary>
        /// Test: Names with periods/dots
        /// Expected: Format should handle periods correctly
        /// </summary>
        [Fact]
        public void FullName_WithPeriodsInNames_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "J.R.",
                LastName = "Smith-Jones"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Smith-Jones, J.R.", result);
        }

        /// <summary>
        /// Test: International characters in names
        /// Expected: Format should handle accented and special characters
        /// </summary>
        [Fact]
        public void FullName_WithInternationalCharacters_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "José",
                LastName = "García"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("García, José", result);
        }

        /// <summary>
        /// Test: Verify exact format with comma and space separator
        /// Expected: Separator must be exactly ", " (comma followed by single space)
        /// </summary>
        [Fact]
        public void FullName_VerifyExactSeparatorFormat_HasCommaAndSpace()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "William",
                LastName = "Thompson"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Contains(", ", result);
            Assert.Equal("Thompson, William", result);
            
            // Verify the separator is exactly ", " by checking structure
            var parts = result.Split(", ");
            Assert.Equal(2, parts.Length);
            Assert.Equal("Thompson", parts[0]);
            Assert.Equal("William", parts[1]);
        }

        /// <summary>
        /// Test: FullName property is read-only
        /// Expected: Cannot assign a value directly to FullName property
        /// </summary>
        [Fact]
        public void FullName_IsComputedProperty_CannotBeSet()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "Susan",
                LastName = "Mitchell"
            };

            // Act & Assert
            // This test verifies the FullName property is computed (read-only)
            // by checking that it's a computed property, not settable
            var property = typeof(TestPerson).GetProperty(nameof(TestPerson.FullName));
            Assert.NotNull(property);
            Assert.False(property.CanWrite); // Ensure property cannot be written to
        }

        /// <summary>
        /// Test: Consistency across different Person-derived classes
        /// Expected: Both Person and Student should have same FullName behavior
        /// </summary>
        [Fact]
        public void FullName_WithStudentAndPersonSubclass_BothHaveSameBehavior()
        {
            // Arrange
            var student = new Student
            {
                FirstMidName = "Nancy",
                LastName = "Davolio",
                EnrollmentDate = new DateTime(2013, 9, 1)
            };

            var testPerson = new TestPerson
            {
                FirstMidName = "Nancy",
                LastName = "Davolio"
            };

            // Act
            var studentResult = student.FullName;
            var personResult = testPerson.FullName;

            // Assert
            Assert.Equal(studentResult, personResult);
            Assert.Equal("Davolio, Nancy", studentResult);
        }

        /// <summary>
        /// Test: Unicode characters in names
        /// Expected: Format should handle various Unicode characters
        /// </summary>
        [Fact]
        public void FullName_WithUnicodeCharacters_ReturnsFormattedString()
        {
            // Arrange
            var person = new TestPerson
            {
                FirstMidName = "François",
                LastName = "Müller"
            };

            // Act
            var result = person.FullName;

            // Assert
            Assert.Equal("Müller, François", result);
        }
    }

    /// <summary>
    /// Concrete implementation of the abstract Person class for testing purposes.
    /// Since Person is abstract, we need a concrete test class to instantiate it.
    /// </summary>
    public class TestPerson : Person
    {
        // No additional implementation needed - inherits all functionality from Person
    }
}
