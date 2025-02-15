using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models;
using System.ComponentModel.DataAnnotations;
[Table("Learning_path")] // Matches the database table name
public class LearningPath
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PathID { get; set; } // Matches pathID as the primary key

    [Required]
    public int LearnerID { get; set; } // Matches LearnerID, references Learner

    [Required]
    public int ProfileID { get; set; } // Matches ProfileID, references PersonalizationProfiles

    [Required(ErrorMessage = "Completion Status is required.")]
    [StringLength(50)]
    [Column("completion_status")]
    
    public string? CompletionStatus { get; set; } // Matches completion_status (VARCHAR(50))
    [Required(ErrorMessage = "Custom Content is required.")]
    [Column("custom_content")]
    public string? CustomContent { get; set; } // Matches custom_content (VARCHAR(MAX))
    [Required(ErrorMessage = "Adaptive Rules are required.")]
    [Column("adaptive_rules")]
    public string? AdaptiveRules { get; set; } // Matches adaptive_rules (VARCHAR(MAX))

    // Navigation property for the PersonalizationProfiles table
    public PersonalizationProfiles PersonalizationProfiles { get; set; }
}
