using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Userstories.Models;

public class PersonalizationProfiles
{
    [Key, Column(Order = 0)]
    public int LearnerID { get; set; } // Matches the database column [LearnerID]

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto-increment
    public int ProfileID { get; set; } // Matches the database column [ProfileID]
    [Required(ErrorMessage = "Preferred Content Type is required.")]
    [Column("preferred_content_type", TypeName = "VARCHAR(50)")]
    public string PreferredContentType { get; set; } // Matches the database column [preferred_content_type]

    [Required(ErrorMessage = "Emotional State is required.")]
    [Column("emotional_state", TypeName = "VARCHAR(50)")]
    public string EmotionalState { get; set; } // Matches the database column [emotional_state]
[Required(ErrorMessage = "Personality Type is required.")]
    [Column("personality_type", TypeName = "VARCHAR(50)")]
    public string PersonalityType { get; set; } // Matches the database column [personality_type]

    // Navigation property
    [ForeignKey("LearnerID")]
    public Learner Learner { get; set; } // Matches the foreign key relationship
}