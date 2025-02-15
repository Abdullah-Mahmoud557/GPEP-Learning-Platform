using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models;

[Table("Taken_assessments")] // Matches the database table name
public class TakenAssessments
{
    [Key]
    [Required]
    [Column(Order = 1)]
    public int LearnerID { get; set; } // Matches LearnerID, references Learner

    [Key]
    [Required]
    [Column(Order = 2)]
    public int AssessmentID { get; set; } // Matches AssessmentID, references Assessments

    [Required(ErrorMessage = "Scored Points are required.")]
    [Column("Scored_points")]
    public int ScoredPoints { get; set; } // Matches Scored_points (INT)

    // Navigation properties for relationships
    public Learner? Learner { get; set; } // Navigation property for Learner
    [ForeignKey(nameof(AssessmentID))]
    public Assessments? Assessment { get; set; } // Navigation property for Assessments
}