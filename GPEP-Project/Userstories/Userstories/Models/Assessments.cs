using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models;

[Table("Assessments")] // Matches the database table name
public class Assessments
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ID { get; set; } // Primary Key for Assessments

    [Required(ErrorMessage = "Module ID is required.")]
    public int ModuleID { get; set; } // Matches ModuleID, references Modules

    [Required(ErrorMessage = "Course ID is required.")]
    public int CourseID { get; set; } // Matches CourseID, references Courses

    [Required(ErrorMessage = "Assessment type is required.")]
    [StringLength(50)]
    [Column("type")]
    public string? Type { get; set; } // Matches type (VARCHAR(50))

    [Required(ErrorMessage = "Total Marks are required.")]
    [Column("total_marks")]
    public int TotalMarks { get; set; } // Matches total_marks (INT)

    [Required(ErrorMessage = "Passing Marks are required.")]
    [Column("passing_marks")]
    public int PassingMarks { get; set; } // Matches passing_marks (INT)

    [StringLength(250)]
    [Column("criteria")]
    [Required(ErrorMessage = "Criteria is required.")]
    public string? Criteria { get; set; } // Matches criteria (VARCHAR(250))

    [Required(ErrorMessage = "Weightage is required.")]
    [Column("weightage")]
    public double? Weightage { get; set; }

    [StringLength(500)]
    [Column("description")]
    [Required(ErrorMessage = "Description is required.")]
    public string? Description { get; set; } // Matches description (VARCHAR(500))

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(100)]
    [Column("title")]
    public string? Title { get; set; } // Matches title (VARCHAR(100))

    // Navigation property
    [ForeignKey("ModuleID, CourseID")]
    public Modules Module { get; set; }
    public ICollection<TakenAssessments>? TakenAssessments { get; set; } // Navigation property for TakenAssessments
}